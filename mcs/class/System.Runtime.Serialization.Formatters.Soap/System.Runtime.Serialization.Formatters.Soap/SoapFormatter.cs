/****************************************************/
/*Soapformatter class implementation                */
/*Author: Jesús M. Rodríguez de la Vega             */
/*gsus@brujula.net                                  */
/****************************************************/

using System;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;


namespace System.Runtime.Serialization.Formatters.soap
{
	public class SoapFormatter : IRemotingFormatter, IFormatter
	{
		private ObjectSerializer   ObjSerializer;	
		private ObjectDeserializer ObjDeserializer;
		/*this is the soapformater's properties               
		  the Binder, Context and SurrogateSelector properties
		  have not been declared yet*/

		public FormatterAssemblyStyle AssemblyFormat
		{
			get{return AssemblyFormat;}
			set{AssemblyFormat= value;}
		}

		[MonoTODO]
		public SerializationBinder Binder {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public StreamingContext Context {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ISurrogateSelector SurrogateSelector {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public ISoapMessage TopObject
		{
			get{return TopObject;}
			set{TopObject= value;}
		}

		public FormatterTypeStyle TypeFormat
		{
			get{return TypeFormat;}
			set{TypeFormat= value;}
		}

		//the other constructor are not supplied yet
		public SoapFormatter()
		{			
		}
        		
		public void Serialize(Stream serializationStream, object graph)
		{
			Serialize (serializationStream, graph, null);
		}

		public void Serialize(Stream serializationStream, object graph, Header[] headers)
		{
			ObjSerializer= new ObjectSerializer(serializationStream);
			ObjSerializer.BeginWrite();
			ObjSerializer.Serialize(graph);
		}

		public object Deserialize(Stream serializationStream)
		{
			return Deserialize (serializationStream, null);
		}

		public object Deserialize(Stream serializationStream, HeaderHandler handler)
		{
			ObjDeserializer= new ObjectDeserializer(serializationStream);
			return ObjDeserializer.Deserialize(serializationStream);
		}
		
	}
}
