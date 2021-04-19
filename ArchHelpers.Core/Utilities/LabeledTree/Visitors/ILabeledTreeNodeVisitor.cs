namespace ArchHelpers.Core.Utilities.LabeledTree.Visitors
{
    public interface ILabeledTreeNodeVisitor<TData>
    {
        void Visit(ILabeledTreeNode<TData> node);
    }
}