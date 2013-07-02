using System;
using System.Collections;
using System.Reflection;

using Mono.Test;

class Program
{
	public static int Main ()
	{
		BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance |
			BindingFlags.DeclaredOnly;
		Type type = typeof (Info);

		PropertyInfo [] properties = type.GetProperties (flags);
		if (properties.Length != 2) {
			Console.WriteLine ("#1: " + properties.Length.ToString ());
			return 1;
		}
		if (properties [0].Name != "System.Collections.IEnumerator.Current") {
			Console.WriteLine ("#2: " + properties [0].Name);
			return 2;
		}

		if (properties [1].Name != "Mono.Test.ITest.Item") {
			Console.WriteLine ("#3: " + properties [1].Name);
			return 3;
		}

		return 0;
	}
}

namespace Mono.Test
{
	interface ITest
	{
		object this [int index]
		{
			get;
			set;
		}
	}
}

class Info : IEnumerator, ITest
{
	object IEnumerator.Current
	{
		get { return null; }
	}

	bool IEnumerator.MoveNext ()
	{
		return false;
	}

	void IEnumerator.Reset ()
	{
	}

	object ITest.this [int index]
	{
		get { return null; }
		set { }
	}
}
