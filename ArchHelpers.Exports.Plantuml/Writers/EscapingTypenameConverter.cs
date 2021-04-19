using System;
using System.Linq;
using ArchHelpers.Core.Converters;

namespace ArchHelpers.Exports.Plantuml
{
    public class EscapingTypenameConverter : ITypeNameConverter
    {
        public string ToString(Type type) => GetTypeName(type);

        private static string GetTypeName(Type type)
        {
            var genericArguments = type.GetGenericArguments();

            if (genericArguments.Length > 0)
            {
                var genericNames = genericArguments.Select(GetTypeName);
                return $"{GetSimpleTypeName(type)}__{string.Join(',', genericNames)}__";
            }

            return GetSimpleTypeName(type);
        }

        private static string GetSimpleTypeName(Type type) =>
            type.IsGenericType ? type.Name.Split('`')[0] : type.Name;
    }
}