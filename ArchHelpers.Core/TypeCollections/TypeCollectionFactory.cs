using System;
using System.Collections.Generic;
using ArchHelpers.Core.Utilities.LabeledTree;
using ArchHelpers.Core.Utilities.LabeledTree.Builders;

namespace ArchHelpers.Core.TypeCollections
{
    public class TypeCollectionFactory
    {
        public ILabeledTreeNode<Type[]> CreateFrom(IEnumerable<Type> types)
        {
            var typeCollectionBuilders = new Dictionary<NodePath, LabeledArrayTreeNodeBuilder<Type>>();
            var (rootCollectionBuilder, _) = GetOrCreateBuilder(typeCollectionBuilders, NodePath.RootPath);
            foreach (var type in types)
            {
                var path = new NodePath(type.Namespace);
                var (builder, _) = GetOrCreateBuilder(typeCollectionBuilders, path);
                builder.TryAddType(type);

                UpdateParentCollections(typeCollectionBuilders, builder);
            }

            return rootCollectionBuilder.Build();
        }

        private static (LabeledArrayTreeNodeBuilder<Type> builder, bool created) GetOrCreateBuilder(
            Dictionary<NodePath, LabeledArrayTreeNodeBuilder<Type>>dict,
            NodePath path)
        {
            if (dict.TryGetValue(path, out var typeCollectionBuilder))
            {
                return (typeCollectionBuilder, false);
            }

            var builder = new LabeledArrayTreeNodeBuilder<Type>(path);
            dict.Add(path, builder);
            return (builder, true);
        }

        private static void UpdateParentCollections(
            Dictionary<NodePath, LabeledArrayTreeNodeBuilder<Type>> dict,
            LabeledArrayTreeNodeBuilder<Type> builder)
        {
            var parentPath = builder.Path.GetParentPath();
            if (parentPath == null)
            {
                return;
            }

            var (parentBuilder, parentCreated) = GetOrCreateBuilder(dict, parentPath.Value);
            parentBuilder.TryAddOwnedCollection(builder);

            if (parentCreated)
            {
                UpdateParentCollections(dict, parentBuilder);
            }
        }
    }
}