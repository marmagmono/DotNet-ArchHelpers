using System;
using System.IO;
using System.Linq;
using ArchHelpers.Core.Converters;
using ArchHelpers.Core.TypeQueries;

namespace ArchHelpers.Core.Writers.SimpleImplementations
{
    public class SimpleTypeWriter : ITypeWriter
    {
        private readonly ITypeQueryFactory _typeQueryFactory;
        private readonly ITypeNameWriter _typeNameWriter;
        private readonly ITypeNameConverter _typeNameConverter;
        private readonly IEventWriter _eventWriter;
        private readonly IMethodWriter _methodWriter;
        private readonly IFieldWriter _fieldWriter;
        private readonly IPropertyWriter _propertyWriter;

        public SimpleTypeWriter(
            ITypeQueryFactory typeQueryFactory,
            ITypeNameWriter typeNameWriter,
            ITypeNameConverter typeNameConverter,
            IEventWriter eventWriter,
            IMethodWriter methodWriter,
            IFieldWriter fieldWriter,
            IPropertyWriter propertyWriter)
        {
            _typeQueryFactory = typeQueryFactory;
            _typeNameWriter = typeNameWriter;
            _typeNameConverter = typeNameConverter;
            _eventWriter = eventWriter;
            _methodWriter = methodWriter;
            _fieldWriter = fieldWriter;
            _propertyWriter = propertyWriter;
        }

        public void Write(TextWriter writer, Type type)
        {
            var typeQuery = _typeQueryFactory.CreateTypeQuery(new TypeQueryOptions());
            Write(writer, type, typeQuery);
        }

        public void Write(TextWriter writer, Type type, IQueryType queryType)
        {
            WriteTypeKind(writer, type);
            writer.Write(' ');
            _typeNameWriter.Write(writer, type);

            if (type.BaseType != null)
            {
                writer.Write(" : ");
                _typeNameWriter.Write(writer, type.BaseType);
            }

            var implementedInterfaces = queryType.GetInterfaces(type).ToArray();
            if (implementedInterfaces.Length > 0)
            {
                writer.Write(" implements ");
                writer.Write(string.Join(',', implementedInterfaces.Select(_typeNameConverter.ToString)));
            }

            writer.WriteLine(" {");

            WriteMembers("Fields",
                writer,
                () => queryType.GetFields(type),
                i => _fieldWriter.Write(writer, i));

            WriteMembers("Properties",
                writer,
                () => queryType.GetProperties(type),
                i => _propertyWriter.Write(writer, i));

            WriteMembers("Methods",
                writer,
                () => queryType.GetMethods(type),
                i => _methodWriter.Write(writer, i));

            WriteMembers("Events",
                writer,
                () => queryType.GetEvents(type),
                i => _eventWriter.Write(writer, i));

            writer.WriteLine("}");
        }

        private static void WriteTypeKind(TextWriter writer, Type type)
        {
            // TODO: struct
            writer.Write(type.IsInterface ? "interface" : "class");
        }

        private void WriteMembers<T>(string header, TextWriter writer, Func<T[]> getMembers, Action<T> writeMember)
        {
            var members = getMembers();
            if (members.Length <= 0)
            {
                return;
            }

            WriteHeader(writer, header);
            foreach (var member in members)
            {
                writeMember(member);
                writer.Write(writer.NewLine);
            }
        }

        private void WriteHeader(TextWriter writer, string header)
        {
            writer.WriteLine($"---------------- {header} ------------------------");
        }
    }
}