using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;
using Monodoc.Generators;

namespace MonoTests.Monodoc
{
	[TestFixture]
	public class EcmaDocTest
	{
		// Because EcmaDoc is internal and we can't use InternalsVisibleTo since test assemblies aren't
		// signed/strong-named by the build, we have to resort to reflection
		TDelegate GetTestedMethod<TDelegate> (string methodName)
		{
			var ecmaDoc = Type.GetType ("Monodoc.Providers.EcmaDoc, monodoc, PublicKey=0738eb9f132ed756");
			return (TDelegate)(object)Delegate.CreateDelegate (typeof (TDelegate), ecmaDoc.GetMethod (methodName));
		}

		[Test]
		public void CountTypeGenericArgumentsTest ()
		{
			var realCountTypeGenericArguments = GetTestedMethod<Func<string, int, int>> ("CountTypeGenericArguments");
			// Since we don't use the optional start index parameters, bypass it by wrapping the func
			Func<string, int> countTypeGenericArguments = s => realCountTypeGenericArguments (s, 0);

			Assert.AreEqual (0, countTypeGenericArguments ("T:System.String"), "#0a");
			Assert.AreEqual (0, countTypeGenericArguments ("T:String"), "#0b");
			Assert.AreEqual (0, countTypeGenericArguments ("String"), "#0c");

			Assert.AreEqual (1, countTypeGenericArguments ("T:System.Collections.Foo<T>"), "#1a");
			Assert.AreEqual (1, countTypeGenericArguments ("T:System.Foo<T>"), "#1b");
			Assert.AreEqual (1, countTypeGenericArguments ("T:Foo<T>"), "#1c");
			Assert.AreEqual (1, countTypeGenericArguments ("Foo<T>"), "#1d");

			Assert.AreEqual (2, countTypeGenericArguments ("T:System.Collections.Foo<T, U>"), "#2a");
			Assert.AreEqual (2, countTypeGenericArguments ("T:System.Foo<TKey, TValue>"), "#2b");
			Assert.AreEqual (2, countTypeGenericArguments ("T:Foo<Something,Else>"), "#2c");
			Assert.AreEqual (2, countTypeGenericArguments ("Foo<TDelegate,TArray>"), "#2d");

			Assert.AreEqual (3, countTypeGenericArguments ("T:System.Collections.Foo<T, U, V>"), "#3a");
			Assert.AreEqual (3, countTypeGenericArguments ("T:System.Foo<TKey, TValue, THash>"), "#3b");
			Assert.AreEqual (3, countTypeGenericArguments ("T:Foo<Something,Else,Really>"), "#3c");
			Assert.AreEqual (3, countTypeGenericArguments ("Foo<TDelegate,TArray,TEvent>"), "#3d");
		}

		[Test]
		public void CountTypeGenericArgumentsTest_Nested ()
		{
			var realCountTypeGenericArguments = GetTestedMethod<Func<string, int, int>> ("CountTypeGenericArguments");
			// Since we don't use the optional start index parameters, bypass it by wrapping the func
			Func<string, int> countTypeGenericArguments = s => realCountTypeGenericArguments (s, 0);

			Assert.AreEqual (1, countTypeGenericArguments ("T:System.Collections.Foo<T[]>"), "#1a");
			Assert.AreEqual (1, countTypeGenericArguments ("T:System.Collections.Foo<IList<T>>"), "#1b");
			Assert.AreEqual (2, countTypeGenericArguments ("T:System.Collections.Foo<T, KeyValuePair<T, U>>"), "#2a");
			Assert.AreEqual (2, countTypeGenericArguments ("T:System.Collections.Foo<T, KeyValuePair<IProducerConsumerCollection<U>, IEquatable<V>>>"), "#2b");
			Assert.AreEqual (3, countTypeGenericArguments ("T:System.Collections.Foo<T, IProducerConsumerCollection<U>, IEquatable<V>>"), "#3a");
		}
	}
}
