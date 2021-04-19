using System;
using System.Threading.Tasks;

namespace ArchHelpers.Core.Utilities.LabeledTree.Visitors
{
    public class LabeledTreeWalker
    {
        public void WalkTree<TData>(ILabeledTreeNode<TData> rootNode, Action<ILabeledTreeNode<TData>> visitAction) =>
            WalkTree(rootNode, new DelegatingVisitor<TData>(visitAction));

        public void WalkTree<TData>(ILabeledTreeNode<TData> rootNode, ILabeledTreeNodeVisitor<TData> treeNodeVisitor)
        {
            treeNodeVisitor.Visit(rootNode);

            foreach (var subCollection in rootNode.GetChildren())
            {
                WalkTree(subCollection, treeNodeVisitor);
            }
        }

        public void WalkTree<TData>(ILabeledTreeNode<TData> rootNode, ILabeledTreeNodeExtendedVisitor<TData> treeNodeVisitor)
        {
            treeNodeVisitor.StartVisit(rootNode);

            foreach (var subCollection in rootNode.GetChildren())
            {
                WalkTree(subCollection, treeNodeVisitor);
            }

            treeNodeVisitor.EndVisit(rootNode);
        }

        public async Task WalkTree<TData>(ILabeledTreeNode<TData> rootNode, ILabeledTreeNodeAsyncExtendedVisitor<TData> treeNodeVisitor)
        {
            await treeNodeVisitor.StartVisit(rootNode);

            foreach (var subCollection in rootNode.GetChildren())
            {
                await WalkTree(subCollection, treeNodeVisitor);
            }

            await treeNodeVisitor.EndVisit(rootNode);
        }

        private class DelegatingVisitor<TData> : ILabeledTreeNodeVisitor<TData>
        {
            private readonly Action<ILabeledTreeNode<TData>> _visitAction;

            public DelegatingVisitor(Action<ILabeledTreeNode<TData>> visitAction)
            {
                _visitAction = visitAction;
            }

            public void Visit(ILabeledTreeNode<TData> typeCollection) => _visitAction(typeCollection);
        }
    }
}