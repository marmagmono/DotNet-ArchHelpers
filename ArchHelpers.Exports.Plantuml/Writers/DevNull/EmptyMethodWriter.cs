using System.IO;
using System.Reflection;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml.Writers.DevNull
{
    public class EmptyMethodWriter : IMethodWriter
    {
        public void Write(TextWriter writer, MethodInfo methodInfo)
        {
        }
    }
}