using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis;
using ArchHelpers.Core.Converters;
using ArchHelpers.Core.TypeQueries;
using ArchHelpers.Core.Utilities.LabeledTree;
using ArchHelpers.Core.Utilities.LabeledTree.Visitors;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml
{
    public class PlantumlExporter : IPlantumlExporter
    {
        private readonly ITypeWriter _typeWriter;
        private readonly ITypeNameConverter _escapedTypeNameConverter;

        public PlantumlExporter(
            ITypeWriter typeWriter,
            ITypeNameConverter escapedTypeNameConverter)
        {
            _typeWriter = typeWriter;
            _escapedTypeNameConverter = escapedTypeNameConverter;
        }

        public async Task CreateClassDiagramFile(string fileName, ILabeledTreeNode<TypeContext[]> typeCollection)
        {
            await using var fileWriter = new StreamWriter(fileName, false);
            await WritePlantumlHeader(fileWriter);

            var collectionWalker = new LabeledTreeWalker();

            // Write class hierarchy
            var classHierarchyWriter = new TypeHierarchyWriter(_typeWriter, fileWriter);
            await collectionWalker.WalkTree(typeCollection, classHierarchyWriter);

            // Write type relations
            var relationWriter = new TypeRelationsWriter(_escapedTypeNameConverter, fileWriter, classHierarchyWriter.WrittenTypes);
            await collectionWalker.WalkTree(typeCollection, relationWriter);

            await WritePlantumlFooter(fileWriter);
        }

        private static async Task WritePlantumlHeader(TextWriter writer) => await writer.WriteLineAsync("@startuml Something");

        private static async Task WritePlantumlFooter(TextWriter writer) => await writer.WriteLineAsync("@enduml");

        private class TypeHierarchyWriter : ILabeledTreeNodeAsyncExtendedVisitor<TypeContext[]>
        {
            private readonly ITypeWriter _typeWriter;
            private readonly TextWriter _writer;

            public HashSet<Type> WrittenTypes { get; } = new HashSet<Type>();

            public TypeHierarchyWriter(ITypeWriter typeWriter, TextWriter writer)
            {
                _typeWriter = typeWriter;
                _writer = writer;
            }

            public async Task StartVisit(ILabeledTreeNode<TypeContext[]> node)
            {
                if (node.Path.Equals(NodePath.RootPath))
                {
                    return;
                }

                var pathLabel = node.Path.GetNodeLabel();
                await _writer.WriteLineAsync($"package {pathLabel} {{");

                foreach (var typeContext in node.Data)
                {
                    WrittenTypes.Add(typeContext.Type);
                    _typeWriter.Write(_writer, typeContext.Type);
                }
            }

            public async Task EndVisit(ILabeledTreeNode<TypeContext[]> node)
            {
                if (node.Path.Equals(NodePath.RootPath))
                {
                    return;
                }

                await _writer.WriteLineAsync($"}}\n");
            }
        }

        private class TypeRelationsWriter : ILabeledTreeNodeAsyncExtendedVisitor<TypeContext[]>
        {
            private readonly ITypeNameConverter _escapedTypeNameConverter;
            private readonly TextWriter _writer;
            private readonly HashSet<Type> _typesToWrite;

            public TypeRelationsWriter(
                ITypeNameConverter escapedTypeNameConverter,
                TextWriter writer,
                HashSet<Type> typesToWrite)
            {
                _escapedTypeNameConverter = escapedTypeNameConverter;
                _writer = writer;
                _typesToWrite = typesToWrite;
            }

            public async Task StartVisit(ILabeledTreeNode<TypeContext[]> node)
            {
                static string RelationKindToArrow(RelationKind relationKind) => relationKind switch
                {
                    RelationKind.Extends => "<|--",
                    RelationKind.Implements => "<|..",
                    _ => "-->"
                };

                foreach (var typeContext in node.Data)
                {
                    foreach (var typeRelation in typeContext.Uses)
                    {
                        if (!_typesToWrite.Contains(typeRelation.Dependency))
                        {
                            continue;;
                        }

                        if (typeRelation.RelationKind == RelationKind.ParameterUse)
                        {
                            continue;
                        }

                        if (typeRelation.RelationKind == RelationKind.Implements ||
                            typeRelation.RelationKind == RelationKind.Extends)
                        {
                            await _writer.WriteAsync(_escapedTypeNameConverter.ToString(typeRelation.Dependency));
                            await _writer.WriteAsync($" {RelationKindToArrow(typeRelation.RelationKind)} ");
                            await _writer.WriteAsync(_escapedTypeNameConverter.ToString(typeRelation.Type));
                            await _writer.WriteAsync(_writer.NewLine);
                        }
                        else
                        {
                            await _writer.WriteAsync(_escapedTypeNameConverter.ToString(typeRelation.Type));
                            await _writer.WriteAsync($" {RelationKindToArrow(typeRelation.RelationKind)} ");
                            await _writer.WriteAsync(_escapedTypeNameConverter.ToString(typeRelation.Dependency));
                            await _writer.WriteAsync(_writer.NewLine);
                        }

                    }
                }
            }

            public Task EndVisit(ILabeledTreeNode<TypeContext[]> node) => Task.CompletedTask;
        }
    }
}