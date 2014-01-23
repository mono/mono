// created on 24/04/2003 at 15:35
//
//	System.Runtime.Serialization.Formatters.Soap.SoapReader
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
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Serialization.Formatters.Soap {
	internal sealed class SoapReader {

		#region Fields

		private SerializationBinder _binder;
		private SoapTypeMapper mapper;
		private ObjectManager objMgr;
		private StreamingContext _context;
		private long _nextAvailableId = long.MaxValue;
		private ISurrogateSelector _surrogateSelector;
		private XmlTextReader xmlReader;
		private Hashtable _fieldIndices;
		private long _topObjectId = 1;
		
		class TypeMetadata
		{
			public MemberInfo[] MemberInfos;
			public Hashtable Indices;
		}

		#endregion

		#region Properties

		private long NextAvailableId
		{
			get 
			{
				_nextAvailableId--;
				return _nextAvailableId;
			}
		}

		#endregion

		#region Constructors
		
		public SoapReader(SerializationBinder binder, ISurrogateSelector selector, StreamingContext context) 
		{
			_binder = binder;
			objMgr = new ObjectManager(selector, context);
			_context = context;
			_surrogateSelector = selector;
			_fieldIndices = new Hashtable();
		}

		#endregion

		#region Public Methods

		public object Deserialize(Stream inStream, ISoapMessage soapMessage)
		{
			var savedCi = CultureInfo.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Deserialize_inner(inStream, soapMessage);
			}
			finally {
				Thread.CurrentThread.CurrentCulture = savedCi;
			}

			return TopObject;
		}

		void Deserialize_inner(Stream inStream, ISoapMessage soapMessage)
		{
			ArrayList headers = null;
			xmlReader = new XmlTextReader(inStream);
			xmlReader.WhitespaceHandling = WhitespaceHandling.None;
			mapper = new SoapTypeMapper(_binder);

			try
			{
				// SOAP-ENV:Envelope
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement ();
				xmlReader.MoveToContent();
				
				// Read headers
				while (!(xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == SoapTypeMapper.SoapEnvelopeNamespace))
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Header" && xmlReader.NamespaceURI == SoapTypeMapper.SoapEnvelopeNamespace)
					{
						if (headers == null) headers = new ArrayList ();
						DeserializeHeaders (headers);
					}
					else
						xmlReader.Skip ();
					xmlReader.MoveToContent();
				}
				
				// SOAP-ENV:Body
				xmlReader.ReadStartElement();
				xmlReader.MoveToContent();

				// The root object
				if (soapMessage != null)
				{
					if (DeserializeMessage (soapMessage)) {
						_topObjectId = NextAvailableId;
						RegisterObject (_topObjectId, soapMessage, null, 0, null, null);
					}
					xmlReader.MoveToContent();
					
					if (headers != null)
						soapMessage.Headers = (Header[]) headers.ToArray (typeof(Header));
				}
				
				while (xmlReader.NodeType != XmlNodeType.EndElement)
					Deserialize();
					
				// SOAP-ENV:Body
				xmlReader.ReadEndElement ();
				xmlReader.MoveToContent();

				// SOAP-ENV:Envelope
				xmlReader.ReadEndElement ();
			}
			finally 
			{
				if(xmlReader != null) xmlReader.Close();
			}
		}
		
		#endregion
		
		public SoapTypeMapper Mapper {
			get { return mapper; }
		}
		
		public XmlTextReader XmlReader {
			get { return xmlReader; }
		}

		#region Private Methods

		private object TopObject 
		{
			get 
			{
				objMgr.DoFixups();
				objMgr.RaiseDeserializationEvent();
				return objMgr.GetObject(_topObjectId);
			}
		}

		private bool IsNull()
		{
			string tmp = xmlReader["null", XmlSchema.InstanceNamespace];
			return (tmp == null || tmp == string.Empty)?false:true;
		}

		private long GetId()
		{
			long id = 0;

			string strId = xmlReader["id"];
			if(strId == null || strId == String.Empty) return 0;
			id = Convert.ToInt64(strId.Substring(4));
			return id;
		}

		private long GetHref()
		{
			long href = 0;
			
			string strHref = xmlReader["href"];
			if(strHref == null || strHref == string.Empty) return 0;
			href = Convert.ToInt64(strHref.Substring(5));
			return href;
		}

		private Type GetComponentType()
		{
			string strValue = xmlReader["type", XmlSchema.InstanceNamespace];
			if(strValue == null) {
				if(GetId() != 0) return typeof(string);
				return null;
			}
			return GetTypeFromQName (strValue);
		}

		private bool DeserializeMessage(ISoapMessage message) 
		{
			string typeNamespace, assemblyName;

			if(xmlReader.Name == SoapTypeMapper.SoapEnvelopePrefix + ":Fault")
			{
				Deserialize();
				return false;
			}

			SoapServices.DecodeXmlNamespaceForClrTypeNamespace(
				xmlReader.NamespaceURI,
				out typeNamespace,
				out assemblyName);
			message.MethodName = xmlReader.LocalName;
			message.XmlNameSpace = xmlReader.NamespaceURI;

			ArrayList paramNames = new ArrayList();
			ArrayList paramValues = new ArrayList();
			long paramValuesId = NextAvailableId;
			int[] indices = new int[1];

			if (!xmlReader.IsEmptyElement)
			{
				int initialDepth = xmlReader.Depth;
				xmlReader.Read();
				int i = 0;
				while(xmlReader.Depth > initialDepth) 
				{
					long paramId, paramHref;
					object objParam = null;
					paramNames.Add (xmlReader.Name);
					Type paramType = null;
					
					if (message.ParamTypes != null) {
						if (i >= message.ParamTypes.Length)
							throw new SerializationException ("Not enough parameter types in SoapMessages");
						paramType = message.ParamTypes [i];
					}
					
					indices[0] = i;
					objParam = DeserializeComponent(
						paramType,
						out paramId,
						out paramHref,
						paramValuesId,
						null,
						indices);
					indices[0] = paramValues.Add(objParam);
					if(paramHref != 0) 
					{
						RecordFixup(paramValuesId, paramHref, paramValues.ToArray(), null, null, null, indices);
					}
					else if(paramId != 0) 
					{
//						RegisterObject(paramId, objParam, null, paramValuesId, null, indices);
					}
					else 
					{
					}
					i++;
				}
				xmlReader.ReadEndElement();
			}
			else
			{
				xmlReader.Read();
			}
			
			message.ParamNames = (string[]) paramNames.ToArray(typeof(string));
			message.ParamValues = paramValues.ToArray();
			RegisterObject(paramValuesId, message.ParamValues, null, 0, null, null);
			return true;
		}

		void DeserializeHeaders (ArrayList headers)
		{
			xmlReader.ReadStartElement ();
			xmlReader.MoveToContent ();
			
			while (xmlReader.NodeType != XmlNodeType.EndElement) 
			{
				if (xmlReader.NodeType != XmlNodeType.Element) { xmlReader.Skip(); continue; }
				
				if (xmlReader.GetAttribute ("root", SoapTypeMapper.SoapEncodingNamespace) == "1")
					headers.Add (DeserializeHeader ());
				else
					Deserialize ();

				xmlReader.MoveToContent ();
			}
			
			xmlReader.ReadEndElement ();
		}
		
		Header DeserializeHeader ()
		{
			Header h = new Header (xmlReader.LocalName, null);
			h.HeaderNamespace = xmlReader.NamespaceURI;
			h.MustUnderstand = xmlReader.GetAttribute ("mustUnderstand", SoapTypeMapper.SoapEnvelopeNamespace) == "1";
			
			object value;
			long fieldId, fieldHref;
			long idHeader = NextAvailableId;
			FieldInfo fieldInfo = typeof(Header).GetField ("Value");

			value = DeserializeComponent (null, out fieldId, out fieldHref, idHeader, fieldInfo, null);
			h.Value = value;

			if(fieldHref != 0 && value == null)
			{
				RecordFixup (idHeader, fieldHref, h, null, null, fieldInfo, null);
			}
			else if(value != null && value.GetType().IsValueType && fieldId != 0)
			{
				RecordFixup (idHeader, fieldId, h, null, null, fieldInfo, null);
			}
			else if(fieldId != 0)
			{
				RegisterObject (fieldId, value, null, idHeader, fieldInfo, null);
			}
			
			RegisterObject (idHeader, h, null, 0, null, null);
			return h;
		}

		
		private object DeserializeArray(long id)
		{
			// Special case for base64 byte arrays
			if (GetComponentType () == typeof(byte[])) {
				byte[] data = Convert.FromBase64String (xmlReader.ReadElementString());
				RegisterObject(id, data, null, 0, null, null);
				return data;
			}
			
			// Get the array properties
			string strArrayType = xmlReader["arrayType", SoapTypeMapper.SoapEncodingNamespace];
			string[] arrayInfo = strArrayType.Split(':');
			int arraySuffixInfo = arrayInfo[1].LastIndexOf('[');
			String arrayElementType = arrayInfo[1].Substring(0, arraySuffixInfo);
			String arraySuffix = arrayInfo[1].Substring(arraySuffixInfo);
			string[] arrayDims = arraySuffix.Substring(1,arraySuffix.Length-2).Trim().Split(',');
			int numberOfDims = arrayDims.Length;
			int[] lengths = new int[numberOfDims];
			
			for (int i=0; i < numberOfDims; i++)
			{
				lengths[i] = Convert.ToInt32(arrayDims[i]);
			}

			int[] indices = new int[numberOfDims];

			// Create the array
			Type arrayType = mapper.GetType (arrayElementType, xmlReader.LookupNamespace(arrayInfo[0]));
			Array array = Array.CreateInstance(
				arrayType,
				lengths);

			for(int i = 0; i < numberOfDims; i++) 
			{
				indices[i] = array.GetLowerBound(i);
			}

			// Deserialize the array items
			int arrayDepth = xmlReader.Depth;
			xmlReader.Read();
			while(xmlReader.Depth > arrayDepth)
			{
				Type itemType = GetComponentType();
				if(itemType == null) 
					itemType = array.GetType().GetElementType();
				long itemId, itemHref;

				object objItem = DeserializeComponent(itemType,
					out itemId,
					out itemHref,
					id,
					null,
					indices);
				if(itemHref != 0)
				{
					object obj = objMgr.GetObject(itemHref);
					if(obj != null)
						array.SetValue(obj, indices);
					else
						RecordFixup(id, itemHref, array, null, null, null, indices);
				}
				else if(objItem != null && objItem.GetType().IsValueType && itemId != 0)
				{
					RecordFixup(id, itemId, array, null, null, null, indices);
				}
				else if(itemId != 0)
				{
					RegisterObject(itemId, objItem, null, id, null, indices);
					array.SetValue(objItem, indices);
				}
				else 
				{
					array.SetValue(objItem, indices);
				}

				// Get the next indice
				for(int dim = array.Rank - 1; dim >= 0; dim--)
				{
					indices[dim]++;
					if(indices[dim] > array.GetUpperBound(dim))
					{
						if(dim > 0)
						{
							indices[dim] = array.GetLowerBound(dim);
							continue;
						}
						
					}
					break;
				}
			}

			RegisterObject(id, array, null, 0, null, null);
			xmlReader.ReadEndElement();
			return array;

		}


		private object Deserialize()
		{
			object objReturn = null;
			Type type = mapper.GetType (xmlReader.LocalName, xmlReader.NamespaceURI);

			// Get the Id
			long id = GetId();
			id = (id == 0)?1:id;

			if(type == typeof(Array))
			{
				objReturn = DeserializeArray(id);
			}
			else
			{
				objReturn = DeserializeObject(type, id, 0, null, null);

			}

			return objReturn;
		}


		private object DeserializeObject(
			Type type, 
			long id, 
			long parentId, 
			MemberInfo parentMemberInfo,
			int[] indices)
		{
			SerializationInfo info = null;
			bool NeedsSerializationInfo = false;
			bool hasFixup;

			// in case of String & TimeSpan we should allways use 'ReadInternalSoapValue' method
			// in case of other internal types, we should use ReadInternalSoapValue' only if it is NOT
			// the root object, means it is a data member of another object that is being serialized.
			bool shouldReadInternal = (type == typeof(String) || type == typeof(TimeSpan) );
			if(shouldReadInternal || mapper.IsInternalSoapType (type) && (indices != null || parentMemberInfo != null) ) 
			{
				object obj = mapper.ReadInternalSoapValue (this, type);
				
				if(id != 0) 
					RegisterObject(id, obj, info, parentId, parentMemberInfo, indices);

				return obj;
			}
			object objReturn = 
				FormatterServices.GetUninitializedObject(type);

#if NET_2_0 && !TARGET_JVM
			objMgr.RaiseOnDeserializingEvent (objReturn);
#endif
			if(objReturn is ISerializable)
				NeedsSerializationInfo = true;

			if(_surrogateSelector != null && NeedsSerializationInfo == false)
			{
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _surrogateSelector.GetSurrogate(
					type,
					_context,
					out selector);
				NeedsSerializationInfo |= (surrogate != null);
			}

			if(NeedsSerializationInfo)
			{
				objReturn = 
					DeserializeISerializableObject(objReturn, id, out info, out hasFixup);
			}
			else
			{
				objReturn = 
					DeserializeSimpleObject(objReturn, id, out hasFixup);
				if(!hasFixup && objReturn is IObjectReference)
					objReturn = ((IObjectReference)objReturn).GetRealObject(_context);
			}

			RegisterObject(id, objReturn, info, parentId, parentMemberInfo, indices);
			xmlReader.ReadEndElement();
			return objReturn;
		}


		private object DeserializeSimpleObject(
			object obj,
			long id,
			out bool hasFixup
			)
		{
			hasFixup = false;
			Type currentType = obj.GetType();
			TypeMetadata tm = GetTypeMetadata (currentType);

			object[] data = new object[tm.MemberInfos.Length];
			xmlReader.Read();
			xmlReader.MoveToContent ();
			while (xmlReader.NodeType != XmlNodeType.EndElement)
			{
				if (xmlReader.NodeType != XmlNodeType.Element) {
					xmlReader.Skip ();
					continue;
				}
				
				object fieldObject;
				long fieldId, fieldHref;

				object indexob = tm.Indices [xmlReader.LocalName];
				if (indexob == null)
					throw new SerializationException ("Field \"" + xmlReader.LocalName + "\" not found in class " + currentType.FullName);
				
				int index = (int) indexob;
				FieldInfo fieldInfo = (tm.MemberInfos[index]) as FieldInfo;

				fieldObject = 
					DeserializeComponent(fieldInfo.FieldType,
					out fieldId,
					out fieldHref,
					id,
					fieldInfo,
					null);

				data[index] = fieldObject;

				if(fieldHref != 0 && fieldObject == null)
				{
					RecordFixup(id, fieldHref, obj, null, null, fieldInfo, null);
					hasFixup = true;
					continue;
				}
				if(fieldObject != null && fieldObject.GetType().IsValueType && fieldId != 0)
				{
					RecordFixup(id, fieldId, obj, null, null, fieldInfo, null);
					hasFixup = true;
					continue;
				}

				if(fieldId != 0)
				{
					RegisterObject(fieldId, fieldObject, null, id, fieldInfo, null);
				}
			}

			FormatterServices.PopulateObjectMembers (obj, tm.MemberInfos, data);
			return obj;
		}


		private object DeserializeISerializableObject(
			object obj, 
			long id, 
			out SerializationInfo info,
			out bool hasFixup
			)
		{
			long fieldId, fieldHref;
			info = new SerializationInfo(obj.GetType(), new FormatterConverter());
			hasFixup = false;
			
			int initialDepth = xmlReader.Depth;
			xmlReader.Read();
			while(xmlReader.Depth > initialDepth) 
			{
				Type fieldType = GetComponentType();
				string fieldName = XmlConvert.DecodeName (xmlReader.LocalName);
				object objField = DeserializeComponent(
					fieldType,
					out fieldId,
					out fieldHref,
					id,
					null,
					null);
				if(fieldHref != 0 && objField == null) 
				{
					RecordFixup(id, fieldHref, obj, info, fieldName, null, null);
					hasFixup = true;
					continue;
				}
				else if(fieldId != 0 && objField.GetType().IsValueType)
				{
					RecordFixup(id, fieldId, obj, info, fieldName, null, null);
					hasFixup = true;
					continue;
				}
				
				if(fieldId != 0) 
				{
					RegisterObject(fieldId, objField, null, id, null, null);
				}

				info.AddValue(fieldName, objField, (fieldType != null)?fieldType:typeof(object));
			}

			return obj;
		}


		private object DeserializeComponent(
			Type componentType, 
			out long componentId,
			out long componentHref,
			long parentId,
			MemberInfo parentMemberInfo,
			int[] indices)
		{
			object objReturn;
			componentId = 0;
			componentHref = 0;

			if(IsNull())
			{
				xmlReader.Read();
				return null;
			}

			Type xsiType = GetComponentType();
			if(xsiType != null) componentType = xsiType;

			if(xmlReader.HasAttributes)
			{
				componentId = GetId();
				componentHref = GetHref();
			}

			if(componentId != 0)
			{
				// It's a string
				string str = xmlReader.ReadElementString();
				objMgr.RegisterObject (str, componentId);
				return str;
			}
			if(componentHref != 0)
			{
				// Move the cursor to the next node
				xmlReader.Read();
				return objMgr.GetObject(componentHref);
			}

			if(componentType == null)
				return xmlReader.ReadElementString();

			componentId = NextAvailableId;
			objReturn = DeserializeObject(
				componentType,
				componentId,
				parentId,
				parentMemberInfo,
				indices);
			return objReturn;
		}

		public void RecordFixup(
			long parentObjectId, 
			long childObjectId,
			object parentObject,
			SerializationInfo info,
			string fieldName,
			MemberInfo memberInfo,
			int[] indices)
		{
			if(info != null)
			{
				objMgr.RecordDelayedFixup(parentObjectId, fieldName, childObjectId);
			}
			else if (parentObject is Array) 
			{
				if (indices.Length == 1)
					objMgr.RecordArrayElementFixup (parentObjectId, indices[0], childObjectId);
				else
					objMgr.RecordArrayElementFixup (parentObjectId, (int[])indices.Clone(), childObjectId);
			}
			else 
			{
				objMgr.RecordFixup (parentObjectId, memberInfo, childObjectId);
			}
		}

		private void RegisterObject (
			long objectId, 
			object objectInstance, 
			SerializationInfo info, 
			long parentObjectId, 
			MemberInfo parentObjectMember, 
			int[] indices)
		{
			if (parentObjectId == 0) indices = null;

			if (!objectInstance.GetType().IsValueType || parentObjectId == 0)
				objMgr.RegisterObject (objectInstance, objectId, info, 0, null, null);
			else
			{
				if(objMgr.GetObject(objectId) != null)
					throw new SerializationException("Object already registered");
				if (indices != null) indices = (int[])indices.Clone();
				objMgr.RegisterObject (
					objectInstance, 
					objectId, 
					info, 
					parentObjectId, 
					parentObjectMember, 
					indices);
			}
		}

		TypeMetadata GetTypeMetadata (Type type)
		{
			TypeMetadata tm = _fieldIndices[type] as TypeMetadata;
			if (tm != null) return tm;
			
			tm = new TypeMetadata ();
			tm.MemberInfos = FormatterServices.GetSerializableMembers (type, _context);
			
			tm.Indices	= new Hashtable();
			for(int i = 0; i < tm.MemberInfos.Length; i++) {
				SoapFieldAttribute at = (SoapFieldAttribute) InternalRemotingServices.GetCachedSoapAttribute (tm.MemberInfos[i]);
				tm.Indices [XmlConvert.EncodeLocalName (at.XmlElementName)] = i;
			}
			
			_fieldIndices[type] = tm;
			return tm;
		}
		
		public Type GetTypeFromQName (string qname)
		{
			string[] strName = qname.Split(':');
			string namespaceURI = xmlReader.LookupNamespace (strName[0]);
			return mapper.GetType (strName[1], namespaceURI);
		}
		
		#endregion
	}
}
