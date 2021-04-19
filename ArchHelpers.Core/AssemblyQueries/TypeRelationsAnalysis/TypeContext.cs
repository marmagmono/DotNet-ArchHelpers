using System;
using System.Collections.Generic;
using ArchHelpers.Core.TypeQueries;

namespace ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis
{
    public record TypeContext(
        Type Type,
        IReadOnlyList<TypeRelation> Uses,
        IReadOnlyList<TypeRelation> UsedBy);
}