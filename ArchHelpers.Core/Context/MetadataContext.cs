using System.Reflection;

namespace ArchHelpers.Core.Context
{
    internal sealed class MetadataContext : IMetadataContext
    {
        private readonly PathAssemblyResolver _pathAssemblyResolver;
        private readonly MetadataLoadContext _metadataLoadContext;

        public MetadataContext(PathAssemblyResolver pathAssemblyResolver, MetadataLoadContext metadataLoadContext)
        {
            _pathAssemblyResolver = pathAssemblyResolver;
            _metadataLoadContext = metadataLoadContext;
        }

        public void Dispose()
        {
            _metadataLoadContext.Dispose();
        }

        public Assembly LoadFromName(string assemblyName)
        {
            return _metadataLoadContext.LoadFromAssemblyName(assemblyName);
        }
    }
}