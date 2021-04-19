using System;
using System.Threading.Tasks;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis;
using ArchHelpers.Core.Utilities.LabeledTree;

namespace ArchHelpers.Exports.Plantuml
{
    public interface IPlantumlExporter
    {
        public Task CreateClassDiagramFile(string fileName, ILabeledTreeNode<TypeContext[]> typeTree);
    }
}