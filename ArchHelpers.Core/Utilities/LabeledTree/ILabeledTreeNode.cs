namespace ArchHelpers.Core.Utilities.LabeledTree
{
    public interface ILabeledTreeNode<TData>
    {
        NodePath Path { get; }

        TData Data { get; }

        LabeledTreeNode<TData>[] GetChildren();
    }
}