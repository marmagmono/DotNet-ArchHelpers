using System;
using System.Collections.Generic;
using System.Linq;
using ArchHelpers.Core.TypeQueries;
using ArchHelpers.Core.Utilities;

namespace ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis.Builders
{
    public enum RelationGraphBuilderOptions
    {
        GetAllTypes,
        ExcludeSystemTypes
    }

    public class RelationGraphBuilder
    {
        public RelationsGraph BuildTypeRelationGraph(IEnumerable<Type> types, RelationGraphBuilderOptions options)
        {
            bool IsNotSystemType(Type type) => type.Namespace != "System" && type.Namespace?.StartsWith("System.") == false;

            return options switch
            {
                RelationGraphBuilderOptions.GetAllTypes => BuildTypeRelationGraph(types, acceptDependencyType: _ => true),
                RelationGraphBuilderOptions.ExcludeSystemTypes => BuildTypeRelationGraph(types, IsNotSystemType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private RelationsGraph BuildTypeRelationGraph(IEnumerable<Type> types, Func<Type, bool> acceptDependencyType)
        {
            var knownRelations = new Dictionary<Type, TypeContextBuilder>();

            var queryType = new QueryType(new TypeQueryOptions());
            foreach (var analysedType in types)
            {
                var typeRelations = queryType.GetDependencies(analysedType, acceptDependencyType);
                knownRelations.CreateOrUpdateValue(
                    analysedType,
                    () => new TypeContextBuilder(analysedType).AddUses(typeRelations),
                    builder => builder.AddUses(typeRelations));

                foreach (var typeRelation in typeRelations)
                {
                    knownRelations.CreateOrUpdateValue(
                        typeRelation.Dependency,
                        () => new TypeContextBuilder(typeRelation.Dependency).AddUsedBy(typeRelation),
                        builder => builder.AddUsedBy(typeRelation));
                }
            }

            var builtTypeContexts = knownRelations
                .ToDictionary(kv => kv.Key,
                    kv => kv.Value.Build());
            return new RelationsGraph(builtTypeContexts);
        }

        private class TypeContextBuilder
        {
            private readonly Type _type;
            private readonly List<TypeRelation> _uses = new();
            private readonly List<TypeRelation> _usedBy = new();

            public TypeContextBuilder(Type type)
            {
                _type = type;
            }

            public TypeContextBuilder AddUsedBy(TypeRelation relation)
            {
                _usedBy.Add(relation);
                return this;
            }

            public TypeContextBuilder AddUses(IEnumerable<TypeRelation> uses)
            {
                _uses.AddRange(uses);
                return this;
            }

            public TypeContext Build() => new TypeContext(_type, _uses, _usedBy);
        }
    }
}