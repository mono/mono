// mono writer-method-gen.exe > System.Xml/XmlDictionaryWriterAutoGen.cs
using System;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
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
using System;

namespace System.Xml
{
	public abstract partial class XmlDictionaryWriter : XmlWriter
	{
		void CheckWriteArrayArguments (Array array, int offset, int length)
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
		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {{
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}}

		}}", ToCSharp (type), type.Name);

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
}

