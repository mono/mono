// created on 24/04/2003 at 15:35
//
//	System.Runtime.Serialization.Formatters.Soap.SoapReader
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

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
			xmlReader = new XmlTextReader(inStream);
			xmlReader.WhitespaceHandling = WhitespaceHandling.None;
			mapper = new SoapTypeMapper(_binder);

			try
			{
				// SOAP-ENV:Envelope
				xmlReader.MoveToContent();
				xmlReader.Read();
				// SOAP-ENV:Body
				xmlReader.Read();

				// The root object
				if(soapMessage != null)
				{
					if(DeserializeMessage(soapMessage)) 
					{
						RegisterObject(1, soapMessage, null, 0, null, null);
					}
				}
				
				if(xmlReader.NodeType != XmlNodeType.EndElement)
				{
					do
					{
						Deserialize();

					}
					while(xmlReader.NodeType != XmlNodeType.EndElement); 
				}
			}
			finally 
			{
				if(xmlReader != null) xmlReader.Close();
			}

			return TopObject;
		}
		
		#endregion

		#region Private Methods

		private object TopObject 
		{
			get 
			{
				objMgr.DoFixups();
				objMgr.RaiseDeserializationEvent();
				return objMgr.GetObject(1);
			}
		}

		private bool IsNull()
		{
			string tmp = xmlReader["xsi:null"];
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
			Type type = null;
			if(GetId() != 0) return typeof(string);
			string strValue = xmlReader["xsi:type"];
			if(strValue == null) return null;
			string[] strName = strValue.Split(':');
			string namespaceURI = xmlReader.LookupNamespace(strName[0]);
			type = mapper[new Element(string.Empty, strName[1], namespaceURI)];

			return type;
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

			int initialDepth = xmlReader.Depth;
			xmlReader.Read();
			int i = 0;
			while(xmlReader.Depth > initialDepth) 
			{
				long paramId, paramHref;
				object objParam = null;
				paramNames.Add(xmlReader.Name);
				indices[0] = i;
				objParam = DeserializeComponent(
					null,
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
					RegisterObject(paramId, objParam, null, paramValuesId, null, indices);
				}
				else 
				{
				}
				i++;
			}

			message.ParamNames = (string[]) paramNames.ToArray(typeof(string));
			message.ParamValues = paramValues.ToArray();
			xmlReader.ReadEndElement();
			RegisterObject(paramValuesId, message.ParamValues, null, 0, null, null);
			return true;
		}

		
		private object DeserializeArray(long id)
		{
			// Get the array properties
			string strArrayType = xmlReader["arrayType", SoapTypeMapper.SoapEncodingNamespace];
			string[] arrayInfo = strArrayType.Split(':','[',',',']');
			int numberOfDims = arrayInfo.Length - 3;
			int[] lengths = new int[numberOfDims];
			string[] arrayDims = new String[numberOfDims];
			Array.Copy(arrayInfo, 2, arrayDims, 0, numberOfDims);
			for (int i=0; i < numberOfDims; i++)
			{
				lengths[i] = Convert.ToInt32(arrayDims[i]);
			}

			int[] indices = new int[numberOfDims];

			// Create the array
			Type arrayType = mapper[new Element(arrayInfo[0], arrayInfo[1], xmlReader.LookupNamespace(arrayInfo[0]))];
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
			Element element = new Element(
				xmlReader.Prefix,
				xmlReader.LocalName,
				xmlReader.NamespaceURI);


			Type type = mapper[element];

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

			if(SoapTypeMapper.CanBeValue(type)) 
			{
				string elementString = xmlReader.ReadElementString();
				object obj;
				if(type.IsEnum)
					obj = Enum.Parse(type, elementString);
				else
					obj = Convert.ChangeType(elementString, type);
				if(id > 0) 
					RegisterObject(id, obj, info, parentId, parentMemberInfo, indices);

				return obj;
			}
			object objReturn = 
				FormatterServices.GetUninitializedObject(type);
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
			MemberInfo[] memberInfos = 
				FormatterServices.GetSerializableMembers(currentType, _context);
			Hashtable indices = (Hashtable) _fieldIndices[currentType];
			if(indices == null) 
			{
				indices	= new Hashtable();
				for(int i = 0; i < memberInfos.Length; i++) 
				{
					indices.Add(memberInfos[i].Name, i);
				}
				_fieldIndices[currentType] = indices;
			}

			int objDepth = xmlReader.Depth;
			object[] data = new object[memberInfos.Length];
			xmlReader.Read();
			for(int i = 0; i < memberInfos.Length; i++)
			{
				object fieldObject;
				long fieldId, fieldHref;
				int index = (int) indices[xmlReader.LocalName];
				FieldInfo fieldInfo = (memberInfos[index]) as FieldInfo;
				if(fieldInfo == null) continue;

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

			FormatterServices.PopulateObjectMembers(obj, memberInfos, data);
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
				string fieldName = xmlReader.LocalName;
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
				return xmlReader.ReadElementString();
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


		
		#endregion
	}
}
