// created on 07/04/2003 at 17:56
//
//	System.Runtime.Serialization.Formatters.Soap.SoapWriter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Globalization;
using System.Text;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal class SoapWriter: IComparer {
		private struct EnqueuedObject {
			public long _id;
			public object _object;
			
			public EnqueuedObject(object currentObject, long id) {
				_id = id;
				_object = currentObject;
			}

			public long Id 
			{
				get 
				{
					return _id;
				}
			}

			public object Object 
			{
				get 
				{
					return _object;
				}
			}
		}
		
		#region Fields

		private XmlTextWriter _xmlWriter;
		private Queue _objectQueue = new Queue();
		private Hashtable _prefixTable = new Hashtable();
		private Hashtable _objectToIdTable = new Hashtable();
		private ISurrogateSelector _surrogateSelector;
		private SoapTypeMapper _mapper;
		private StreamingContext _context;
		private ISoapMessage _soapMessage = null;
		private ObjectIDGenerator idGen = new ObjectIDGenerator();
		private FormatterAssemblyStyle _assemblyFormat = FormatterAssemblyStyle.Full;
		private FormatterTypeStyle _typeFormat = FormatterTypeStyle.TypesWhenNeeded;
		private static string defaultMessageNamespace;

		#endregion
		
		~SoapWriter() 
		{
		}

		#region Constructors

		internal SoapWriter(
			Stream outStream, 
			ISurrogateSelector selector, 
			StreamingContext context,
			ISoapMessage soapMessage)
		{
			_xmlWriter = new XmlTextWriter(outStream, null);
			_xmlWriter.Formatting = Formatting.Indented;
			_surrogateSelector = selector;
			_context = context;
			_soapMessage = soapMessage;

		}

		static SoapWriter() 
		{
			defaultMessageNamespace = typeof(SoapWriter).Assembly.GetName().FullName;
		}

		#endregion

		#region Internal Properties

		internal FormatterAssemblyStyle AssemblyFormat
		{
			get 
			{
				return _assemblyFormat;
			}
			set 
			{
				_assemblyFormat = value;
			}
		}

		internal FormatterTypeStyle TypeFormat 
		{
			get 
			{
				return _typeFormat;
			}
			set 
			{
				_typeFormat = value;
			}
		}

		#endregion

		private void Id(long id) 
		{
			_xmlWriter.WriteAttributeString(null, "id", null, "ref-" + id.ToString());
		}

		private void Href(long href) 
		{
			_xmlWriter.WriteAttributeString(null, "href", null, "#ref-" + href.ToString());
		}


		private void Null() 
		{
			_xmlWriter.WriteAttributeString("xsi", "null", XmlSchema.InstanceNamespace, "1");
		}

		private bool IsEncodingNeeded(
			object componentObject,
			Type componentType)
		{
			if(componentObject == null)
				return false;
			if(_typeFormat == FormatterTypeStyle.TypesAlways)
				return true;
			if(componentType == null) 
			{
				componentType = componentObject.GetType();
				if(componentType.IsPrimitive || componentType == typeof(string))
					return false;
				else
					return true;

			}
			else 
			{
				if(componentType == typeof(object) || componentType != componentObject.GetType())
					return true;
				else
					return false;
			}
		}


		internal void Serialize (object objGraph, Header[] headers, FormatterTypeStyle typeFormat, FormatterAssemblyStyle assemblyFormat)
		{
			_typeFormat = typeFormat;
			_assemblyFormat = assemblyFormat;
			// Create the XmlDocument with the 
			// Envelope and Body elements
			_mapper = new SoapTypeMapper(_xmlWriter, assemblyFormat, typeFormat);

			// The root element
			_xmlWriter.WriteStartElement(
				SoapTypeMapper.SoapEnvelopePrefix, 
				"Envelope",
				SoapTypeMapper.SoapEnvelopeNamespace);

			// adding namespaces
			_xmlWriter.WriteAttributeString(
				"xmlns",
				"xsi",
				"http://www.w3.org/2000/xmlns/",
				"http://www.w3.org/2001/XMLSchema-instance");

			_xmlWriter.WriteAttributeString(
				"xmlns",
				"xsd",
				"http://www.w3.org/2000/xmlns/",
				XmlSchema.Namespace);

			_xmlWriter.WriteAttributeString(
				"xmlns",
				SoapTypeMapper.SoapEncodingPrefix,
				"http://www.w3.org/2000/xmlns/",
				SoapTypeMapper.SoapEncodingNamespace);

			_xmlWriter.WriteAttributeString(
				"xmlns",
				SoapTypeMapper.SoapEnvelopePrefix,
				"http://www.w3.org/2000/xmlns/",
				SoapTypeMapper.SoapEnvelopeNamespace);

			_xmlWriter.WriteAttributeString(
				"xmlns",
				"clr",
				"http://www.w3.org/2000/xmlns/",
				SoapServices.XmlNsForClrType);

			_xmlWriter.WriteAttributeString(
				SoapTypeMapper.SoapEnvelopePrefix,
				"encodingStyle",
				SoapTypeMapper.SoapEnvelopeNamespace,
				"http://schemas.xmlsoap.org/soap/encoding/");
						
			ISoapMessage msg = objGraph as ISoapMessage;
			if (msg != null)
				headers = msg.Headers;
			
			if (headers != null && headers.Length > 0)
			{
				_xmlWriter.WriteStartElement (SoapTypeMapper.SoapEnvelopePrefix, "Header", SoapTypeMapper.SoapEnvelopeNamespace);
				foreach (Header h in headers)
					SerializeHeader (h);
					
				WriteObjectQueue ();
				_xmlWriter.WriteEndElement ();
			}
				
			// The body element
			_xmlWriter.WriteStartElement(
				SoapTypeMapper.SoapEnvelopePrefix,
				"Body",
				SoapTypeMapper.SoapEnvelopeNamespace);


			bool firstTime = false;

			if (msg != null)
				SerializeMessage(msg);
			else
				_objectQueue.Enqueue(new EnqueuedObject( objGraph, idGen.GetId(objGraph, out firstTime)));

			WriteObjectQueue ();

			_xmlWriter.WriteFullEndElement(); // the body element
			_xmlWriter.WriteFullEndElement(); // the envelope element
			_xmlWriter.Flush();
		}
		
		private void WriteObjectQueue ()
		{
			while(_objectQueue.Count > 0) 
			{
				EnqueuedObject currentEnqueuedObject;
				currentEnqueuedObject = (EnqueuedObject) _objectQueue.Dequeue();
				object currentObject =	currentEnqueuedObject.Object;
				Type currentType = currentObject.GetType();

				if(!currentType.IsValueType) _objectToIdTable[currentObject] = currentEnqueuedObject.Id;

				if(currentType.IsArray)
					SerializeArray((Array) currentObject, currentEnqueuedObject.Id);
				else
					SerializeObject(currentObject, currentEnqueuedObject.Id);
			}
		}

		private void SerializeMessage(ISoapMessage message) 
		{
			bool firstTime;
			string ns = message.XmlNameSpace != null ? message.XmlNameSpace : defaultMessageNamespace;
			
			_xmlWriter.WriteStartElement("i2", message.MethodName, ns);
			Id(idGen.GetId(message, out firstTime));

			string[] paramNames = message.ParamNames;
			Type[] paramTypes = message.ParamTypes;
			object[] paramValues = message.ParamValues;
			int length = (paramNames != null)?paramNames.Length:0;
			for(int i = 0; i < length; i++) 
			{
				_xmlWriter.WriteStartElement(paramNames[i]);
				SerializeComponent(paramValues[i], true);
				_xmlWriter.WriteEndElement();
			}


			_xmlWriter.WriteFullEndElement();
		}
				private void SerializeHeader (Header header)
		{
			string ns = header.HeaderNamespace != null ? header.HeaderNamespace : "http://schemas.microsoft.com/clr/soap"; 
			_xmlWriter.WriteStartElement ("h4", header.Name, ns);
			if (header.MustUnderstand)
				_xmlWriter.WriteAttributeString ("mustUnderstand", SoapTypeMapper.SoapEnvelopeNamespace, "1");
			_xmlWriter.WriteAttributeString ("root", SoapTypeMapper.SoapEncodingNamespace, "1");
			SerializeComponent (header.Value, true);
			_xmlWriter.WriteEndElement();
		}

		private void SerializeObject(object currentObject, long currentObjectId) 
		{
			bool needsSerializationInfo = false;
			ISurrogateSelector selector;
			ISerializationSurrogate surrogate = null;
			if(_surrogateSelector != null)
			{
				 surrogate = _surrogateSelector.GetSurrogate(
					currentObject.GetType(),
					_context,
					out selector);
			}
			if(currentObject is ISerializable || surrogate != null) needsSerializationInfo = true;
			if(needsSerializationInfo) 
			{
				SerializeISerializableObject(currentObject, currentObjectId, surrogate);
			}
			else 
			{
				if(!currentObject.GetType().IsSerializable)
					throw new SerializationException(String.Format("Type {0} in assembly {1} is not marked as serializable.", currentObject.GetType(), currentObject.GetType().Assembly.FullName));
				SerializeSimpleObject(currentObject, currentObjectId);
			}
		}

		// implement IComparer
		public int Compare(object x, object y) 
		{
			MemberInfo a = x as MemberInfo;
			MemberInfo b = y as MemberInfo;

			return String.Compare(a.Name, b.Name);
		}

		private void SerializeSimpleObject(
			object currentObject, 
			long currentObjectId) 
		{
			Type currentType = currentObject.GetType();
			
			// Value type have to be serialized "on the fly" so
			// SerializeComponent calls SerializeObject when
			// a field of another object is a struct. A node with the field
			// name has already be written so WriteStartElement must not be called
			// again. Fields that are structs are passed to SerializeObject
			// with a id = 0
			if(currentObjectId > 0)
			{
				Element element = _mapper[currentType];
				_xmlWriter.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);
				Id(currentObjectId);
			}
			if(currentType == typeof(string))
			{
				_xmlWriter.WriteString(currentObject.ToString());
			}
			else
			{
				MemberInfo[] memberInfos = 
					FormatterServices.GetSerializableMembers(currentType, _context);
				object[] objectData =
					FormatterServices.GetObjectData(currentObject, memberInfos);
//				Array.Sort(memberInfos, objectData, this);
				for(int i = 0; i < memberInfos.Length; i++) 
				{
					FieldInfo fieldInfo = memberInfos[i] as FieldInfo;
//					bool specifyEncoding = false;
//					if(objectData[i] != null)
//						 specifyEncoding = (objectData[i].GetType() != fieldInfo.FieldType);
					_xmlWriter.WriteStartElement(fieldInfo.Name);
					SerializeComponent(
						objectData[i], 
						IsEncodingNeeded(objectData[i], fieldInfo.FieldType));
					_xmlWriter.WriteEndElement();
				}
			}
			if(currentObjectId > 0)
				_xmlWriter.WriteFullEndElement();

		}
		
				private void SerializeISerializableObject(
			object currentObject,
			long currentObjectId,
			ISerializationSurrogate surrogate)
		{
			Type currentType = currentObject.GetType();
			SerializationInfo info = new SerializationInfo(currentType, new FormatterConverter());


			ISerializable objISerializable = currentObject as ISerializable;
			if(surrogate != null) surrogate.GetObjectData(currentObject, info, _context);
			else
			{
				objISerializable.GetObjectData(info, _context);
			}

			// Same as above
			if(currentObjectId > 0L)
			{
				Element element = _mapper[info.FullTypeName, info.AssemblyName];
				_xmlWriter.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);
				Id(currentObjectId);
			}

			foreach(SerializationEntry entry in info)
			{
				_xmlWriter.WriteStartElement(entry.Name);
				SerializeComponent(entry.Value, IsEncodingNeeded(entry.Value, null));
				_xmlWriter.WriteEndElement();
			}
			if(currentObjectId > 0)
				_xmlWriter.WriteFullEndElement();

		}

		private void SerializeArray(Array currentArray, long currentArrayId) 
		{
			Element element = _mapper[typeof(System.Array)];
			

			// Set the arrayType attribute
			Type arrayType = currentArray.GetType().GetElementType();
			Element xmlArrayType = _mapper[arrayType];
			_xmlWriter.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);
			if(currentArrayId > 0) Id(currentArrayId);
			
			if (arrayType == typeof(byte)) {
				EncodeType (currentArray.GetType());
				_xmlWriter.WriteString (Convert.ToBase64String ((byte[])currentArray));
				_xmlWriter.WriteFullEndElement();
				return;
			}

			if(_xmlWriter.LookupPrefix(xmlArrayType.NamespaceURI) == null)
			{
				_xmlWriter.WriteAttributeString(
					"xmlns",
					xmlArrayType.Prefix,
					"http://www.w3.org/2000/xmlns/",
					xmlArrayType.NamespaceURI);
			}

			StringBuilder str = new StringBuilder();
			str.AppendFormat("{0}:{1}[",xmlArrayType.Prefix, xmlArrayType.LocalName);
			for(int i = 0; i < currentArray.Rank; i++)
			{
				str.AppendFormat("{0},", currentArray.GetUpperBound(i) + 1);
			}
			str.Replace(',', ']', str.Length - 1, 1);
			_xmlWriter.WriteAttributeString(
				SoapTypeMapper.SoapEncodingPrefix,
				"arrayType",
				SoapTypeMapper.SoapEncodingNamespace,
				str.ToString());
				

			// Get the array items
			int lastNonNullItem = 0;
			int currentIndex = 0;
