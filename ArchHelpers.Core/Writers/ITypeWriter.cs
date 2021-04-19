using System;
using System.IO;
using ArchHelpers.Core.TypeQueries;

namespace ArchHelpers.Core.Writers
{
    public interface ITypeWriter
    {
        void Write(TextWriter writer, Type type);
    }
}