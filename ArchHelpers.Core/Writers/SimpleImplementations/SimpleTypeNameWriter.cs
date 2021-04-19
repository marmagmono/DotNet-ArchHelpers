using System;
using System.IO;
using ArchHelpers.Core.Converters;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class SimpleTypeNameWriter : ITypeNameWriter
    {
        private readonly ITypeNameConverter _typeNameConverter;

        public SimpleTypeNameWriter(ITypeNameConverter typeNameConverter)
        {
            _typeNameConverter = typeNameConverter;
        }

        public void Write(TextWriter writer, Type type)
        {
            writer.Write(_typeNameConverter.ToString(type));
        }
    }
}