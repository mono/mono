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
using System.Threading;
using System.Globalization;


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
		private FormatterAssemblyStyle _assemblyFormat = FormatterAssemblyStyle.Full;
		private ISoapMessage _topObject;
		
#if NET_1_1
		TypeFilterLevel _filterLevel = TypeFilterLevel.Low;
#endif

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
			object objReturn = null;
			SoapParser parser = new SoapParser(serializationStream);
			SoapReader soapReader = new SoapReader(parser);
			soapReader.Binder = _binder;
			
			if(_topObject != null) soapReader.TopObject = _topObject;
			ObjectReader reader = new ObjectReader(_selector, _context, soapReader);
			// Use the english numeral and date format during the serialization
			// and the deserialization.
			// The original CultureInfo is restored when the operation
			// is done
			CultureInfo savedCi = CultureInfo.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				parser.Run();
				objReturn = reader.TopObject;
			}
			finally {
				Thread.CurrentThread.CurrentCulture = savedCi;
			}
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

			// Use the english numeral and date format during the serialization
			// and the deserialization.
			// The original CultureInfo is restored when the operation
			// is done
			CultureInfo savedCi = CultureInfo.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				_objWriter.Serialize(graph);
			}
			finally {
				Thread.CurrentThread.CurrentCulture = savedCi;
			}
			
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
		
#if NET_1_1
		[MonoTODO ("Interpret this")]
		public TypeFilterLevel FilterLevel {
			get {
				return _filterLevel;
			}
			set {
				_filterLevel = value;
			}
		}
#endif
		
		[MonoTODO ("Interpret this")]
		public FormatterAssemblyStyle AssemblyFormat
		{
			get {
				return _assemblyFormat;
			}
			set {
				_assemblyFormat = value;
			}
		}

	}
}
