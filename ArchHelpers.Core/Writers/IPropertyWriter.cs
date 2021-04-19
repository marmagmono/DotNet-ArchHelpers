using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers
{
    public interface IPropertyWriter
    {
        void Write(TextWriter writer, PropertyInfo propertyInfo);
    }
}