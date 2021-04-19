using System;
using System.Reflection;

namespace ArchHelpers.Core.Context
{
    public interface IMetadataContext : IDisposable
    {
        Assembly LoadFromName(string assemblyName);
    }
}