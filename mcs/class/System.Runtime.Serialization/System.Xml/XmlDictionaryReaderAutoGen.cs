
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


		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, bool [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToBoolean (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual bool [] ReadBooleanArray (string localName, string namespaceUri)
		{
			List<bool> list = new List<bool> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToBoolean (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual bool [] ReadBooleanArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadBooleanArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, DateTime [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToDateTime (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual DateTime [] ReadDateTimeArray (string localName, string namespaceUri)
		{
			List<DateTime> list = new List<DateTime> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToDateTime (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual DateTime [] ReadDateTimeArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadDateTimeArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, decimal [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToDecimal (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual decimal [] ReadDecimalArray (string localName, string namespaceUri)
		{
			List<decimal> list = new List<decimal> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToDecimal (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual decimal [] ReadDecimalArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadDecimalArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, double [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, double [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToDouble (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual double [] ReadDoubleArray (string localName, string namespaceUri)
		{
			List<double> list = new List<double> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToDouble (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual double [] ReadDoubleArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadDoubleArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, Guid [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToGuid (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual Guid [] ReadGuidArray (string localName, string namespaceUri)
		{
			List<Guid> list = new List<Guid> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToGuid (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual Guid [] ReadGuidArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadGuidArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, short [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, short [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToInt16 (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual short [] ReadInt16Array (string localName, string namespaceUri)
		{
			List<short> list = new List<short> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToInt16 (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual short [] ReadInt16Array (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadInt16Array (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, int [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, int [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToInt32 (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual int [] ReadInt32Array (string localName, string namespaceUri)
		{
			List<int> list = new List<int> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToInt32 (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual int [] ReadInt32Array (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadInt32Array (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, long [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, long [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToInt64 (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual long [] ReadInt64Array (string localName, string namespaceUri)
		{
			List<long> list = new List<long> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToInt64 (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual long [] ReadInt64Array (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadInt64Array (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, float [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, float [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToSingle (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual float [] ReadSingleArray (string localName, string namespaceUri)
		{
			List<float> list = new List<float> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToSingle (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual float [] ReadSingleArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadSingleArray (localName.Value, namespaceUri.Value);
		}

		public virtual int ReadArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadArray (localName.Value, namespaceUri.Value, array, offset, length);
		}

		public virtual int ReadArray (string localName, string namespaceUri, TimeSpan [] array, int offset, int length)
		{
			CheckReadArrayArguments (array, offset, length);
			for (int i = 0; i < length; i++) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					return i;
				ReadStartElement (localName, namespaceUri);
				array [offset + i] = XmlConvert.ToTimeSpan (ReadContentAsString ());
				ReadEndElement ();
			}
			return length;
		}

		public virtual TimeSpan [] ReadTimeSpanArray (string localName, string namespaceUri)
		{
			List<TimeSpan> list = new List<TimeSpan> ();
			while (true) {
				MoveToContent ();
				if (NodeType != XmlNodeType.Element)
					break;
				ReadStartElement (localName, namespaceUri);
				list.Add (XmlConvert.ToTimeSpan (ReadContentAsString ()));
				ReadEndElement ();
				if (list.Count == Quotas.MaxArrayLength)
					// FIXME: check if raises an error or not
					break;
			}
			return list.ToArray ();
		}

		public virtual TimeSpan [] ReadTimeSpanArray (XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			CheckDictionaryStringArgs (localName, namespaceUri);
			return ReadTimeSpanArray (localName.Value, namespaceUri.Value);
		}

		public override bool ReadElementContentAsBoolean ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			bool ret = ReadContentAsBoolean ();
			ReadEndElement ();
			return ret;
		}

		public override DateTime ReadElementContentAsDateTime ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			DateTime ret = ReadContentAsDateTime ();
			ReadEndElement ();
			return ret;
		}

		public override decimal ReadElementContentAsDecimal ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			decimal ret = ReadContentAsDecimal ();
			ReadEndElement ();
			return ret;
		}

		public override double ReadElementContentAsDouble ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			double ret = ReadContentAsDouble ();
			ReadEndElement ();
			return ret;
		}

		public override float ReadElementContentAsFloat ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			float ret = ReadContentAsFloat ();
			ReadEndElement ();
			return ret;
		}

		public override int ReadElementContentAsInt ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			int ret = ReadContentAsInt ();
			ReadEndElement ();
			return ret;
		}

		public override long ReadElementContentAsLong ()
		{
			ReadStartElement (LocalName, NamespaceURI);
			long ret = ReadContentAsLong ();
			ReadEndElement ();
			return ret;
		}

	}
}
