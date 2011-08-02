using System;
using System.ComponentModel;
using System.Globalization;
using Mono.Options;

class FooConverter : TypeConverter {
	public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof (string))
			return true;
		return base.CanConvertFrom (context, sourceType);
	}

	public override object ConvertFrom (ITypeDescriptorContext context,
			CultureInfo culture, object value)
	{
		string v = value as string;
		if (v != null) {
			switch (v) {
				case "A": return Foo.A;
				case "B": return Foo.B;
			}
		}

		return base.ConvertFrom (context, culture, value);
	}
}

[TypeConverter (typeof(FooConverter))]
class Foo {
	public static readonly Foo A = new Foo ("A");
	public static readonly Foo B = new Foo ("B");
	string s;
	Foo (string s) { this.s = s; }
	public override string ToString () {return s;}
}

class DemoOption<T> : Option {
	Action<T> action;

	public DemoOption (string prototype, Action<T> action)
		: base (prototype, null)
	{
		this.action = action;
	}

	protected override void OnParseComplete (OptionContext c)
	{
		Console.WriteLine ("# Parsed {0}; Value={1}; Index={2}",
				c.OptionName, c.OptionValues [0] ?? "<null>", c.OptionIndex);
		action (Parse<T> (c.OptionValues [0], c));
	}
}

class Test {
	public static void Main (string[] args)
	{
		Foo    f = null;
		int    n = -1;
		string s = null;
		OptionSet p = new OptionSet () {
			new DemoOption<Foo>    ("f:", v => f = v),
			new DemoOption<int>    ("n=", v => n = v),
			new DemoOption<string> ("s:", v => s = v),
		};
		try {
			p.Parse (args);
		}
		catch (OptionException e) {
			Console.Write ("context: ");
			Console.WriteLine (e.Message);
			return;
		}
		Console.WriteLine ("f={0}", f == null ? "<null>" : f.ToString ());
		Console.WriteLine ("n={0}", n);
		Console.WriteLine ("s={0}", s ?? "<null>");
	}
}
