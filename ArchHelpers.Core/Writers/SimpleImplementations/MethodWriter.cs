using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ArchHelpers.Core.Converters;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class MethodWriter : IMethodWriter
    {
        private readonly ITypeNameConverter _typeNameConverter;
        private readonly ITypeNameWriter _typeNameWriter;

        public MethodWriter(ITypeNameConverter typeNameConverter, ITypeNameWriter typeNameWriter)
        {
            _typeNameConverter = typeNameConverter;
            _typeNameWriter = typeNameWriter;
        }

        public void Write(TextWriter writer, MethodInfo methodInfo)
        {
            var pars = methodInfo.GetParameters();
            var parsString = string.Join(',', pars.Select(p => $"{p.Name}: {_typeNameConverter.ToString(p.ParameterType)}"));

            var builder = new StringBuilder();

            _typeNameWriter.Write(writer, methodInfo.ReturnType);
            writer.Write(' ');
            writer.Write(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                var genericParams = string.Join(',',
                    methodInfo.GetGenericArguments().Select(_typeNameConverter.ToString));
                writer.Write('<');
                writer.Write(genericParams);
                writer.Write('>');
            }

            writer.Write('(');
            writer.Write(parsString);
            writer.Write(')');
        }
    }
}