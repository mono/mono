// BinaryFormatter.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//  Lluis Sanchez Gual (lsg@ctv.es)
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
		private FormatterAssemblyStyle assembly_format;
		private SerializationBinder binder;
		private StreamingContext context;
		private ISurrogateSelector surrogate_selector;
		private FormatterTypeStyle type_format;
		
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
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}
			if(serializationStream.CanSeek &&
			   serializationStream.Length==0) {
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}

			ObjectReader serializer = new ObjectReader (surrogate_selector, context);
			BinaryReader reader = new BinaryReader (serializationStream);
			return serializer.ReadObjectGraph (reader);
		}

		[MonoTODO]
		public object Deserialize(Stream serializationStream, HeaderHandler handler) 
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}
			if(serializationStream.CanSeek &&
			   serializationStream.Length==0) {
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}
			
			return(null);
		}
		
		[MonoTODO]
		public object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallmessage)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}
			if(serializationStream.CanSeek &&
			   serializationStream.Length==0) {
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}
			
			return(null);
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}

			ObjectWriter serializer = new ObjectWriter (surrogate_selector, context);
			BinaryWriter writer = new BinaryWriter (serializationStream);
			serializer.WriteObjectGraph (writer, graph);
			writer.Flush();
		}

		[MonoTODO]
		public void Serialize(Stream serializationStream, object graph, Header[] headers)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream is null");
			}

			// fixme: what about headers?
			Serialize (serializationStream, graph);			
		}
	}
}
