using System;
using System.Linq;

namespace ArchHelpers.Core.Converters
{
    public class SimpleTypeNameConverter : ITypeNameConverter
    {
        public string ToString(Type type) => GetTypeName(type);

        private static string GetTypeName(Type type)
        {
            var genericArguments = type.GetGenericArguments();

            if (genericArguments.Length > 0)
            {
                var genericNames = genericArguments.Select(GetTypeName);
                return $"{GetSimpleTypeName(type)}<{string.Join(',', genericNames)}>";
            }

            return GetSimpleTypeName(type);
        }

        private static string GetSimpleTypeName(Type type) =>
            type.IsGenericType ? type.Name.Split('`')[0] : type.Name;
    }
}