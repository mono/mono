using System;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
    
    [TestFixture]
    public class MethodMapTest : MethodMapTestBase
    {
		class A
		{
			public virtual void M1()
			{	
			}

			public virtual void M2()
			{
			}
		}

		class B : A
		{
			public override void M1()
			{
			}
		}

		class C : B, I1
		{
			public override void M1()
			{
			}

			public override void M2()
			{
			}
		}

		class D
		{
			public virtual void M1()
			{
			}
		}

        class Big
        {
            public class Small : A
            {
                public override void M1()
                {
                }
            }
        }

        class Big2
        {
            public class Small : I1
            {
                public void M1()
                {
                }
            }
        }

        public abstract class Intermediate : I1
        {
            public abstract void M1();
        }
        
        public class Final : Intermediate
        {
            public override void M1()
            {
            }
        }

        interface I1
        {
            void M1();
        }

        [Test]
		public void Overrides()
		{
			var overrides = _subject.GetMethodsOverriding(MethodDefinitionOf<A>("M1"));

			var expected = new[]
           	{
				MethodDefinitionOf<B>("M1"),
				MethodDefinitionOf<C>("M1"),
                MethodDefinitionOf<Big.Small>("M1"),
           	};

			CollectionAssert.AreEquivalent(expected, overrides.ToArray());
		}

        [Test]
        public void NestedTypeImplementingInterface()
        {
            var overrides = _subject.GetMethodsOverriddenBy(MethodDefinitionOf<Big2.Small>("M1"));

            var expected = new[]
           	{
				MethodDefinitionOf<I1>("M1"),
           	};

            CollectionAssert.AreEquivalent(expected, overrides.ToArray());
        }

        [Test]
        public void InterfaceOverrides()
        {
            var overrides = _subject.GetMethodsOverriding(MethodDefinitionOf<I1>("M1"));

            var expected = new[]
           	{
				MethodDefinitionOf<C>("M1"),
                MethodDefinitionOf<Big2.Small>("M1"),
                MethodDefinitionOf<Intermediate>("M1"),
                MethodDefinitionOf<Final>("M1")
            };

            CollectionAssert.AreEquivalent(expected, overrides.ToArray());
        }

        [Test]
		public void Overriden()
		{
			var overrides = _subject.GetMethodsOverriddenBy(MethodDefinitionOf<C>("M1"));

		    var expected = new[]
		    {
		        MethodDefinitionOf<B>("M1"),
		        MethodDefinitionOf<A>("M1"),
		        MethodDefinitionOf<I1>("M1")
           	};

			CollectionAssert.AreEquivalent(expected, overrides.ToArray());
		}

        public class UnrelatedType
        {
            void M1()
            {
            }
        }

        [Test]
        public void OverridenByReturnsEmptyCollectionForUnrelatedTpe()
        {
            var overrides = _subject.GetMethodsOverriddenBy(MethodDefinitionOf<UnrelatedType>("M1"));
            CollectionAssert.IsEmpty(overrides);
        }
        [Test]
        public void GetMethodsOverridingReturnsEmptyCollectionForUnrelatedTpe()
        {
            var overrides = _subject.GetMethodsOverriding(MethodDefinitionOf<UnrelatedType>("M1"));
            CollectionAssert.IsEmpty(overrides);
        }
    }

    [TestFixture]
    public class MethodMapTest2 : MethodMapTestBase
    {
        interface I1
        {
            void M();
        }
        interface I2
        {
            void M();
        }
        public class BaseA : I2
        {
            public virtual void M()
            {
            }
        }
        class ChildA : BaseA, I1
        {
            public void M()
            {
            }
        }
        class Unrelated
        {
            public void M()
            {
            }
        }

        [Test]
        public void GetEntireMethodEnheritanceGraph_Respects_Multiple_BaseMethods()
        {
            var entire = _subject.GetEntireMethodEnheritanceGraph(MethodDefinitionOf<I1>("M"));
            var expect = new[]
                             {
                                 MethodDefinitionOf<ChildA>("M"),
                                 MethodDefinitionOf<BaseA>("M"),
                                 MethodDefinitionOf<I2>("M"),
                                 MethodDefinitionOf<I1>("M"),
                             };
        
            CollectionAssert.AreEquivalent(expect,entire.ToList());
        }

    }
}
