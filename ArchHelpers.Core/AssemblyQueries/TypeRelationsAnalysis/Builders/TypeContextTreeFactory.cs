using System.Collections.Generic;
using ArchHelpers.Core.Utilities.LabeledTree;
using ArchHelpers.Core.Utilities.LabeledTree.Builders;

namespace ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis.Builders
{
    public class TypeContextTreeFactory
    {
        public ILabeledTreeNode<TypeContext[]> CreateTreeFrom(IEnumerable<TypeContext> typeContexts)
        {
            var typeCollectionBuilders = new Dictionary<NodePath, LabeledArrayTreeNodeBuilder<TypeContext>>();
            var (rootCollectionBuilder, _) = GetOrCreateBuilder(typeCollectionBuilders, NodePath.RootPath);
            foreach (var typeContext in typeContexts)
            {
                var path = new NodePath(typeContext.Type.Namespace);
                var (builder, _) = GetOrCreateBuilder(typeCollectionBuilders, path);
                builder.TryAddType(typeContext);

                UpdateParentCollections(typeCollectionBuilders, builder);
            }

            return rootCollectionBuilder.Build();
        }

        private static (LabeledArrayTreeNodeBuilder<TypeContext> builder, bool created) GetOrCreateBuilder(
            Dictionary<NodePath, LabeledArrayTreeNodeBuilder<TypeContext>>dict,
            NodePath path)
        {
            if (dict.TryGetValue(path, out var typeCollectionBuilder))
            {
                return (typeCollectionBuilder, false);
            }

            var builder = new LabeledArrayTreeNodeBuilder<TypeContext>(path);
            dict.Add(path, builder);
            return (builder, true);
        }

        private static void UpdateParentCollections(
            Dictionary<NodePath, LabeledArrayTreeNodeBuilder<TypeContext>> dict,
            LabeledArrayTreeNodeBuilder<TypeContext> builder)
        {
            var parentPath = builder.Path.GetParentPath();
            if (parentPath == null)
            {
                return;
            }

            var (parentBuilder, parentCreated) = GetOrCreateBuilder(dict, parentPath.Value);
            parentBuilder.TryAddOwnedCollection(builder);

            // Check if it is necessary to create parent nodes till root is reached.
            if (parentCreated)
            {
                UpdateParentCollections(dict, parentBuilder);
            }
        }
    }
}