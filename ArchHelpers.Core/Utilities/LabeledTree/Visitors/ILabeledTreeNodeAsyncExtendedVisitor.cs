using System.Threading.Tasks;

namespace ArchHelpers.Core.Utilities.LabeledTree.Visitors
{
    public interface ILabeledTreeNodeAsyncExtendedVisitor<TData>
    {
        Task StartVisit(ILabeledTreeNode<TData> node);

        Task EndVisit(ILabeledTreeNode<TData> node);
    }
}