using System.IO;
using System.Reflection;

namespace ArchHelpers.Core.Writers
{
    public interface IEventWriter
    {
        void Write(TextWriter writer, EventInfo eventInfo);
    }
}