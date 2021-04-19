using System.IO;
using System.Reflection;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml.Writers.DevNull
{
    public class EmptyFieldWriter : IFieldWriter
    {
        public void Write(TextWriter writer, FieldInfo fieldInfo)
        {
        }
    }
}