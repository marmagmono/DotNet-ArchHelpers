// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Runtime.InteropServices;
// using ArchHelpers.Core.TypeQueries;
// using ArchHelpers.Core.Writers;
// using ArchHelpers.Core.Writers.SimpleImplementations;
//
// namespace ArchHelpers.ConsoleApp
// {
//     public sealed class ModelLoader : IDisposable
//     {
//         private PathAssemblyResolver _pathAssemblyResolver;
//         private MetadataLoadContext _metadataLoadContext;
//
//         public void LoadModel()
//         {
//             string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
//             string[] aspnetCoreAssemblies = Directory.GetFiles("/usr/share/dotnet/shared/Microsoft.AspNetCore.App/5.0.5", "*.dll");
//             var paths = new List<string>(runtimeAssemblies);
//             paths.AddRange(aspnetCoreAssemblies);
//
//             var ss = runtimeAssemblies.Where(n => n.Contains("Rout")).ToArray();
//
//             _pathAssemblyResolver = new PathAssemblyResolver(paths);
//             _metadataLoadContext = new MetadataLoadContext(_pathAssemblyResolver);
//
//             var routingAssembly = _metadataLoadContext.LoadFromAssemblyName("Microsoft.AspNetCore.Routing");
//             var routingAbstractions = _metadataLoadContext.LoadFromAssemblyName("Microsoft.AspNetCore.Routing.Abstractions");
//
//             var dict = new Dictionary<string, int>();
//             var printer = new SimpleTypeNameWriter();
//             printer.Write(dict.GetType());
//
//             var types = routingAssembly.GetTypes();
//             foreach (var type in types)
//             {
//                 WriteTypeInfoNew(type);
//             }
//
//             int a = 10;
//         }
//
//         public void Dispose()
//         {
//             _metadataLoadContext?.Dispose();
//         }
//
//         private void WriteTypeInfoNew(Type definedType)
//         {
//             var queryOptions = new TypeQueryOptions()
//             {
//             };
//
//             var typeQueryFactory = new TypeQueryFactory();
//             var typeQuery = typeQueryFactory.CreateTypeQuery(definedType, queryOptions);
//             var typeNamePrinter = new SimpleTypeNameWriter();
//             var propertyPrinter = new PropertyWriter(typeNamePrinter);
//             var methodPrinter = new MethodWriter(typeNamePrinter);
//             var eventPrinter = new EventWriter(typeNamePrinter);
//
//             var interfaces = definedType.GetInterfaces();
//             Console.WriteLine($"=========> {typeNamePrinter.Write(definedType)}");
//             Console.WriteLine($"Namespace: {definedType.Namespace}");
//             Console.WriteLine($"Implements: {string.Join(',', interfaces.Select(ii => ii.Name))}");
//
//             Console.WriteLine($"----------- Properties -----------------");
//             foreach (var property in typeQuery.GetProperties())
//             {
//                 Console.WriteLine(propertyPrinter.Write(property));
//             }
//
//             Console.WriteLine($"----------- METHODS -----------------");
//             foreach (var method in typeQuery.GetMethods())
//             {
//                 Console.WriteLine(methodPrinter.Write(method));
//             }
//
//             Console.WriteLine($"----------- EVENTS -----------------");
//             foreach (var eventInfo in typeQuery.GetEvents())
//             {
//                 Console.BackgroundColor = ConsoleColor.Green;
//                 Console.WriteLine(eventPrinter.Write(eventInfo));
//                 Console.ResetColor();
//             }
//         }
//
//         private void WriteTypeInfo(Type definedType)
//         {
//             // Attributes on type, method, property, constructor, parameters
//             // Generics
//             // Base classes
//             // implemented interfaces
//
//             var properties = definedType.GetProperties(
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
//             var methods = definedType.GetMethods(
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
//             var constructors = definedType.GetConstructors(
//                 BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance);
//
//             if (definedType.IsNestedPrivate)
//             {
//                 return;
//             }
//
//             var interfaces = definedType.GetInterfaces();
//
//             Console.WriteLine($"=========> {definedType.Name}");
//             Console.WriteLine($"Namespace: {definedType.Namespace}");
//             Console.WriteLine($"Implements: {string.Join(',', interfaces.Select(ii => ii.Name))}");
//
//
//             Console.WriteLine($"----------- PROPERTIES -----------------");
//             foreach (var property in properties)
//             {
//                 Console.WriteLine($"Property ---> {property.Name}: {property.PropertyType.Name}");
//                 Console.WriteLine($"CanRead: {property.CanRead}");
//                 Console.WriteLine($"CanWrite: {property.CanWrite}");
//             }
//             Console.WriteLine($"----------- METHODS -----------------");
//             foreach (var method in methods)
//             {
//                 if (method.DeclaringType?.FullName?.Equals("System.Object", StringComparison.InvariantCulture) == true)
//                 {
//                     continue;
//                 }
//
//                 var pars = method.GetParameters();
//                 var parsString = string.Join(',', pars.Select(p => $"{p.Name}: {p.ParameterType.Name}"));
//                 Console.WriteLine($"Method ---> {method.Name}({parsString}): {method.ReturnType.Name} Declared in: {method.DeclaringType?.Name}");
//             }
//             Console.WriteLine($"----------- Constructors -----------------");
//             foreach (var constructor in constructors)
//             {
//                 if (constructor.DeclaringType?.FullName?.Equals("System.Object", StringComparison.InvariantCulture) == true)
//                 {
//                     continue;
//                 }
//
//                 var pars = constructor.GetParameters();
//                 var parsString = string.Join(',', pars.Select(p => $"{p.Name}: {p.ParameterType.Name}"));
//                 Console.WriteLine($"Method ---> {constructor.Name}({parsString})");
//             }
//             Console.WriteLine($"=========================================================================");
//         }
//     }
// }