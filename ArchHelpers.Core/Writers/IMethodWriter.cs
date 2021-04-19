using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers
{
    public interface IMethodWriter
    {
        void Write(TextWriter writer, MethodInfo methodInfo);
    }
}