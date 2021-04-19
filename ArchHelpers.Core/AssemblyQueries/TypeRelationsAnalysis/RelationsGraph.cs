using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis.Builders;
using ArchHelpers.Core.Utilities.LabeledTree;

namespace ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis
{
    public readonly struct FilterRelationOptions
    {
        /// <summary>
        /// Regex that specifies which class will be the root of search for relations.
        /// </summary>
        public string SearchRootClassNameRegex { get; }

        /// <summary>
        /// Depth of the search. 0 means that only classes captured by <see cref="SearchRootClassNameRegex"/> will be shown.
        /// </summary>
        public int SearchDepth { get; }

        /// <summary>
        /// Function used for filtering dependencies that should appear in the final relation tree.
        /// </summary>
        public Func<Type, bool>? DependencyFilter { get; }

        public FilterRelationOptions(string searchRootClassNameRegex, int searchDepth, Func<Type, bool>? dependencyFilter = null)
        {
            SearchRootClassNameRegex = searchRootClassNameRegex;
            SearchDepth = searchDepth;
            DependencyFilter = dependencyFilter;
        }
    }

    public class RelationsGraph
    {
        private readonly Dictionary<Type, TypeContext> _typeContexts;

        public RelationsGraph(Dictionary<Type, TypeContext> typeContexts)
        {
            _typeContexts = typeContexts;
        }

        public RelationsGraph Filter(FilterRelationOptions options)
        {
            void AddTypeContextToQueue(Queue<TypeContext> searchTypes, HashSet<Type> visitedTypes, Type type)
            {
                if (visitedTypes.Contains(type))
                {
                    return;
                }

                searchTypes.Enqueue(_typeContexts[type]);
            }

            Regex re = new(
                options.SearchRootClassNameRegex,
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

            Func<Type, bool> acceptAllDependencies = _ => true;
            Func<Type, bool> acceptDependency = options.DependencyFilter ?? acceptAllDependencies;

            // Initialize root search types
            var searchTypes = new Queue<TypeContext>();
            var visitedTypes = new HashSet<Type>();
            foreach (var typeContext in _typeContexts.Values)
            {
                if (typeContext.Type.FullName != null && re.IsMatch(typeContext.Type.FullName))
                {
                    searchTypes.Enqueue(typeContext);
                }
            }

            // Perform search
            for (int i = 0; i <= options.SearchDepth; i++)
            {
                int numTypesAtDepth = searchTypes.Count;
                for (int typeNum = 0; typeNum < numTypesAtDepth; typeNum++)
                {
                    var typeContext = searchTypes.Dequeue();
                    visitedTypes.Add(typeContext.Type);

                    foreach (var typeRelation in typeContext.Uses)
                    {
                        var typeToAdd = typeRelation.Dependency;
                        if (acceptDependency(typeToAdd))
                        {
                            AddTypeContextToQueue(searchTypes, visitedTypes, typeToAdd);
                        }
                    }

                    foreach (var typeRelation in typeContext.UsedBy)
                    {
                        var typeToAdd = typeRelation.Type;
                        if (acceptDependency(typeToAdd))
                        {
                            AddTypeContextToQueue(searchTypes, visitedTypes, typeToAdd);
                        }
                    }
                }
            }

            // Build filtered graph.
            var filteredDict = new Dictionary<Type, TypeContext>();
            foreach (var type in visitedTypes)
            {
                filteredDict[type] = _typeContexts[type];
            }

            return new RelationsGraph(filteredDict);
        }

        public IEnumerable<TypeContext> GetTypeContexts() => _typeContexts.Values;

        public ILabeledTreeNode<TypeContext[]> AsNamespaceTree()
        {
            var treeFactory = new TypeContextTreeFactory();
            return treeFactory.CreateTreeFrom(_typeContexts.Values);
        }
    }
}