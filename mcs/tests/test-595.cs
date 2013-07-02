using System;

public struct SymbolId
{
}

public interface IAttributesCollection
{
	object this [SymbolId name] { get; set; }
}

class AttributesCollection : IAttributesCollection
{
	public object this [SymbolId name] { 
		get { return null; } 
		set { }
	}
}

class Program
{
	public static object SetDictionaryValue (object self, SymbolId name, object value)
	{
		IAttributesCollection dict = new AttributesCollection ();
		return dict [name] = value;
	}

	public static void Main ()
	{
		SetDictionaryValue (null, new SymbolId (), 1);
	}
}