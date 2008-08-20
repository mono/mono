
using System;

namespace System.Xml
{
	public abstract partial class XmlDictionaryWriter : XmlWriter
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


		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, bool [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, decimal [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, double [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, Guid [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, short [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, int [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, long [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, float [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

		public virtual void WriteArray (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			WriteArray (prefix, localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual void WriteArray (string prefix, string localName, string namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckWriteArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				WriteStartElement (prefix, localName, namespaceUri);
				WriteValue (array [offset + i]);
				WriteEndElement ();
			}

		}

	}
}
