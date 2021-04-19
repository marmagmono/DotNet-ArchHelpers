using System;
using System.IO;
using ArchHelpers.Core.Converters;
using ArchHelpers.Core.TypeQueries;
using ArchHelpers.Core.Writers;

namespace ArchHelpers.Exports.Plantuml
{
    public class PlantUmlTypeWriter : ITypeWriter
    {
        private readonly IQueryType _queryType;
        private readonly ITypeNameWriter _typeNameWriter;
        private readonly ITypeNameWriter _escapedTypeNameWriter;
        private readonly IEventWriter _eventWriter;
        private readonly IMethodWriter _methodWriter;
        private readonly IFieldWriter _fieldWriter;
        private readonly IPropertyWriter _propertyWriter;

        public PlantUmlTypeWriter(
            IQueryType queryType,
            ITypeNameWriter typeNameWriter,
            ITypeNameWriter escapedTypeNameWriter,
            IEventWriter eventWriter,
            IMethodWriter methodWriter,
            IFieldWriter fieldWriter,
            IPropertyWriter propertyWriter)
        {
            _queryType = queryType;
            _typeNameWriter = typeNameWriter;
            _escapedTypeNameWriter = escapedTypeNameWriter;
            _eventWriter = eventWriter;
            _methodWriter = methodWriter;
            _fieldWriter = fieldWriter;
            _propertyWriter = propertyWriter;
        }

        public void Write(TextWriter writer, Type type)
        {
            WriteTypeKind(writer, type);
            writer.Write(" \"");

            _typeNameWriter.Write(writer, type);
            writer.Write("\" as ");
            _escapedTypeNameWriter.Write(writer, type);

            writer.WriteLine(" {");

            WriteMembers(
                writer,
                () => _queryType.GetFields(type),
                i => _fieldWriter.Write(writer, i));

            WriteMembers(
                writer,
                () => _queryType.GetProperties(type),
                i => _propertyWriter.Write(writer, i));

            WriteMembers(
                writer,
                () => _queryType.GetMethods(type),
                i => _methodWriter.Write(writer, i));

            WriteMembers(
                writer,
                () => _queryType.GetEvents(type),
                i => _eventWriter.Write(writer, i));

            writer.WriteLine("}");
        }

        private static void WriteTypeKind(TextWriter writer, Type type)
        {
            // TODO: struct
            writer.Write(type.IsInterface ? "interface" : "class");
        }

        private void WriteMembers<T>(TextWriter writer, Func<T[]> getMembers, Action<T> writeMember)
        {
            var members = getMembers();
            if (members.Length <= 0)
            {
                return;
            }

            foreach (var member in members)
            {
                writeMember(member);
                writer.Write(writer.NewLine);
            }
        }
    }
}