using System.IO;
using System.Reflection;
using System.Text;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class EventWriter : IEventWriter
    {
        private readonly ITypeNameWriter _typeNameWriter;

        public EventWriter(ITypeNameWriter typeNameWriter)
        {
            _typeNameWriter = typeNameWriter;
        }

        public void Write(TextWriter writer, EventInfo eventInfo)
        {
            var builder = new StringBuilder();
            if (eventInfo.EventHandlerType != null)
            {
                _typeNameWriter.Write(writer, eventInfo.EventHandlerType);
                writer.Write(' ');
            }

            writer.Write(eventInfo.Name);
        }
    }
}