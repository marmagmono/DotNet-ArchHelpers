using System;

namespace ArchHelpers.Core.TypeQueries
{
    public enum RelationKind
    {
        Implements,
        Extends,
        Uses,
        ParameterUse,
    }

    public record TypeRelation(Type Type, Type Dependency, RelationKind RelationKind);
}