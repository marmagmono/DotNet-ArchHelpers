using System;
using System.Collections.Generic;
using System.Reflection;

namespace ArchHelpers.Core.TypeQueries
{
    public interface IQueryType
    {
        IEnumerable<TypeRelation> GetDependencies(Type type, Func<Type, bool> acceptDependencyType);

        IEnumerable<Type> GetInterfaces(Type type);

        Type[] GetGenericTypeArguments(Type type);

        ConstructorInfo[] GetConstructors(Type type);

        FieldInfo[] GetFields(Type type);

        PropertyInfo[] GetProperties(Type type);

        EventInfo[] GetEvents(Type type);

        MethodInfo[] GetMethods(Type type);
    }
}