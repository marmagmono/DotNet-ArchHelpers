namespace ArchHelpers.Core.TypeQueries
{
    public class TypeQueryOptions
    {
        public bool IncludeNonPublic { get; set; } = true;

        public bool IncludeStatic { get; set; } = false;

        public bool IncludeInherited { get; set; } = false;
    }

    public interface ITypeQueryFactory
    {
        IQueryType CreateTypeQuery(TypeQueryOptions queryOptions);
    }

    public class TypeQueryFactory : ITypeQueryFactory
    {
        public IQueryType CreateTypeQuery(TypeQueryOptions queryOptions)
        {
            return new QueryType(queryOptions);
        }
    }
}