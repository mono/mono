//
// RelaxngDatatypeProviderTest.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//

using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.XmlSchema;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RelaxngDatatypeProviderTests
	{
		[Test]
		public void CustomTypeProvider ()
		{
			var datatypeLibrary = SetupMyDataProvider ();
			XmlDocument xml = new XmlDocument ();
			xml.LoadXml ("<root> <v1>mytype</v1> <v2>1</v2> </root>");
			XmlDocument schemaXml = ReadDoc (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/463264.rng"));
			XmlReader reader = new RelaxngValidatingReader (new XmlNodeReader (xml), new XmlNodeReader (schemaXml), datatypeLibrary);
			while (reader.Read ())
				;
		}

		[Test]
		public void Bug463267 ()
		{
			var datatypeLibrary = SetupMyDataProvider ();
			XmlDocument xml = new XmlDocument ();
			xml.LoadXml ("<root> <v2>1</v2> <v1>mytype</v1> </root>");
			XmlDocument schemaXml = ReadDoc (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/463267.rng"));
			XmlReader reader = new RelaxngValidatingReader (new XmlNodeReader (xml), new XmlNodeReader (schemaXml), datatypeLibrary);
			while (reader.Read ())
				;
		}

		RelaxngDatatypeProvider SetupMyDataProvider ()
		{
			var datatypeLibrary = new RelaxngMergedProvider ();

			datatypeLibrary [MyRngTypeProvider.Namespace] = new MyRngTypeProvider();
			datatypeLibrary ["http://www.w3.org/2001/XMLSchema-datatypes"] = XsdDatatypeProvider.Instance;
			datatypeLibrary [System.Xml.Schema.XmlSchema.Namespace] = XsdDatatypeProvider.Instance;
			datatypeLibrary [String.Empty] = RelaxngMergedProvider.DefaultProvider [string.Empty];
			return datatypeLibrary;
		}

		XmlDocument ReadDoc (string s)
		{
			var d = new XmlDocument ();
			d.Load (s);
			return d;
		}
	}

	class MyRngTypeProvider : RelaxngDatatypeProvider
	{
		public static readonly string Namespace = "http://tempuri.org/mytypes";

		public override RelaxngDatatype GetDatatype(string name, string ns, RelaxngParamList parameters)
		{
			switch (name)
			{
			case "mytype":
			    return new MyType();
			}
			return null;
		}
	}

	class MyType : RelaxngDatatype
	{
		public override string Name {
			get { return "mytype"; }
		}

		public override string NamespaceURI {
			get { return MyRngTypeProvider.Namespace; }
		}

		public override object Parse (string text, System.Xml.XmlReader reader)
		{
			return text;
		}

		public override bool IsValid (string text, System.Xml.XmlReader reader)
		{
			return ((string) Parse (text, reader)) == "mytype";
		}
	}
}