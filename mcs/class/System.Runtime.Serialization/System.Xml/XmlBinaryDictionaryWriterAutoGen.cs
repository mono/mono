
using System;
using BF = System.Xml.XmlBinaryFormat;

namespace System.Xml
{
	internal partial class XmlBinaryDictionaryWriter : XmlDictionaryWriter
	{
		void CheckWriteArrayArguments (Array array, int offset, int length)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset is negative");
			if (offset > array.Length)
				throw new ArgumentOutOfRangeException ("offset exceeds the length of the destination array");
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length is negative");
			if (length > array.Length - offset)
				throw new ArgumentOutOfRangeException ("length + offset exceeds the length of the destination array");
		}

		void CheckDictionaryStringArgs (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");
		}



		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, bool [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (bool [] array, int offset, int length)
		{
			writer.Write ((byte) 0xB5); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (DateTime [] array, int offset, int length)
		{
			writer.Write ((byte) 0x97); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, decimal [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (decimal [] array, int offset, int length)
		{
			writer.Write ((byte) 0x95); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, double [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (double [] array, int offset, int length)
		{
			writer.Write ((byte) 0x93); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, Guid [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (Guid [] array, int offset, int length)
		{
			writer.Write ((byte) 0xB1); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, short [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (short [] array, int offset, int length)
		{
			writer.Write ((byte) 0x8B); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, int [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (int [] array, int offset, int length)
		{
			writer.Write ((byte) 0x8D); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, long [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (long [] array, int offset, int length)
		{
			writer.Write ((byte) 0x8F); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, float [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (float [] array, int offset, int length)
		{
			writer.Write ((byte) 0x91); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}


		public override void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		public override void WriteArray (string prefix, string localName, string namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			writer.Write (BF.Array);
			WriteStartElement (prefix, localName, namespaceUri);
			WriteEndElement ();
			WriteArrayRemaining (array, offset, length);
		}

		void WriteArrayRemaining (TimeSpan [] array, int offset, int length)
		{
			writer.Write ((byte) 0xAF); // ident
			writer.WriteFlexibleInt (length);
			for (int i = offset; i < offset + length; i++)
				WriteValueContent (array [i]);
		}

	}
}
