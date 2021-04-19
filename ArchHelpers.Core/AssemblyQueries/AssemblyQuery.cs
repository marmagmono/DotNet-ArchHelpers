using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArchHelpers.Core.TypeCollections;
using ArchHelpers.Core.Utilities.LabeledTree;
using ArchHelpers.Core.Utilities.LabeledTree.Visitors;

namespace ArchHelpers.Core.AssemblyQueries
{
    using ITypeCollection = ILabeledTreeNode<Type[]>;

    public class AssemblyQuery : IAssemblyQuery
    {
        private readonly Assembly _assembly;
        private ITypeCollection? _typeCollection;

        public AssemblyQuery(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<Type> GetAllTypes()
        {
            static bool IsNotCompilerGenerated(Type type) => type.Name.StartsWith('<') == false;

            return _assembly.GetTypes().Where(IsNotCompilerGenerated).ToArray();
        }

        public IEnumerable<Type> GetAllTypesMatching(Func<Type, bool> acceptType)
        {
            return GetAllTypes().Where(acceptType);
        }

        public NodePath[] GetNamespaces()
        {
            EnsureTypeCollection();

            var pathCollector = new TypeCollectionPathCollector();
            var collectionWalker = new LabeledTreeWalker();
            collectionWalker.WalkTree(_typeCollection!, pathCollector);

            return pathCollector.Result;
        }

        public ITypeCollection GetTypesByNamespaces()
        {
            EnsureTypeCollection();

            return _typeCollection!;
        }

        private void EnsureTypeCollection()
        {
            if (_typeCollection == null)
            {
                _typeCollection = BuildNamespacesHierarchy(_assembly);
            }
        }

        private ITypeCollection BuildNamespacesHierarchy(Assembly assembly)
        {
            var collectionFactory = new TypeCollectionFactory();
            return collectionFactory.CreateFrom(GetAllTypes());
        }

        private class TypeCollectionPathCollector : ILabeledTreeNodeVisitor<Type[]>
        {
            private readonly List<NodePath> paths = new();

            public NodePath[] Result => paths.ToArray();

            public void Visit(ITypeCollection typeCollection)
            {
                if (typeCollection.GetChildren().Length > 0)
                {
                    paths.Add(typeCollection.Path);
                }
            }
        }
    }
}