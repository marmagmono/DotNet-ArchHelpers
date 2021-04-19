using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class FieldWriter : IFieldWriter
    {
        private readonly ITypeNameWriter _typeNameWriter;

        public FieldWriter(ITypeNameWriter typeNameWriter)
        {
            _typeNameWriter = typeNameWriter;
        }

        public void Write(TextWriter writer, FieldInfo fieldInfo)
        {
            _typeNameWriter.Write(writer, fieldInfo.FieldType);
            writer.Write(' ');
            writer.Write(fieldInfo.Name);
        }
    }
}