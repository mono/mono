// created on 07/04/2003 at 17:16
//
//	System.Runtime.Serialization.Formatters.Soap.SoapFormatter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;


namespace System.Runtime.Serialization.Formatters.Soap {
	enum RemMessageType {
		MethodCall, MethodResponse, ServerFault, NotRecognize
	}
	
	public class SoapFormatter: IRemotingFormatter, IFormatter {
		private ObjectWriter _objWriter;
		private SoapWriter _soapWriter;
		private SerializationBinder _binder;
		private StreamingContext _context;
		private ISurrogateSelector _selector;
		private ISoapMessage _topObject;
		
		public SoapFormatter() {
			
		}
		
		public SoapFormatter(ISurrogateSelector selector, StreamingContext context):this() {
			_selector = selector;
			_context = context;
		}
		
		~SoapFormatter() {
		}
		
		public object Deserialize(Stream serializationStream) {
			return Deserialize(serializationStream, null);
		}
		
		public object Deserialize(Stream serializationStream, HeaderHandler handler) {
			SoapParser parser = new SoapParser(serializationStream);
			SoapReader soapReader = new SoapReader(parser);
			
			
			if(_topObject != null) soapReader.TopObject = _topObject;
			ObjectReader reader = new ObjectReader(_selector, _context, soapReader);
			parser.Run();
			object objReturn = reader.TopObject;
			if(objReturn is SoapMessage) FixupSoapMessage((SoapMessage)objReturn);
			return objReturn;
		}
		
		
		
		public void Serialize(Stream serializationStream, object graph) {
			Serialize(serializationStream, graph, null);
		}
		
		public void Serialize(Stream serializationStream, object graph, Header[] headers) {
			if(serializationStream == null)
				throw new ArgumentNullException("serializationStream");
			if(!serializationStream.CanWrite)
				throw new SerializationException("Can't write in the serialization stream");
			_soapWriter = new SoapWriter(serializationStream);
			_objWriter = new ObjectWriter((ISoapWriter) _soapWriter, _selector,  new StreamingContext(StreamingContextStates.File));
			_soapWriter.Writer = _objWriter;
			_objWriter.Serialize(graph);
			
		}
		
		public ISurrogateSelector SurrogateSelector {
			get {
				return _selector;
			}
			set {
				_selector = value;
			}
		}
		
		
		public SerializationBinder Binder {
			get {
				return _binder;
			}
			set {
				_binder = value;
			}
		}
		
		public StreamingContext Context {
			get {
				return _context;
			}
			set {
				_context = value;
			}
		}
		
		public ISoapMessage TopObject {
			get {
				return _topObject;
			}
			set {
				_topObject = value;
			}
		}
		
	
		//private methods
		
		// finish the work on the SoapMessage
		// fill the SoapMessage.ParamName array
		// fill the SoapMessage.ParamType array
		// convert the SoapMessage.ParamValue array items
		// to the right type if needed
		private void FixupSoapMessage(ISoapMessage msg) {
			string typeNamespace, assemblyName;
			
			SoapServices.DecodeXmlNamespaceForClrTypeNamespace(msg.XmlNameSpace, out typeNamespace, out assemblyName);
			try{
				RemMessageType messageType;
				MethodInfo mi = GetMethodInfo(msg.MethodName, msg.XmlNameSpace, out messageType);
				
				FormatterConverter conv = new FormatterConverter();
				switch(messageType) {
					case RemMessageType.MethodCall:
						int nbOutParams = 0;
						ParameterInfo[] paramInfo = mi.GetParameters();
						foreach(ParameterInfo param in paramInfo) {
							// The *out* parameters aren't serialized
							if(!param.IsOut) {
								msg.ParamNames[param.Position - nbOutParams] = param.Name;
								msg.ParamTypes[param.Position - nbOutParams] = param.ParameterType;
								if(msg.ParamValues[param.Position - nbOutParams] is IConvertible) msg.ParamValues[param.Position - nbOutParams] = conv.Convert(msg.ParamValues[param.Position - nbOutParams], param.ParameterType);
							}
							else nbOutParams++;
						}
						break;
					case RemMessageType.MethodResponse:
						int offset = 0;
						if(mi.ReturnType != typeof(void)) {
							msg.ParamNames[0] = "return";
							msg.ParamTypes[0] = mi.ReturnType;
							if(msg.ParamValues[0] is IConvertible) msg.ParamValues[0] = Convert.ChangeType(msg.ParamValues[0], mi.ReturnType);
							offset++;
						}
						paramInfo = mi.GetParameters();
						foreach(ParameterInfo param in paramInfo) {
							// The *out* parameters
							if(param.IsOut) {
								msg.ParamNames[offset] = param.Name;
								//msg.ParamTypes[offset] = param.ParameterType;
								//if(msg.ParamValues[offset] is IConvertible) msg.ParamValues[param.Position] = conv.Convert(msg.ParamValues[offset], param.ParameterType);
								offset++;
							}
						}
						break;
					case RemMessageType.ServerFault:
						break;
					case RemMessageType.NotRecognize:
						 throw new SerializationException(String.Format("Can't find a method with name {0} for {1}", msg.MethodName, msg.XmlNameSpace));
					
				}
			}
			catch(Exception e) {
				// Last chance
				// maybe it is a SoapFault object
				if(msg.MethodName != "Fault") throw new SerializationException("Don't understand the response from the server");
				else return;
			}
			
		}
		
		// Get the MethodInfo
		internal static MethodInfo GetMethodInfo(string methodName, string xmlNamespace, out RemMessageType messageType) {
			string typeNamespace, assemblyName;
			bool result = SoapServices.DecodeXmlNamespaceForClrTypeNamespace(xmlNamespace, out typeNamespace, out assemblyName);
			return GetMethodInfo(methodName, typeNamespace, assemblyName, out messageType);
		}
		
		internal static MethodInfo GetMethodInfo(string methodName, string typeName, string assemblyName, out RemMessageType messageType) {
			Type type = GetType(typeName, assemblyName);
			MethodInfo mi = null;
			messageType = RemMessageType.MethodCall;
			if(type != null) {
				mi = type.GetMethod(methodName);
				
				if(mi == null) {
					int index = methodName.LastIndexOf("Response");
					string methodResponseName = methodName.Remove(index, methodName.Length - index);
					mi = type.GetMethod(methodResponseName);
					if(mi == null) {
						if(methodName != "Fault") messageType = RemMessageType.NotRecognize;
						else messageType = RemMessageType.ServerFault;
					} 
					messageType = RemMessageType.MethodResponse;
				}
			}
			return mi;
		}
		
		internal static Type GetType(string typeName, string assemblyName) {
			Type type = Type.GetType(typeName);
			if(type == null) {
				AssemblyName assName = new AssemblyName();
				assName.Name = assemblyName;
				Assembly ass = Assembly.Load(assName);
				type = FormatterServices.GetTypeFromAssembly(ass, typeName);
				if(type == null) throw new SerializationException(String.Format("Can't find type {0} in assembly {1}", typeName, assemblyName));
			}
			return type;
		}
		
	}
}
