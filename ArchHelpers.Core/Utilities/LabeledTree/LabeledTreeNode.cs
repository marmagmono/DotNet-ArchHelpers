namespace ArchHelpers.Core.Utilities.LabeledTree
{
    public class LabeledTreeNode<TData> : ILabeledTreeNode<TData>
    {
        private readonly NodePath _path;

        private readonly TData _nodeData;
        private readonly LabeledTreeNode<TData>[] _children;

        public LabeledTreeNode(NodePath path, TData nodeData, LabeledTreeNode<TData>[] children)
        {
            _path = path;
            _nodeData = nodeData;
            _children = children;
        }

        public NodePath Path => _path;

        public TData Data => _nodeData;

        public LabeledTreeNode<TData>[] GetChildren() => _children;
    }
}