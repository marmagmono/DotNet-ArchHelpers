namespace ArchHelpers.Core.Utilities.LabeledTree.Visitors
{
    public interface ILabeledTreeNodeExtendedVisitor<TData>
    {
        void StartVisit(ILabeledTreeNode<TData> node);

        void EndVisit(ILabeledTreeNode<TData> node);
    }
}