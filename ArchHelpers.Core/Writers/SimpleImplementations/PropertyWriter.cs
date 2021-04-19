using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class PropertyWriter : IPropertyWriter
    {
        private readonly ITypeNameWriter _typeNameWriter;

        public PropertyWriter(ITypeNameWriter typeNameWriter)
        {
            _typeNameWriter = typeNameWriter;
        }

        public void Write(TextWriter writer, PropertyInfo propertyInfo)
        {
            bool hasGetter = propertyInfo.GetMethod?.IsPublic ?? false;
            bool hasSetter = propertyInfo.SetMethod?.IsPublic ?? false;

            _typeNameWriter.Write(writer, propertyInfo.PropertyType);
            writer.Write(' ');
            writer.Write(propertyInfo.Name);
            if (hasGetter && hasSetter)
            {
                writer.Write(" { get; set; }");
            }
            else if (hasGetter)
            {
                writer.Write(" { get; }");
            }
            else if (hasSetter)
            {
                writer.Write(" { set; }");
            }
        }
    }
}