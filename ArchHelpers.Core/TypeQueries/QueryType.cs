using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArchHelpers.Core.TypeQueries
{
    public class QueryType : IQueryType
    {
        private readonly TypeQueryOptions _typeQueryOptions;

        public QueryType(TypeQueryOptions typeQueryOptions)
        {
            _typeQueryOptions = typeQueryOptions;
        }

        public IEnumerable<TypeRelation> GetDependencies(Type type, Func<Type, bool> acceptDependencyType)
        {
            static void BuildBaseDependenciesToExclude(IQueryType queryType, HashSet<Type> typesToExclude, Type baseType)
            {
                foreach (var @interface in queryType.GetInterfaces(baseType))
                {
                    typesToExclude.Add(@interface);
                }

                foreach (var constructorInfo in queryType.GetConstructors(baseType))
                {
                    foreach (var parameterInfo in constructorInfo.GetParameters())
                    {
                        var parameterType = parameterInfo.ParameterType;
                        if (!parameterType.IsGenericParameter)
                        {
                            typesToExclude.Add(parameterType);
                        }
                    }
                }
            }

            // Simple version that does not tries to analyse the bytecode of methods and created local variables
            // and method calls.

            // Attribute usages on class and parameters are ignored as well.

            var set = new HashSet<TypeRelation>(new TypeRelationComparer());

            var baseDependenciesToExclude = new HashSet<Type>();
            if (_typeQueryOptions.IncludeInherited == false && type.BaseType != null)
            {
                BuildBaseDependenciesToExclude(this, baseDependenciesToExclude, type.BaseType);
            }

            static void TryAddTypeRelation(Type type, Type? dependencyType, RelationKind relationKind, HashSet<TypeRelation> set, Func<Type, bool> shouldIncludeType)
            {
                if (dependencyType == null)
                {
                    return;
                }

                if (dependencyType.IsGenericParameter)
                {
                    var constraints = dependencyType.GetGenericParameterConstraints();
                    foreach (var constraint in constraints)
                    {
                        TryAddTypeRelation(type, constraint, relationKind, set, shouldIncludeType);
                    }

                    return;
                }

                if (shouldIncludeType(dependencyType))
                {
                    set.Add(new TypeRelation(type, dependencyType, relationKind));
                }
            }

            static void AddParameterTypes(
                MethodBase method,
                Type type,
                RelationKind relationKind,
                HashSet<TypeRelation> set,
                Func<Type, bool> shouldIncludeType)
            {
                foreach (var parameterInfo in method.GetParameters())
                {
                    var ptype = parameterInfo.ParameterType;
                    TryAddTypeRelation(type, ptype, relationKind, set, shouldIncludeType);
                }
            }

            // Interfaces
            foreach (var @interface in GetInterfaces(type))
            {
                bool ShouldIncludeInterface(Type t) => acceptDependencyType(t) && !baseDependenciesToExclude.Contains(t);

                TryAddTypeRelation(type, @interface, RelationKind.Implements, set, ShouldIncludeInterface);
            }

            // Constructors
            foreach (var constructor in GetConstructors(type))
            {
                bool ShouldIncludeConstructorParameter(Type t) => acceptDependencyType(t) && !baseDependenciesToExclude.Contains(t);

                AddParameterTypes(constructor, type, RelationKind.Uses, set, ShouldIncludeConstructorParameter);
            }

            // Methods
            foreach (var method in GetMethods(type))
            {
                AddParameterTypes(method, type, RelationKind.ParameterUse, set, acceptDependencyType);
                TryAddTypeRelation(type, method.ReturnType, RelationKind.Uses, set, acceptDependencyType);
            }

            // Fields
            foreach (var field in GetFields(type))
            {
                TryAddTypeRelation(type, field.FieldType, RelationKind.Uses, set, acceptDependencyType);
            }

            // Properties
            foreach (var property in GetProperties(type))
            {
                TryAddTypeRelation(type, property.PropertyType, RelationKind.Uses, set, acceptDependencyType);
            }

            // Events
            foreach (var eventInfo in GetEvents(type))
            {
                TryAddTypeRelation(type, eventInfo.EventHandlerType, RelationKind.Uses, set, acceptDependencyType);
            }

            // Base type
            TryAddTypeRelation(type, type.BaseType, RelationKind.Extends, set, acceptDependencyType);

            // Generic arguments
            foreach (var genericTypeArgument in GetGenericTypeArguments(type))
            {
                TryAddTypeRelation(type, genericTypeArgument, RelationKind.ParameterUse, set, acceptDependencyType);
            }

            return set;
        }

        public IEnumerable<Type> GetInterfaces(Type type)
        {
            var interfaces = type.GetInterfaces();
            if (_typeQueryOptions.IncludeNonPublic)
            {
                return interfaces;
            }

            return interfaces.Where(i => i.IsPublic);
        }

        public Type[] GetGenericTypeArguments(Type type)
        {
            var generics = type.GetGenericArguments();
            return generics;
        }

        public ConstructorInfo[] GetConstructors(Type type)
        {
            var bindingFlags = ResolveBindingFlags();
            return type.GetConstructors(bindingFlags);
        }

        public FieldInfo[] GetFields(Type type)
        {
            static bool IsCompilerGenerated(MemberInfo member)
            {
                var attributes = member.GetCustomAttributesData();
                return attributes.Any(a =>
                    a.AttributeType.FullName?
                        .StartsWith("System.Runtime.CompilerServices.CompilerGeneratedAttribute") == true);
            }

            var bindingFlags = ResolveBindingFlags();
            return type.GetFields(bindingFlags).Where(fi => IsCompilerGenerated(fi) == false).ToArray();
        }

        public PropertyInfo[] GetProperties(Type type)
        {
            var bindingFlags = ResolveBindingFlags();
            return type.GetProperties(bindingFlags);
        }

        public EventInfo[] GetEvents(Type type)
        {
            var bindingFlags = ResolveBindingFlags();
            return type.GetEvents(bindingFlags);
        }

        public MethodInfo[] GetMethods(Type type)
        {
            static bool CanBePropertyMethod(MethodInfo mi) => mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_");

            static bool IsPropertyMethod(PropertyInfo[] properties, MethodInfo mi)
            {
                if (!CanBePropertyMethod(mi))
                {
                    return false;
                }

                return properties.Any(p => p.Name.Equals(mi.Name.Substring(4)));
            }

            // event methods ?
            // use method.IsSpecialName ?
            var bindingFlags = ResolveBindingFlags();
            var methods = type.GetMethods(bindingFlags);
            var properties = GetProperties(type);

            if (properties.Length == 0)
            {
                return methods;
            }

            return methods.Where(mi => IsPropertyMethod(properties, mi) == false).ToArray();
        }

        private BindingFlags ResolveBindingFlags()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (_typeQueryOptions.IncludeStatic)
            {
                flags |= BindingFlags.Static;
            }

            if (_typeQueryOptions.IncludeNonPublic)
            {
                flags |= BindingFlags.NonPublic;
            }

            if (_typeQueryOptions.IncludeInherited == false)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            return flags;
        }

        private class TypeRelationComparer : IEqualityComparer<TypeRelation>
        {
            public bool Equals(TypeRelation? x, TypeRelation? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return x.Dependency == y.Dependency && x.RelationKind == y.RelationKind;
            }

            public int GetHashCode(TypeRelation obj)
            {
                return obj.Dependency.GetHashCode();
            }
        }
    }
}