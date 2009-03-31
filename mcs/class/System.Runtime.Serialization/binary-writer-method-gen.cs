// mono binary-writer-method-gen.exe > System.Xml/XmlBinaryDictionaryWriterAutoGen.cs
using System;
using System.Collections.Generic;
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

		Dictionary<Type,byte> table = new Dictionary<Type,byte> ();
		// LAMESPEC: [MC-NBFX] section 2.3.3 dedscribes wrong RecordTypes.
		table.Add (typeof (bool), 0xB5);
		table.Add (typeof (short), 0x8B);
		table.Add (typeof (int), 0x8D);
		table.Add (typeof (long), 0x8F);
		table.Add (typeof (float), 0x91);
		table.Add (typeof (double), 0x93);
		table.Add (typeof (decimal), 0x95);
		table.Add (typeof (DateTime), 0x97);
		table.Add (typeof (TimeSpan), 0xAF);
		table.Add (typeof (Guid), 0xB1);

		Console.WriteLine (@"
using System;
using BF = System.Xml.XmlBinaryFormat;

namespace System.Xml
{
	internal partial class XmlBinaryDictionaryWriter : XmlDictionaryWriter
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

		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}}

		public override void WriteArray (string prefix, string localName, string namespaceUri, {0} [] array, int offset, int length)
		{{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}}

		void WriteArrayRemaining ({0} [] array, int offset, int length)
		{{
			writer.Write ((byte) 0x{2:X02}); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}}", ToCSharp (type), type.Name, table [type]);

		// <note>
		// WriteArrayRemaining() is generated, but are modified and moved into 
		// XmlBinaryDictionaryWriter. (I keep it open here so that we
		// make sure to remove this before compiling. Remove this to get
		// it working fine).
		// </note>


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

