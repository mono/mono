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
	public class SoapFormatter : IFormatter, IRemoteFormatter
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
			ObjSerializer= new ObjectSerializer(serializationStream);
			ObjSerializer.BeginWrite();
			ObjSerializer.Serialize(graph);
		}

		public object Deserialize(Stream serializationStream)			
		{
			ObjDeserializer= new ObjectDeserializer(serializationStream);
			return ObjDeserializer.Deserialize(serializationStream);
		}
		
	}
}
