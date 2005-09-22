using System;
using System.IO;
using System.Xml;

namespace System.Runtime.Serialization
{
	public sealed class XmlFormatter : IFormatter
	{
		StreamingContext context;
		SerializationMode mode;
		KnownTypeCollection knownTypes;
		IDataContractSurrogate surrogate;
		int maxItems;

		public XmlFormatter ()
		{
		}

		public XmlFormatter (SerializationMode mode)
		{
		}

		public XmlFormatter (StreamingContext context)
		{
		}

		public XmlFormatter (SerializationMode mode,
			StreamingContext context)
		{
		}

		public XmlFormatter (SerializationMode mode,
			StreamingContext context, KnownTypeCollection knownTypes)
		{
		}

		SerializationBinder IFormatter.Binder {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		ISurrogateSelector IFormatter.SurrogateSelector {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public StreamingContext Context {
			get { return context; }
			set { context = value; }
		}

		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
			set { surrogate = value; }
		}

		public KnownTypeCollection KnownTypes {
			get { return knownTypes; }
		}

		public int MaxItemsInObjectGraph {
			get { return maxItems; }
			set { maxItems= value; }
		}

		public SerializationMode Mode {
			get { return mode; }
		}

		object IFormatter.Deserialize (Stream stream)
		{
			return Deserialize (stream, null);
		}

		public object Deserialize (Stream stream, Type type)
		{
			return Deserialize (XmlReader.Create (stream), type);
		}

		public object Deserialize (XmlReader reader, Type type)
		{
			return Deserialize (reader, type, false);
		}

		public object Deserialize (XmlReader reader, Type type, bool readContentOnly)
		{
			throw new NotImplementedException ();
		}

		public T Deserialize<T> (Stream stream)
		{
			return (T) Deserialize (XmlReader.Create (stream), typeof (T));
		}

		public T Deserialize<T> (XmlReader reader)
		{
			return (T) Deserialize (reader, typeof (T), false);
		}

		public T Deserialize<T> (XmlReader reader, bool readContentOnly)
		{
			return (T) Deserialize (reader, typeof (T), readContentOnly);
		}

		public void Serialize (Stream stream, object graph)
		{
			Serialize (XmlWriter.Create (stream), graph);
		}

		public void Serialize (XmlWriter writer, object graph)
		{
			Serialize (writer, graph, null, true, false, true);
		}

		public void Serialize (XmlWriter writer, object graph,
			Type rootType, bool preserveObjectReferences,
			bool writeContentOnly,
			bool ignoreUnknownSerializationData)
		{
			throw new NotImplementedException ();
		}
	}
}
