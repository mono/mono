// mono reader-method-gen.exe > System.Xml/XmlDictionaryReaderAutoGen.cs
using System;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml;
using Microsoft.CSharp;

public class Generator
{
	public static void Main ()
	{
		Console.Out.NewLine = "\n";
		Type [] types = new Type [] {
			typeof (bool), typeof (DateTime), typeof (decimal), typeof (double),
			typeof (Guid), typeof (short), typeof (int), typeof (long), typeof (float), typeof (TimeSpan) };

		Console.WriteLine (@"
#pragma warning disable 612
using System;
using System.Collections.Generic;

namespace System.Xml
{
	public abstract partial class XmlDictionaryReader : XmlReader
	{
		static readonly char [] wsChars = new char [] {' ', '\t', '\n', '\r'};

		void CheckReadArrayArguments (Array array, int offset, int length)
		{
			if (array == null)
				throw new ArgumentNullException (""array"");
			if (offset < 0)
				throw new ArgumentOutOfRangeException (""offset is negative"");
			if (offset > array.Length)
				throw new ArgumentOutOfRangeException (""offset exceeds the length of the destination array"");
			if (length < 0)
				throw new ArgumentOutOfRangeException (""length is negative"");
			if (length > array.Length - offset)
				throw new ArgumentOutOfRangeException (""length + offset exceeds the length of the destination array"");
		}

		void CheckDictionaryStringArgs (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException (""localName"");
			if (namespaceUri == null)
				throw new ArgumentNullException (""namespaceUri"");
		}
");

		foreach (Type type in types) {
			Console.WriteLine (@"
		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}}

		public virtual int ReadArray (string localName, string namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {{
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.To{1} (ReadContentAsString ());
				ReadEndElement ();
			}}
			return length;
		}}

		public virtual {0} [] Read{1}Array (string localName, string namespaceUri)
		{{
			List<{0}> list = new List<{0}> ();
			while (true) {{
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.To{1} (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}}
			return list.ToArray ();
		}}

		public virtual {0} [] Read{1}Array (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return Read{1}Array (localName.Value, namespaceUri.Value);
		}}", ToCSharp (type), type.Name);

		}

		Type xr = typeof (XmlReader);
		string name = "ReadElementContentAs";
		foreach (MethodInfo mi in xr.GetMethods ()) {
			if (!mi.Name.StartsWith (name))
				continue;
			ParameterInfo [] pl = mi.GetParameters ();
			if (pl.Length != 2 || pl [0].ParameterType != typeof (string))
				continue;
			if (mi.Name.EndsWith ("AsObject"))
				continue; // special case to filter out.
			if (mi.Name.EndsWith ("AsString"))
				continue; // special case to filter out.

			bool isOverride = xr.GetMethod (mi.Name, Type.EmptyTypes) != null;
			Console.WriteLine (@"
		public {3}{0} {1} ()
		{{
			ReadStartElement (LocalName, NamespaceURI);
			{0} ret = {2} ();
			ReadEndElement ();
			return ret;
		}}",
				ToCSharp (mi.ReturnType),
				mi.Name,
				mi.Name.Replace ("Element", String.Empty),
				isOverride ? "override " : null);
		}

		Console.WriteLine (@"
	}
}");
	}

	static CodeDomProvider cs = new CSharpCodeProvider ();

	static string ToCSharp (Type type)
	{
		string r = cs.GetTypeOutput (new CodeTypeReference (type));
		return r != type.FullName ? r : type.Name;
	}

	static string ToOldName (Type type)
	{
		switch (type.Name) {
		case "Single":
			return "Float";
		case "Int32":
			return "Int";
		case "Int64":
			return "Long";
		case "Int16":
			return "Short";
		default:
			return type.Name;
		}
	}
}

