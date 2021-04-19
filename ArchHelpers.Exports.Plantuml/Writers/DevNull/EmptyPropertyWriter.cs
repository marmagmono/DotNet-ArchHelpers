using System.IO;
using System.Reflection;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml.Writers.DevNull
{
    public class EmptyPropertyWriter : IPropertyWriter
    {
        public void Write(TextWriter writer, PropertyInfo propertyInfo)
        {
        }
    }
}