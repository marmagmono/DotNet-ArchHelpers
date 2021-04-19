using System;

namespace ArchHelpers.Core.Converters
{
    public interface ITypeNameConverter
    {
        string ToString(Type type);
    }
}