using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
    [TestFixture]
    class MethodMapTestForExplicitInterfaceImplementation : MethodMapTestBase
    {
        interface I1
        {
            void M1();
        }

        public class A : I1
        {
            void I1.M1()
            {
            }
        }

        [Test]
        public void MethodsOverridenBy()
        {
            var m = MethodDefinitionOf<A>(String.Format("{0}.M1", typeof(I1).FullName.Replace("+",".")));
            var overrides = _subject.GetMethodsOverriddenBy(m);

            var expected = new[]
           	{
				MethodDefinitionOf<I1>("M1"),
           	};

            CollectionAssert.AreEquivalent(expected, overrides.ToArray());
        }
    }
}

