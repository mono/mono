// BinaryFormatter.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//  Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com

using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Serialization.Formatters.Binary {
	public sealed class BinaryFormatter : IRemotingFormatter, IFormatter 
	{
		private FormatterAssemblyStyle assembly_format;	// TODO: Do something with this
		private SerializationBinder binder;
		private StreamingContext context;
		private ISurrogateSelector surrogate_selector;
		private FormatterTypeStyle type_format;			// TODO: Do something with this
		
		public BinaryFormatter()
		{
			surrogate_selector=null;
			context=new StreamingContext(StreamingContextStates.All);
		}
		
		public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
		{
			surrogate_selector=selector;
			this.context=context;
		}

		public FormatterAssemblyStyle AssemblyFormat
		{
			get {
				return(assembly_format);
			}
			set {
				assembly_format=value;
			}
		}

		public SerializationBinder Binder
		{
			get {
				return(binder);
			}
			set {
				binder=value;
			}
		}

		public StreamingContext Context 
		{
			get {
				return(context);
			}
			set {
				context=value;
			}
		}
		
		public ISurrogateSelector SurrogateSelector 
		{
			get {
				return(surrogate_selector);
			}
			set {
				surrogate_selector=value;
			}
		}
		
		public FormatterTypeStyle TypeFormat 
		{
			get {
				return(type_format);
			}
			set {
				type_format=value;
			}
		}

		public object Deserialize(Stream serializationStream)
		{
			return Deserialize (serializationStream, null);
		}

		public object Deserialize(Stream serializationStream, HeaderHandler handler) 
		{
			if(serializationStream==null) 
			{
				throw new ArgumentNullException("serializationStream is null");
			}
			if(serializationStream.CanSeek &&
				serializationStream.Length==0) 
			{
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}

			BinaryReader reader = new BinaryReader (serializationStream);

			bool hasHeader;
			ReadBinaryHeader (reader, out hasHeader);

			// Messages are read using a special static method, which does not use ObjectReader
			// if it is not needed. This saves time and memory.

			BinaryElement elem = (BinaryElement) reader.PeekChar();

			if (elem == BinaryElement.MethodCall) {
				return MessageFormatter.ReadMethodCall (reader, hasHeader, handler, surrogate_selector, context, binder);
			}
			else if (elem == BinaryElement.MethodResponse) {
				return MessageFormatter.ReadMethodResponse (reader, hasHeader, handler, null, surrogate_selector, context, binder);
			}
			else {
				ObjectReader serializer = new ObjectReader (surrogate_selector, context, binder);

				object result;
				Header[] headers;
				serializer.ReadObjectGraph (reader, hasHeader, out result, out headers);
				if (handler != null) handler(headers);
				return result;
			}
		}
		
		public object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallmessage)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}
			if(serializationStream.CanSeek &&
			   serializationStream.Length==0) {
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}

			BinaryReader reader = new BinaryReader (serializationStream);

			bool hasHeader;
			ReadBinaryHeader (reader, out hasHeader);
			return MessageFormatter.ReadMethodResponse (reader, hasHeader, handler, methodCallmessage, surrogate_selector, context, binder);
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			Serialize (serializationStream, graph, null);
		}

		public void Serialize(Stream serializationStream, object graph, Header[] headers)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}

			BinaryWriter writer = new BinaryWriter (serializationStream);
			WriteBinaryHeader (writer, headers!=null);

			if (graph is IMethodCallMessage) {
				MessageFormatter.WriteMethodCall (writer, graph, headers, surrogate_selector, context);
			}
			else if (graph is IMethodReturnMessage)  {
				MessageFormatter.WriteMethodResponse (writer, graph, headers, surrogate_selector, context);
			}
			else {
				ObjectWriter serializer = new ObjectWriter (surrogate_selector, context);
				serializer.WriteObjectGraph (writer, graph, headers);
			}
			writer.Flush();
		}

		private void WriteBinaryHeader (BinaryWriter writer, bool hasHeaders)
		{
			writer.Write ((byte)BinaryElement.Header);
			writer.Write ((int)1);
			if (hasHeaders) writer.Write ((int)2);
			else writer.Write ((int)-1);
			writer.Write ((int)1);
			writer.Write ((int)0);
		}

		private void ReadBinaryHeader (BinaryReader reader, out bool hasHeaders)
		{
			reader.ReadByte();
			reader.ReadInt32();
			int val = reader.ReadInt32();
			hasHeaders = (val==2);
			reader.ReadInt32();
			reader.ReadInt32();
		}
	}
}
