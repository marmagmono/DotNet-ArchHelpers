using System;
using System.Linq;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis;
using ArchHelpers.Core.AssemblyQueries.TypeRelationsAnalysis.Builders;
using ArchHelpers.Core.TypeQueries;
using Xunit;

namespace ArchHelpers.Core.Tests.AssemblyQueries.TypeRelationsAnalysis
{
    public class RelationGraphBuilderFacts
    {
        private static readonly (Type, Type, RelationKind)[] EmptyTypeContextArray =
            new (Type t, Type dependency, RelationKind rk)[] { };

        public class SomeClass
        {
        }

        public class AnotherClass
        {
        }

        public interface IDoSomething
        {
            public void DoSomething();
        }

        public interface IDoAnotherThing
        {
            public void DoAnotherThing();
        }

        public class Doer : IDoSomething, IDoAnotherThing
        {
            public void DoSomething()
            {
            }

            public void DoAnotherThing()
            {
            }
        }
        
        public class EmptyGenericClass<T, Y>
        {
            public T Property { get; set; }

            public Y AnotherProperty { get; set; }

            public void GetSomething(T p1, Y p2)
            {
            }

            public U GenericMethod<U, V>(U p1, V p2) where U: new()
            {
                return new U();
            }
        }

        public class ConstrainedGenericClass<T>
            where T : IDoSomething, IDoAnotherThing
        {
        }

        public class BaseClass : IDoAnotherThing
        {
            public BaseClass(Doer doer, IDoSomething anotherDoer)
            {
            }

            public void DoAnotherThing()
            {
            }
        }

        public class DerivedClass : BaseClass
        {
            public DerivedClass(Doer doer, IDoSomething anotherDoer) : base(doer, anotherDoer)
            {
            }
        }

        [Fact]
        public void EmptyNotFilledGenericClassDependencies_AreEmpty()
        {
            // Arrange
            var t = typeof(EmptyGenericClass<,>);
            var sut = new RelationGraphBuilder();

            // Act
            var relations = sut.BuildTypeRelationGraph(new Type[] {t}, RelationGraphBuilderOptions.ExcludeSystemTypes);

            // Assert
            var typeContexts = relations.GetTypeContexts();
            Assert.Collection(
                typeContexts,
                typeContext =>
                {
                    Assert.Equal(t, typeContext.Type);
                    Assert.Empty(typeContext.Uses);
                    Assert.Empty(typeContext.UsedBy);
                });
        }

        [Fact]
        public void FilledGenericClassDependencies_AreListed()
        {
            // Arrange
            var t = typeof(ConstrainedGenericClass<Doer>);
            var sut = new RelationGraphBuilder();

            // Act
            var relations = sut.BuildTypeRelationGraph(new Type[] {t}, RelationGraphBuilderOptions.ExcludeSystemTypes);

            // Assert
            var typeContexts = relations.GetTypeContexts().ToArray();
            Assert.Equal(2, typeContexts.Length);
            Assert.Equal(typeContexts[0].Type, t);

            Assert.Equal(1, typeContexts[0].Uses.Count);
            Assert.Equal(0, typeContexts[0].UsedBy.Count);
            AssertRelation(typeContexts[0].Uses[0], t, typeof(Doer), RelationKind.Uses);

            Assert.Equal(0, typeContexts[1].Uses.Count);
            Assert.Equal(1, typeContexts[1].UsedBy.Count);
            AssertRelation(typeContexts[1].UsedBy[0], t, typeof(Doer), RelationKind.Uses);
        }

        [Fact]
        public void UnfilledGenericClass_Constraints_AreListed()
        {
            // Arrange
            var t = typeof(ConstrainedGenericClass<>);
            var sut = new RelationGraphBuilder();

            // Act
            var relations = sut.BuildTypeRelationGraph(new Type[] {t}, RelationGraphBuilderOptions.ExcludeSystemTypes);

            // Assert
            var typeContexts = relations.GetTypeContexts().ToArray();
            Assert.Equal(3, typeContexts.Length);
            Assert.Equal(typeContexts[0].Type, t);

            var typeContext = typeContexts[0];
            AssertTypeContext(
                typeContext,
                new []
                {
                    (t, typeof(IDoSomething), RelationKind.Uses),
                    (t, typeof(IDoAnotherThing), RelationKind.Uses)
                },
                EmptyTypeContextArray);
        }

        [Fact]
        public void ParentConstructorDependencies_AndInterfacesImplementedByParent_AreExcluded()
        {
            // Arrange
            var t = typeof(DerivedClass);
            var sut = new RelationGraphBuilder();

            // Act
            var relations = sut.BuildTypeRelationGraph(new Type[] {t}, RelationGraphBuilderOptions.ExcludeSystemTypes);

            // Assert
            var typeContexts = relations.GetTypeContexts().ToArray();
            Assert.Equal(2, typeContexts.Length);

            AssertTypeContext(
                typeContexts[0],
                new []
                {
                    (t, typeof(BaseClass), RelationKind.Extends)
                },
                EmptyTypeContextArray);

            AssertTypeContext(
                typeContexts[1],
                EmptyTypeContextArray,
                new []
                {
                    (t, typeof(BaseClass), RelationKind.Extends)
                });
        }

        private void AssertTypeContext(
            TypeContext tc,
            (Type t, Type dependency, RelationKind rk)[] expectedUses,
            (Type t, Type dependency, RelationKind rk)[] expectedUsedBy)
        {
            Assert.Equal(expectedUses.Length, tc.Uses.Count);
            Assert.Equal(expectedUsedBy.Length, tc.UsedBy.Count);

            for (var i = 0; i < expectedUses.Length; i++)
            {
                AssertRelation(tc.Uses[i], expectedUses[i].t, expectedUses[i].dependency, expectedUses[i].rk);
            }

            for (var i = 0; i < expectedUsedBy.Length; i++)
            {
                AssertRelation(tc.UsedBy[i], expectedUsedBy[i].t, expectedUsedBy[i].dependency, expectedUsedBy[i].rk);
            }
        }

        private void AssertRelation(TypeRelation tr, Type type, Type dependency, RelationKind relationKind)
        {
            Assert.Equal(type, tr.Type);
            Assert.Equal(dependency, tr.Dependency);
            Assert.Equal(relationKind, tr.RelationKind);
        }

    }
}