using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers
{
    public interface IFieldWriter
    {
        void Write(TextWriter writer, FieldInfo fieldInfo);
    }
}