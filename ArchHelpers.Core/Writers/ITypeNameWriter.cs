using System;
using System.IO;

namespace ArchHelpers.Core.Writers
{
    public interface ITypeNameWriter
    {
        void Write(TextWriter writer, Type type);
    }
}