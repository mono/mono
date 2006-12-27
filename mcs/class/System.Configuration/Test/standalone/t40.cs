using System;
using System.Configuration;

public class MyElement : ConfigurationElement
{
	public MyElement ()
	{
	}

	[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsKey)]
	public string Name {
		get { return (string) this ["name"]; }
	}
	[ConfigurationProperty ("value")]
	public string Value {
		get { return (string) this ["value"]; }
	}
}

[ConfigurationCollection (typeof (MyElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
public class MyElementCollection : ConfigurationElementCollection
{
	protected override ConfigurationElement CreateNewElement ()
	{
		return new MyElement ();
	}
	protected override object GetElementKey (ConfigurationElement e)
	{
		return ((MyElement) e).Name;
	}

	public void Add (MyElement e)
	{
		BaseAdd (e);
	}

	protected override void BaseAdd (ConfigurationElement e)
	{
		base.BaseAdd (e);
	}
}

public class MySection : ConfigurationSection
{
	[ConfigurationProperty ("MyElements")]
	public MyElementCollection MyElements {
		get { return (MyElementCollection) this ["MyElements"]; }
	}
}

public class Driver
{
	public static void Main ()
	{
		try {
			MySection ms = (MySection) ConfigurationManager.GetSection ("MySection");
			foreach (MyElement e in ms.MyElements)
				Console.WriteLine (e.Name);
		} catch (ConfigurationException ex) {
			Console.WriteLine ("Error.");
		}
	}
}

