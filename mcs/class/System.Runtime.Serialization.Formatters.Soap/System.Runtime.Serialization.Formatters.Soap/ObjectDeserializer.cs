using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace System.Runtime.Serialization.Formatters.Soap
{	
	internal class ObjectDeserializer
	{
		/**const section**/ 
		const string cStringType        = "System.String";
		const string basicassembly      = "mscorlib";
		const string xmlnsassem         = "http://schemas.microsoft.com/clr/nsassem/";
		const string xmlns              = "http://schemas.microsoft.com/clr/ns/";				
		const string cTarget            = "Target";
		const string cDelegatesClass    = "System.MulticastDelegate";
		const string cMethodName        = "MethodName";
		const string cSoapEncArray      = "SOAP-ENC:Array";
		const string cId                = "id";

		private ArrayList  FObjectList;
		private int        FObjectNumber;
		private SoapReader ObjectRdr;

		public XmlDocument FXmlDoc;		

		private int AddObject(object graph, out bool AlreadyExists, int ObjectIndex, out object ResultObject)
		{
			AlreadyExists= true;
			ResultObject= null;			
			if((FObjectList.Count< ObjectIndex)||(FObjectList[ObjectIndex - 1]==null))//the object not exits
			{
				if(FObjectList.Count< ObjectIndex)
				{
					int Capacity= FObjectList.Capacity;
					int Start= FObjectList.Count;
					for(int i= Start; i<= Capacity; i++)
					{FObjectList.Add(null);}
				}
				if(FObjectList[ObjectIndex - 1]==null)
				{
					FObjectList.Insert(ObjectIndex - 1, graph);
					AlreadyExists= false;				
					ResultObject= graph;
					FObjectNumber++;
				}
			}		
			else
				ResultObject= FObjectList[ObjectIndex - 1];
			return ObjectIndex;			
		}

		private string GetAssemblyIndex(string SoapNamespace, string ReferenceName)
		{						
			XmlAttributeCollection XmlAttrCollection= FXmlDoc.DocumentElement.Attributes;
			bool Continue= true;
			int i= 0;
			string ItemName= "";
			while((Continue)&&(i<= XmlAttrCollection.Count - 1))
			{
				string AttrValue= XmlAttrCollection.Item(i).Value;				
				if(AttrValue==SoapNamespace)
				{
					ItemName= XmlAttrCollection.Item(i).Name;
					ItemName= ItemName.Substring(ItemName.IndexOf(":") + 1, ItemName.Length - ItemName.IndexOf(":") - 1);
					Continue= false;
				}
				i++;
			}
			return ItemName + ":" + ReferenceName;
		}

		public void ClearLits()
		{
			FObjectList.Clear();
		}

		public ObjectDeserializer(Stream serializationStream)
		{
			FXmlDoc= new XmlDocument();
			FXmlDoc.Load(serializationStream);
			FObjectList= new ArrayList();
			ObjectRdr= new SoapReader();
			ObjectRdr.FXmlDoc= FXmlDoc;
		}
		/**simple types deserialization**/
		private void DeserialiazeValueType(FieldInfo objectfield, XmlElement ParentElement/*string XmlParentElement*/, object ActualObject/*, string XmlParentElementId*/)
		{	
			if((objectfield.FieldType.Assembly.GetName().Name == basicassembly)||(objectfield.FieldType.IsEnum))
			{
				string fieldvalue= ObjectRdr.ReadValueTypeFromXml(objectfield.Name, ParentElement);
				ValueType objvalue= ObjectRdr.GetValueTypeFromString(objectfield.FieldType.UnderlyingSystemType.Name, fieldvalue);
				objectfield.SetValue(ActualObject, objvalue);            
			}
			else //is an struct
				DeserializeStruct(objectfield, ParentElement, ActualObject, false);	  
		}
		/**Structs deserialization**/     
		private void DeserializeStructValueType(FieldInfo structfield, XmlElement ParentElement/*string XmlParentElement*/, object ActualObject/*, string XmlParentElementId*/, string StructName, object StructObject, bool NestedStruct)
		{
			if(structfield.FieldType.Assembly.GetName().Name == basicassembly)
			{
				string fieldvalue= ObjectRdr.ReadStructValueFieldFromXml(structfield.Name, ParentElement, StructName, false);
				ValueType objValue= ObjectRdr.GetValueTypeFromString(structfield.FieldType.UnderlyingSystemType.Name, fieldvalue);
				structfield.SetValue(StructObject, objValue);			
			}
			else //is a nested struct
			{
				XmlElement StructElement= (XmlElement)ParentElement.GetElementsByTagName(structfield.Name).Item(0);
				DeserializeStruct(structfield, StructElement, StructObject, true);
			}
		}

		private void DeserializeStructReferenceType(FieldInfo structfield, XmlElement ParentElement, object ActualObject, bool NestedStruct, int FieldIndex)
		{			
			DeserializeReferenceType(structfield, ParentElement, ActualObject, FieldIndex);
		}

		private void DeserializeStruct(FieldInfo objectfield, XmlElement ParentElement, /*, string XmlParentElement*/ object ActualObject/*, string XmlParentElementId*/, bool NestedStruct)
		{
			object StructValue= Assembly.Load(objectfield.FieldType.Assembly.GetName().Name).CreateInstance(objectfield.FieldType.FullName);
			if(StructValue != null)
			{
				FieldInfo[] structfields= objectfield.FieldType.GetFields();
				for(int index= 0; index <= structfields.Length - 1; index++)
				{
					if(!structfields[index].IsNotSerialized)
					{
						if(structfields[index].FieldType.IsValueType)				  
							DeserializeStructValueType(structfields[index], ParentElement, ActualObject, objectfield.Name, StructValue, NestedStruct);				  				  
						else //is a reference type				  
							DeserializeStructReferenceType(structfields[index], ParentElement, StructValue, NestedStruct, index);
				  
					}
				}	
				objectfield.SetValue(ActualObject, StructValue);
			}
			else
				objectfield.SetValue(ActualObject, null);
		}
       
		private void DeserializeStruct(ref Array ArrayValue, int ItemIndex, XmlElement ParentElement, object ActualObject, bool NestedStruct)
		{
			string XsdType= ((XmlElement)ParentElement.ChildNodes.Item(ItemIndex)).GetAttribute("xsi:type");			
			string NsName;
			string AssemblyName= ObjectRdr.GetFullObjectLocation(XsdType, out NsName);
			object StructValue= Assembly.Load(AssemblyName).CreateInstance(NsName);
			if(StructValue != null)
			{
				FieldInfo[] structfields= StructValue.GetType().GetFields();
				for(int index= 0; index <= structfields.Length - 1; index++)
				{
					if(!structfields[index].IsNotSerialized)
					{
						if(structfields[index].FieldType.IsValueType)				  
							DeserializeStructValueType(structfields[index], ParentElement, ActualObject, "", StructValue, NestedStruct);				  				  
						else //is a reference type				  
							DeserializeStructReferenceType(structfields[index], ParentElement, StructValue, NestedStruct, index);
				  
					}
				}	
				ArrayValue.SetValue(StructValue, ItemIndex);
			}
			else
				ArrayValue.SetValue(null, ItemIndex);
		}

		/**Reference types deserialization**/
		private void DeserializeReferenceType(FieldInfo objectfield, XmlElement ParentElement, object ActualObject, int FieldIndex)
		{
			XmlElement RefElement= null;
			ReferenceTypes RefType= ObjectRdr.GetReferenceType(objectfield.Name, ParentElement, ref RefElement);
			switch(RefType)
			{
				case ReferenceTypes.String_Type         :   DeserializeString(objectfield, ParentElement, ActualObject);
					break;
				case ReferenceTypes.Object_Type         :   DeserializeInterfacedObjectField(objectfield, ParentElement, ActualObject);
					break;
				case ReferenceTypes.Delegate_Type       :   DeserializeDelegates(objectfield, ParentElement, ActualObject);
					break;
				case ReferenceTypes.Array_Type          :   DeserializeArray(objectfield, ParentElement, ActualObject);
					break;
			}  			
		}
		
		private void DeseralizeArrayItemReferenceType(ref Array ArrayValue, int index, XmlElement ArrayElement, object ActualObject)
		{
			XmlElement RefElement= null;
			ReferenceTypes RefType= ObjectRdr.GetReferenceType(index, ArrayElement, ref RefElement);
			switch(RefType)
			{
				case ReferenceTypes.String_Type         :   DeserializeString(ref ArrayValue, index, ArrayElement, ActualObject);
					break;
				case ReferenceTypes.Object_Type         :   DeserializeInterfacedObjectField(ref ArrayValue, index, ArrayElement, ActualObject);
					break;
				case ReferenceTypes.Delegate_Type       :   DeserializeDelegates(ref ArrayValue, index, ArrayElement, ActualObject);
					break;
				case ReferenceTypes.Array_Type          :   DeserializeArray(ref ArrayValue, index, ArrayElement, ActualObject);
					break;
			} 
		}

		/**Strings deseralization**/
		private void DeserializeString(ref Array ArrayValue, int ItemIndex, XmlElement ParentElement, object ActualObject)
		{
			int ReferenceIndex= ObjectRdr.ReadStringIdFromXml(ItemIndex, ParentElement);//the reference index
			string StringObj;
			if(ReferenceIndex == -1)
				StringObj= null;
			else
				StringObj= ObjectRdr.ReadStringTypeFromXml(ItemIndex, ParentElement);
			object ResultObject;
			bool AlreadyExist;
			AddObject(StringObj, out AlreadyExist, ReferenceIndex, out ResultObject);			
			((Array)ArrayValue).SetValue((string)ResultObject, ItemIndex);
		}
		
		private void DeserializeString(FieldInfo objectfield, XmlElement ParentElement, object ActualObject)
		{
			int ReferenceIndex= ObjectRdr.ReadStringIdFromXml(objectfield.Name, ParentElement);//the reference index
			string StringObj;
			if(ReferenceIndex == -1)
				StringObj= null;
			else
				StringObj= ObjectRdr.ReadStringTypeFromXml(objectfield.Name, ParentElement);
			object ResultObject;
			bool AlreadyExist;
			AddObject(StringObj, out AlreadyExist, ReferenceIndex, out ResultObject);
			objectfield.SetValue(ActualObject, (string)ResultObject);
		}
		/**interfaces deserialization**/
		//object's interfaces fields serialization
		private void DeserializeInterfacedObjectField(FieldInfo objectfield, XmlElement ParentElement, object ActualObject)
		{          	
			int ReferenceIndex= ObjectRdr.ReadReferenceIndexFromXml(objectfield.Name, ParentElement);//the reference index           
			string ReferenceFullName= ObjectRdr.ReadReferenceFullNameFromXml(ReferenceIndex.ToString());//objectfield.FieldType.FullName;
			if(ReferenceIndex != -1) //not null
			{		 	
				object ItemValue= CommonIntObjectDeserialization(ReferenceIndex, ReferenceFullName, ParentElement);
				objectfield.SetValue(ActualObject, ItemValue);
			}
			else
				objectfield.SetValue(ActualObject, null);
		}
	
		//Array's items interfaces serialization
		private void DeserializeInterfacedObjectField(ref Array ArrayValue, int ItemIndex, XmlElement ParentElement, object ActualObject)
		{
			int ReferenceIndex= ObjectRdr.ReadReferenceIndexFromXml(ItemIndex, ParentElement);//the reference index           
			string ReferenceFullName= ObjectRdr.ReadReferenceFullNameFromXml(ReferenceIndex.ToString());//objectfield.FieldType.FullName;
			if(ReferenceIndex != -1) //not null
			{			 
				object ItemValue= CommonIntObjectDeserialization(ReferenceIndex, ReferenceFullName, ParentElement);
				((Array)ArrayValue).SetValue(ItemValue, ItemIndex);
			}
			else
				((Array)ArrayValue).SetValue(null, ItemIndex);

		}

		private object CommonIntObjectDeserialization(int ReferenceIndex, string ReferenceFullName, XmlElement ParentElement)
		{
			bool AlreadyExists;	
			string AssemblyName= ObjectRdr.GetAssemblyNameFromId(ReferenceIndex);//ReadAssemblyNameFromXml(/*objectfield.Name, */ParentElement);
			object ItemValue= Assembly.Load(AssemblyName).CreateInstance(ReferenceFullName);	  		  			
			DeserializeObject(ref ItemValue, ReferenceIndex);
			return ItemValue;
		}
		/**Delegates Deserialization**/
		//object's delegates fields serialization
		private void DeserializeDelegates(FieldInfo Delegatefield, XmlElement ParentElement, object ActualObject)
		{
			int ReferenceIndex= ObjectRdr.ReadReferenceIndexFromXml(Delegatefield.Name, ParentElement);//the reference index           
			string DelegateElementName= ObjectRdr.GetDelegateElementName(ReferenceIndex);
			XmlElement CurrentElement= ObjectRdr.GetCurrentElement(DelegateElementName, ReferenceIndex.ToString());
			if(ReferenceIndex != -1) //not null
			{					  
				object DelegateValue= CommonDelegateDeserialization(CurrentElement, Delegatefield.FieldType);
				Delegatefield.SetValue(ActualObject, DelegateValue);
			}
		}

		//Array's delegates items serialization
		private void DeserializeDelegates(ref Array ArrayValue, int ItemIndex, XmlElement ParentElement, object ActualObject)
		{
			int ReferenceIndex= ObjectRdr.ReadReferenceIndexFromXml(ItemIndex, ParentElement);//the reference index           
			string DelegateElementName= ObjectRdr.GetDelegateElementName(ReferenceIndex);
			XmlElement CurrentElement= ObjectRdr.GetCurrentElement(DelegateElementName, ReferenceIndex.ToString());
			if(ReferenceIndex != -1) //not null
			{	          
				Type DlgType= ObjectRdr.GetDelegateTypeFromXml(CurrentElement);			  			 
				object DelegateValue= CommonDelegateDeserialization(CurrentElement, DlgType);
				((Array)ArrayValue).SetValue(DelegateValue, ItemIndex);
			}
		}

		private object CommonDelegateDeserialization(XmlElement CurrentElement, Type DlgType)
		{
			int TargetIndex= ObjectRdr.ReadReferenceIndexFromXml(cTarget, CurrentElement);//the reference index 
			string AssemblyName= ObjectRdr.GetAssemblyNameFromId(TargetIndex);
			string TargetFullName= ObjectRdr.ReadReferenceFullNameFromXml(TargetIndex.ToString());
			object Target= Assembly.Load(AssemblyName).CreateInstance(TargetFullName);			
			DeserializeObject(ref Target, TargetIndex);
			string MethodName= ObjectRdr.ReadStringTypeFromXml(cMethodName ,CurrentElement);
			Delegate DelegateValue= MulticastDelegate.CreateDelegate(DlgType, Target, MethodName);
			return DelegateValue;
		}

		/**Arrays Deserialization**/
		//Object's fields desearialization
		private void DeserializeArray(FieldInfo ArrayField, XmlElement ParentElement, object ActualObject)
		{
			int ArrayIndex= ObjectRdr.ReadReferenceIndexFromXml(ArrayField.Name, ParentElement);
			XmlElement ArrayElement;
			string ArrayTypeName;
			Array ArrayValue=  CommonArrayDeserialization(ArrayIndex, out ArrayTypeName, out ArrayElement);
			object ResultObject;		  
			bool AlreadyExists;
			AddObject(ArrayValue, out AlreadyExists, ArrayIndex, out ResultObject);//add the array
			ArrayField.SetValue(ActualObject, ResultObject);
			if(!AlreadyExists)			
				DeserializeArrayItems(ArrayValue, ArrayElement, ActualObject, ArrayTypeName);				
		}
		//Array's items deserialization
		private void DeserializeArray(ref Array ArrayValue, int ItemIndex, XmlElement ParentElement, object ActualObject)
		{
			int ArrayIndex= ObjectRdr.ReadReferenceIndexFromXml(ItemIndex, ParentElement);
			XmlElement ArrayElement;
			string ArrayTypeName;
			Array ArrayActualValue= CommonArrayDeserialization(ArrayIndex, out ArrayTypeName, out ArrayElement);
			((Array)ArrayValue).SetValue(ArrayActualValue, ItemIndex);		  
			bool AlreadyExists;
			object ResultObject;
			AddObject(ArrayActualValue, out AlreadyExists, ArrayIndex, out ResultObject);
			if(!AlreadyExists)		  
				DeserializeArrayItems(ArrayActualValue, ArrayElement, ActualObject, ArrayTypeName);			  		  
		}


		private Array CommonArrayDeserialization(int ArrayIndex, out string ArrTypeName, out XmlElement ArrayElement)
		{
			ArrayElement= ObjectRdr.GetCurrentElement(cSoapEncArray, ArrayIndex.ToString());
			string AssemblyName;		
			string ArrayTypeName= ObjectRdr.ReadArrayTypeFromXml(ArrayElement, out AssemblyName);
			int StartIndex= ArrayTypeName.LastIndexOf("[");
			string ArrayLength= ArrayTypeName.Substring(StartIndex + 1, ArrayTypeName.Length - 2 - StartIndex);
			ArrayTypeName= ArrayTypeName.Substring(0, ArrayTypeName.LastIndexOf("["));
			Assembly.Load(AssemblyName);
			Type ArrayType= Type.GetType(ArrayTypeName);
			ArrTypeName= ArrayType.Name;
			Array ArrayActualValue= Array.CreateInstance(ArrayType, Convert.ToInt32(ArrayLength));
			return ArrayActualValue;
		}

		private void DeserializeArrayItems(Array ArrayValue, XmlElement ArrayElement, object ActualObject, string ArrayTypeName)
		{
			bool IsStruct= false;
			bool IsNull= false;
			for(int index= 0; index<= ((Array)ArrayValue).Length - 1; index++)
			{		    
				bool IsValueType= ObjectRdr.IsArrayItemValueType(ArrayElement, index, ref IsNull, ref IsStruct);
				if(IsNull)
					((Array)ArrayValue).SetValue(null, index);
				else
					if(IsValueType)
					DeserializeArrayItemValueType(ref ArrayValue, index, ArrayElement, ArrayTypeName, IsStruct, ActualObject);
				else //is a reference type
					DeseralizeArrayItemReferenceType(ref ArrayValue, index, ArrayElement, ActualObject);    
			}
		}

		private void DeserializeArrayItemValueType(ref Array ArrayValue, int index, XmlElement ArrayElement, string ItemTypeName, bool IsStruct, object ActualObject)
		{
			if(!IsStruct)
			{
				ValueType ItemValue= ObjectRdr.ReadArrayItemSimpleTypeFromXml(ArrayElement, index, ItemTypeName);
				((Array)ArrayValue).SetValue(ItemValue, index);
			}
			else
				DeserializeStruct(ref ArrayValue, index, ArrayElement, ActualObject, false);
		}

		/**objects desrialization**/
		private int DeserializeObject(ref object graph, int ObjectIndex)
		{
			bool AlreadyExits;			
			string XmlElemtName= GetAssemblyIndex(GetXmlNamespace(graph.GetType().Namespace, graph.GetType().Assembly.GetName().Name), graph.GetType().Name);					
			/**this is temporal**/
			XmlElement ObjectElement= ObjectRdr.GetCurrentElement(XmlElemtName, ObjectIndex.ToString());
			object ResultObject;
			ObjectIndex= AddObject(graph, out AlreadyExits, ObjectIndex, out ResultObject);
			if(!AlreadyExits)//new object
			{
				FieldInfo[]objectfields= ResultObject.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
				for(int index= 0; index<= objectfields.Length - 1; index++)
				{
					if(!objectfields[index].IsNotSerialized)
					{
						if(objectfields[index].FieldType.IsValueType)// the field is a value type
							DeserialiazeValueType(objectfields[index], ObjectElement, /*XmlElemtName,*/ ResultObject/*, ObjectIndex.ToString()*/);
						else
							DeserializeReferenceType(objectfields[index], ObjectElement, ResultObject, index);
					}
				}
			}
			graph= ResultObject;
			return ObjectIndex;
		}

		private string GetMainAssemblyFullNameFromXml(out string AssemblyName)
		{
			XmlNode SoapEnvNode= FXmlDoc.DocumentElement.GetElementsByTagName("SOAP-ENV:Body").Item(0);
			XmlNode MainObjectNode= SoapEnvNode.ChildNodes.Item(0);	
			int StartIndex= MainObjectNode.Name.IndexOf(":");
			string ClassName= MainObjectNode.Name.Substring(StartIndex + 1, MainObjectNode.Name.Length - StartIndex - 1);		
			string AttributeName= FXmlDoc.DocumentElement.Attributes.GetNamedItem("xmlns:" + MainObjectNode.Name.Substring(0, 2)).Value;
			StartIndex= AttributeName.LastIndexOf("/");
			AssemblyName= AttributeName.Substring(StartIndex + 1, AttributeName.Length - StartIndex - 1);
			string TempStr= AttributeName.Substring(0, StartIndex);
			StartIndex= TempStr.LastIndexOf("/");
			string ReferenceFullName= TempStr.Substring(StartIndex + 1, TempStr.Length - StartIndex - 1);
			return ReferenceFullName + "." + ClassName;
		}

		private object GetMainObjectFromXml()
		{			
			string AssName;
			string ReferenceFullName= GetMainAssemblyFullNameFromXml(out AssName);
			return Assembly.Load(AssName).CreateInstance(ReferenceFullName);
		}

		public string GetXmlNamespace(string NamepaceName, string AssemblyName)
		{
			string XmlAssNs;
			if(AssemblyName == basicassembly)			 
				XmlAssNs= xmlns + NamepaceName;			
			else
				XmlAssNs= xmlnsassem + NamepaceName + '/' + AssemblyName;			
			return XmlAssNs;
		}
				      
		public object Deserialize(Stream serializationStream)
		{			
			object MainObject= GetMainObjectFromXml();
			DeserializeObject(ref MainObject, 1); 
			return MainObject;
		}
	}
}
