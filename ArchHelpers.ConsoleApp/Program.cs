using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchHelpers.Core.AssemblyQueries;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis.Builders;
using ArchHelpers.Core.Context;
using ArchHelpers.Core.Converters;
using ArchHelpers.Core.TypeQueries;
using ArchHelpers.Core.Utilities.LabeledTree.Visitors;
using ArchHelpers.Core.Writers;
using ArchHelpers.Core.Writers.SimpleImplementations;
using ArchHelpers.Exports.Plantuml;
using ArchHelpers.Exports.Plantuml.Writers.DevNull;

namespace ArchHelpers.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var context = MetadataContextFactory.CreateContextForDirectories(args[0]);
            var orleansRuntimeAssembly = context.LoadFromName("Orleans.Runtime");

            await ProgramFlow(args[1], orleansRuntimeAssembly, "^Orleans.Runtime.GrainDirectory.", 2);
            //PlayWithAssembly(orleansRuntimeAssembly);
            //await ExportToPlantUml1(args[1], orleansRuntimeAssembly);
            //PrintTypesInAssembly(orleansRuntimeAssembly);
        }

        private static async Task ProgramFlow(string outputFileName, Assembly assembly, string classNameRegex, int searchDepth)
        {
            static ITypeWriter CreatePlantUmlTypeWriter()
            {
                bool writeMembers = true;

                var queryType = new QueryType(new TypeQueryOptions());
                var normalTypeNameConverter = new SimpleTypeNameConverter();
                var escapingTypeNameConverter = new EscapingTypenameConverter();

                var normalTypeNameWriter = new SimpleTypeNameWriter(normalTypeNameConverter);
                var escapedTypeNameWriter = new SimpleTypeNameWriter(escapingTypeNameConverter);
                IPropertyWriter propertyWriter = writeMembers ? new PropertyWriter(normalTypeNameWriter) : new EmptyPropertyWriter();
                IMethodWriter methodWriter = writeMembers ? new MethodWriter(normalTypeNameConverter, normalTypeNameWriter) : new EmptyMethodWriter();
                IFieldWriter fieldWriter = writeMembers ? new FieldWriter(normalTypeNameWriter) : new EmptyFieldWriter();
                IEventWriter eventWriter = writeMembers ? new EventWriter(normalTypeNameWriter) : new EmptyEventWriter();

                return new PlantUmlTypeWriter(
                    queryType,
                    normalTypeNameWriter,
                    escapedTypeNameWriter,
                    eventWriter,
                    methodWriter,
                    fieldWriter,
                    propertyWriter);
            }

            IEnumerable<Type> LoadTypes()
            {
                var re = new Regex(classNameRegex, RegexOptions.Compiled);

                var aq = new AssemblyQuery(assembly);
                return aq.GetAllTypes(); //aq.GetAllTypesMatching(t => t.FullName != null && re.IsMatch(t.FullName));
            }

            RelationsGraph BuildRelationGraph(IEnumerable<Type> types)
            {
                var rgb = new RelationGraphBuilder();
                var fullGraph = rgb.BuildTypeRelationGraph(types, RelationGraphBuilderOptions.ExcludeSystemTypes);

                Regex re = new(classNameRegex);
                return fullGraph.Filter(
                    new FilterRelationOptions(
                        classNameRegex,
                        searchDepth, t =>
                            (t.FullName != null && re.IsMatch(t.FullName))));
            }

            async Task ExportToPlantUml(RelationsGraph relationsGraph)
            {
                var exporter = new PlantumlExporter(CreatePlantUmlTypeWriter(), new EscapingTypenameConverter());

                var relationsInNamespaces = relationsGraph.AsNamespaceTree();

                await exporter.CreateClassDiagramFile(outputFileName, relationsInNamespaces);
            }

            var types = LoadTypes();
            var relationGraph = BuildRelationGraph(types);
            await ExportToPlantUml(relationGraph);

        }

        private static void PlayWithAssembly(Assembly assembly)
        {
            var typeNameConverter = new SimpleTypeNameConverter();
            var query = new AssemblyQuery(assembly);
            var namespaces = query.GetTypesByNamespaces();
            var namespacesWalker = new LabeledTreeWalker();
            namespacesWalker.WalkTree(
                namespaces,
                collection =>
                {
                    var numTypes = collection.Data.Length;
                    if (numTypes <= 0)
                    {
                        return;
                    }

                    Console.WriteLine("============================================");
                    Console.WriteLine($"{collection.Path} -> {numTypes}");
                    var types = collection.Data;
                    foreach (var type in types)
                    {
                        Console.WriteLine(typeNameConverter.ToString(type));
                    }
                });
        }

        private static async Task ExportToPlantUml1(string filePath, Assembly assembly)
        {
            var assemblyQuery = new AssemblyQuery(assembly);
            var types = assemblyQuery.GetTypesByNamespaces();

            bool writeMembers = true;

            var typeNameConverter = new SimpleTypeNameConverter();
            var escapingTypeNameConverter = new EscapingTypenameConverter();
            var typeNameWriter = new SimpleTypeNameWriter(typeNameConverter);
            var typeQueryFactory = new TypeQueryFactory();
            IPropertyWriter propertyWriter = writeMembers ? new PropertyWriter(typeNameWriter) : new EmptyPropertyWriter();
            IMethodWriter methodWriter = writeMembers ? new MethodWriter(typeNameConverter, typeNameWriter) : new EmptyMethodWriter();
            IFieldWriter fieldWriter = writeMembers ? new FieldWriter(typeNameWriter) : new EmptyFieldWriter();
            IEventWriter eventWriter = writeMembers ? new EventWriter(typeNameWriter) : new EmptyEventWriter();

            var escapingTypeNameWriter = new SimpleTypeNameWriter(escapingTypeNameConverter);
            // var typeWriter = new PlantUmlTypeWriter(typeQueryFactory,
            //     escapingTypeNameWriter,
            //     escapingTypeNameConverter,
            //     eventWriter,
            //     methodWriter,
            //     fieldWriter,
            //     propertyWriter);

            // var exporter = new PlantumlExporter(typeWriter);
            //await exporter.CreateClassDiagramFile(filePath, types);
        }

        private static async Task ExportToPlantUml(string filePath, Assembly assembly)
        {
            var queryOptions = new TypeQueryOptions()
            {
                IncludeNonPublic = true,
            };

            var typeNameConverter = new SimpleTypeNameConverter();
            var escapingTypeNameConverter = new EscapingTypenameConverter();

            var typeNameWriter = new SimpleTypeNameWriter(typeNameConverter);
            var typeQueryFactory = new TypeQueryFactory();
            var propertyWriter = new PropertyWriter(typeNameWriter);
            var methodWriter = new MethodWriter(typeNameConverter, typeNameWriter);
            var fieldWriter = new FieldWriter(typeNameWriter);
            var eventWriter = new EventWriter(typeNameWriter);

            var escapingTypeNameWriter = new SimpleTypeNameWriter(escapingTypeNameConverter);
            // var typeWriter = new PlantUmlTypeWriter(typeQueryFactory,
            //     escapingTypeNameWriter,
            //     escapingTypeNameConverter,
            //     eventWriter,
            //     methodWriter,
            //     fieldWriter,
            //     propertyWriter);

            // var exporter = new PlantumlExporter(typeWriter);

            var types = assembly.GetTypes().Where(t => !t.Name.StartsWith("<")).ToArray();
            //await exporter.CreateClassDiagramFile(filePath, types);
        }

        private static void PrintTypesInAssembly(Assembly assembly)
        {
            var queryOptions = new TypeQueryOptions()
            {
                IncludeNonPublic = true,
            };

            var typeNameConverter = new SimpleTypeNameConverter();
            var typeNameWriter = new SimpleTypeNameWriter(typeNameConverter);
            var typeQueryFactory = new TypeQueryFactory();
            var propertyWriter = new PropertyWriter(typeNameWriter);
            var methodWriter = new MethodWriter(typeNameConverter, typeNameWriter);
            var fieldWriter = new FieldWriter(typeNameWriter);
            var eventWriter = new EventWriter(typeNameWriter);
            var typeWriter = new SimpleTypeWriter(
                typeQueryFactory,
                typeNameWriter,
                typeNameConverter,
                eventWriter,
                methodWriter,
                fieldWriter,
                propertyWriter);

            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.StartsWith("<"))
                {
                    continue;
                }

                var typeQuery = typeQueryFactory.CreateTypeQuery(queryOptions);
                typeWriter.Write(Console.Out, type, typeQuery);
            }
        }
    }
}