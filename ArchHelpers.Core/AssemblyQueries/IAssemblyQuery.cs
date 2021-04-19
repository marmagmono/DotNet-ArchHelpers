using System;
using System.Collections.Generic;
using ArchHelpers.Core.Utilities.LabeledTree;

namespace ArchHelpers.Core.AssemblyQueries
{
    public interface IAssemblyQuery
    {
        IEnumerable<Type> GetAllTypes();

        IEnumerable<Type> GetAllTypesMatching(Func<Type, bool> acceptType);

        NodePath[] GetNamespaces();

        ILabeledTreeNode<Type[]> GetTypesByNamespaces();
    }
}