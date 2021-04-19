using System.Collections.Generic;
using System.Linq;

namespace ArchHelpers.Core.Utilities.LabeledTree.Builders
{
    internal class LabeledArrayTreeNodeBuilder<TData>
    {
        private readonly List<TData> _data = new();
        private readonly List<LabeledArrayTreeNodeBuilder<TData>> _children = new();

        public LabeledArrayTreeNodeBuilder(string path)
        {
            Path = path;
        }

        public LabeledArrayTreeNodeBuilder(NodePath path)
        {
            Path = path;
        }

        public NodePath Path { get; }

        public void TryAddType(TData type)
        {
            if (_data.Contains(type))
            {
                return;
            }

            _data.Add(type);
        }

        public void TryAddOwnedCollection(LabeledArrayTreeNodeBuilder<TData> collectionBuilder)
        {
            if (_children.Any(sc => sc.Path.Equals(collectionBuilder.Path)))
            {
                return;
            }

            _children.Add(collectionBuilder);
        }

        public LabeledTreeNode<TData[]> Build()
        {
            var subCollections = _children.Select(c => c.Build());
            return new LabeledTreeNode<TData[]>(Path, _data.ToArray(), subCollections.ToArray());
        }
    }
}