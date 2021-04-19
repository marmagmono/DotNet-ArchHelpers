using System.IO;
using System.Reflection;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml.Writers.DevNull
{
    public class EmptyEventWriter : IEventWriter
    {
        public void Write(TextWriter writer, EventInfo eventInfo)
        {
        }
    }
}