//			bool specifyEncoding = false;
			foreach(object item in currentArray) 
			{
				if(item != null)
				{
					for(int j = lastNonNullItem; j < currentIndex; j++)
					{
						_xmlWriter.WriteStartElement("item");
						Null();
						_xmlWriter.WriteEndElement();
					}; 
					lastNonNullItem = currentIndex + 1;
//					specifyEncoding |= (arrayType != item.GetType());
					_xmlWriter.WriteStartElement("item");
					SerializeComponent(item, IsEncodingNeeded(item, arrayType));
					_xmlWriter.WriteEndElement();
				}
				currentIndex++;
			}
			_xmlWriter.WriteFullEndElement();
			
		}

		private void SerializeComponent(
			object obj,
			bool specifyEncoding)
		{
			if(_typeFormat == FormatterTypeStyle.TypesAlways)
				specifyEncoding = true;

			// A null component
			if(obj == null)
			{
				Null();
				return;
			}
			Type objType = obj.GetType();
			bool canBeValue = SoapTypeMapper.CanBeValue(objType);
			bool firstTime;
			long id = 0;

			// An object already serialized
			if((id = idGen.HasId(obj, out firstTime)) != 0L) 
			{
				Href((long)idGen.GetId(obj, out firstTime));
				return;
			}



			// A string
			if(objType == typeof(string)) 
			{
				if(_typeFormat != FormatterTypeStyle.XsdString)
				{
					id = idGen.GetId(obj, out firstTime);
					Id(id);
				}
//				specifyEncoding = false;
			}

			// This component has to be 
			// serialized later
			if(!canBeValue && !objType.IsValueType)
			{
				long href = idGen.GetId(obj, out firstTime);
				Href(href);
				_objectQueue.Enqueue(new EnqueuedObject(obj, href));
				return;
			}

			if(specifyEncoding)
			{
				EncodeType(objType);
			}

			// A struct
			if(!canBeValue && objType.IsValueType)
			{
				SerializeObject(obj, 0);
				return;
			}

			_xmlWriter.WriteString (SoapTypeMapper.GetXsdValue (obj));
		}
		
		private void EncodeType(Type type) 
		{
			if(type == null) 
				throw new SerializationException("Oooops");

			Element xmlType = _mapper[type];

			_xmlWriter.WriteAttributeString(
				"xsi",
				"type",
				"http://www.w3.org/2001/XMLSchema-instance",
				xmlType.Prefix + ":" + xmlType.LocalName);
			string prefix = _xmlWriter.LookupPrefix(xmlType.NamespaceURI);
			if(prefix == null || prefix == string.Empty) 
			{
				_xmlWriter.WriteAttributeString(
					"xmlns",
					xmlType.Prefix,
					"http://www.w3.org/2000/xmlns/",
					xmlType.NamespaceURI);

			}

		}
	}
}
