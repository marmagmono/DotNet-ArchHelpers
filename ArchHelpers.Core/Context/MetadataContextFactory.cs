using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ArchHelpers.Core.Context
{
    public static class MetadataContextFactory
    {
        public static IMetadataContext CreateContextForDirectories(params string[] assembliesDirectories)
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var paths = new List<string>(runtimeAssemblies);
            foreach (var directory in assembliesDirectories)
            {
                paths.AddRange(Directory.GetFiles(directory, "*.dll"));
            }

            var pathAssemblyResolver = new PathAssemblyResolver(paths);
            var metadataLoadContext = new MetadataLoadContext(pathAssemblyResolver);

            return new MetadataContext(pathAssemblyResolver, metadataLoadContext);
        }
    }
}