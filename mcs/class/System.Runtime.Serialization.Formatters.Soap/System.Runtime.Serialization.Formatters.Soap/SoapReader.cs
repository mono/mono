using System;
using System.Xml;
using System.Reflection;

namespace System.Runtime.Serialization.Formatters.Soap
{	

	public enum ReferenceTypes {Array_Type, Object_Type, Interface_Type, Delegate_Type, String_Type};

	internal class SoapReader
	{
		/******const section******/
		const string xmlns_SOAP_ENC	= "http://schemas.xmlsoap.org/soap/encoding/";
		const string xmlns_SOAP_ENV	= "http://schemas.xmlsoap.org/soap/envelope/";

		const string cSoapRef           = "href";
		const string cObjectRef         = "ref-";
		const string cId                = "id";
		const string cXsiNull           = "xsi:null";
		const string basicassembly      = "mscorlib";
		const string startxmlns         = "xmlns:a";		
		const string Systemns           = "http://schemas.microsoft.com/clr/ns/System";		
		const string cDefaultValue      = "_0x00_";
		/******Array's serialization section******/
		const string cItem           = "item";
		const string cSoapEncArray   = "SOAP-ENC:Array";
		const string cSoapArrayType  = "SOAP-ENC:arrayType"; 
		const string cXsiType        = "xsi:type";
		const string cNullObject     = "xsi:null=\"1\"/";
		/******Delegate's serialization section******/
		const string cDelegateSerClass  = "DelegateSerializationHolder";
		const string cDelegateType      = "DelegateType";
		const string cDelegateAssembly  = "DelegateAssembly";
		/******fields's section******/		
		public XmlDocument FXmlDoc;		
		public XmlElement  DeepElement; //the current Xml Struct Element
		//public Utils       FUtils;

		public SoapReader()
		{		
			//FUtils= new Utils();
		}

		/**Reference Types reader**/
		public int ReadObjectIndexFromXml(string ObjectElemt)
		{
			XmlNodeList ObjectElement= FXmlDoc.DocumentElement.GetElementsByTagName(ObjectElemt);
			XmlElement FCurrentElement= (XmlElement)ObjectElement.Item(0);
			string refid= (FCurrentElement).GetAttribute(cId);
			int startindex= refid.IndexOf("-");
			refid= refid.Substring(startindex + 1, refid.Length - startindex - 1);		   
			return Convert.ToInt32(refid);
		}		

		private string ReadReferenceFullNameFromXmlNode(XmlNode ReferenceNode)
		{
			int StartIndex= ReferenceNode.Name.IndexOf(":");
			string ClassName= ReferenceNode.Name.Substring(StartIndex + 1, ReferenceNode.Name.Length - StartIndex - 1);		
			string AttributeName= FXmlDoc.DocumentElement.Attributes.GetNamedItem("xmlns:" + ReferenceNode.Name.Substring(0, StartIndex)).Value;
			StartIndex= AttributeName.LastIndexOf("/");			
			string TempStr= AttributeName.Substring(0, StartIndex);
			StartIndex= TempStr.LastIndexOf("/");
			string ReferenceFullName= TempStr.Substring(StartIndex + 1, TempStr.Length - StartIndex - 1);
			return ReferenceFullName + "." + ClassName;			
		}

		public string ReadReferenceFullNameFromXml(string RefereneId)
		{
			string RefId= cObjectRef + RefereneId;
			XmlNodeList NodeList = FXmlDoc.DocumentElement.GetElementsByTagName("Body", xmlns_SOAP_ENV).Item(0).ChildNodes;
			bool Continue= true;
			int index= 0;
			string Result= "";
			while((Continue)&&(index <= NodeList.Count - 1))
			{
				XmlElement ActElement= (XmlElement)NodeList.Item(index);
				if(ActElement.GetAttribute("id")== RefId)//the attributes match
				{
					Result= ReadReferenceFullNameFromXmlNode(ActElement);
					Continue= false;
				}
				else
					index++;
			}
			return Result;
		}

		/**ReadReferenceIndexFromXml**/
		public int ReadReferenceIndexFromXml(string FieldName, XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.GetElementsByTagName(FieldName).Item(0);		  
			if(FieldElement.GetAttribute(cXsiNull)== "") //if it is not a null field
			{
				string hrefvalue= FieldElement.GetAttribute(cSoapRef);
				int StartIndex= hrefvalue.IndexOf("-");
				return Convert.ToInt32(hrefvalue.Substring(StartIndex + 1, hrefvalue.Length - 1 - StartIndex));
			}
			else
				return -1;
		}

		public int  ReadReferenceIndexFromXml(int ItemIndex, XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.ChildNodes.Item(ItemIndex);
			if(FieldElement.GetAttribute(cXsiNull)== "") //if it is not a null field
			{
				string hrefvalue= FieldElement.GetAttribute(cSoapRef);
				int StartIndex= hrefvalue.IndexOf("-");
				return Convert.ToInt32(hrefvalue.Substring(StartIndex + 1, hrefvalue.Length - 1 - StartIndex));
			}
			else
				return -1;
		}



		/**String reader**/
		public int ReadStringIdFromXml(/*string XmlParentElement, */string FieldName,/* string XmlParentElementId, */XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.GetElementsByTagName(FieldName).Item(0);//(XmlElement)GetCurrentElement(XmlParentElement, XmlParentElementId).GetElementsByTagName(FieldName).Item(0);
			if(FieldElement.GetAttribute(cId)== "")
				return ReadReferenceIndexFromXml(FieldName, ParentElement);
			else
				return ReadFieldIdValueFromXml(FieldName, ParentElement);
		}

		public int ReadStringIdFromXml(int ItemIndex, XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.ChildNodes.Item(ItemIndex);
			string StrId= FieldElement.GetAttribute(cId);
			if(StrId == "")
				return ReadReferenceIndexFromXml(ItemIndex, ParentElement);
			else
				return ReadFieldIdValueFromXml(ItemIndex, ParentElement);
		}
		
		public string ReadStringTypeFromXml(string FieldName, XmlElement ParentElement)
		{
			XmlNode XmlField= ParentElement.GetElementsByTagName(FieldName).Item(0);
			if(XmlField!= null)
				return XmlField.InnerXml;
			else
				return null;
		}

		public string ReadStringTypeFromXml(int ItemIndex, XmlElement ParentElement)
		{
			XmlNode XmlField= ParentElement.ChildNodes.Item(ItemIndex);
			if(XmlField!= null)
				return XmlField.InnerXml;
			else
				return null;
		}

		/**Delegates reader**/
		public string GetDelegateElementName(int DelegateId)
		{					
			XmlAttributeCollection XmlAttrCollection= FXmlDoc.DocumentElement.Attributes;
			bool Continue= true;
			int i= 0;
			string ItemName= "";
			while((Continue)&&(i<= XmlAttrCollection.Count - 1))
			{
				string AttrValue= XmlAttrCollection.Item(i).Value;				
				if(AttrValue == Systemns)
				{
					ItemName= XmlAttrCollection.Item(i).Name;
					ItemName= ItemName.Substring(ItemName.LastIndexOf(":") + 1, ItemName.Length - 1 - ItemName.LastIndexOf(":"));
					Continue= false;
				}
				i++;
			}
			return ItemName + ":" + cDelegateSerClass;
		}

		public Type GetDelegateTypeFromXml(XmlElement ParentElement)
		{
			string DelegateAssembly= ParentElement.GetElementsByTagName(cDelegateAssembly).Item(0).InnerXml;
			DelegateAssembly= DelegateAssembly.Substring(0, DelegateAssembly.IndexOf(","));
			string DelegateType= ParentElement.GetElementsByTagName(cDelegateType).Item(0).InnerXml;
			return Assembly.Load(DelegateAssembly).GetType(DelegateType);		   
		}
        
		/**Arrays reader**/
		public string ReadArrayTypeFromXml(XmlElement ArrayElement, out string AssemblyName)
		{          
			string ArrayTypeAttr= ArrayElement.GetAttribute(cSoapArrayType);
			int StartIndex= ArrayTypeAttr.LastIndexOf(":");
			string Result;
			if(ArrayTypeAttr.Substring(0, 4) == "xsd:")	
			{
				Result= "System";
				string CLRType= GetCLRTypeFromXsdType(ArrayTypeAttr.Substring(0, ArrayTypeAttr.IndexOf("[")));
				StartIndex= ArrayTypeAttr.IndexOf("[");
				Result= "System." + CLRType + ArrayTypeAttr.Substring(StartIndex, ArrayTypeAttr.Length - StartIndex);
				AssemblyName= basicassembly;
			}
			else
			{
				AssemblyName= ReadAssemblyNameFromXml(ArrayTypeAttr);				
				string NsIndex= ArrayTypeAttr.Substring(1, StartIndex - 1);
				Result= ReadNamespaceFromXml(NsIndex);				
				Result= Result +   "." + ArrayTypeAttr.Substring(StartIndex + 1, ArrayTypeAttr.Length - StartIndex - 1);
			}		  
			return Result;
		} 
        		

		public bool IsArrayItemValueType(XmlElement ParentElement, int ItemIndex, ref bool IsNull, ref bool IsStruct)
		{
			XmlElement ArrayItem= (XmlElement)ParentElement.ChildNodes.Item(ItemIndex);		  
			bool Result= false;
			if(ArrayItem.GetAttribute(cXsiNull)== "")//is not null
			{
				IsNull= false;
				if((ArrayItem.InnerXml != "")&&(ArrayItem.GetAttribute(cId) == ""))
				{
					Result= true; 
					if(ArrayItem.InnerXml.Substring(0, 1)== "<") //is an atruct
						IsStruct= true;
				}
			}
			else
				IsNull= true;				
			return Result;
		}

		public ValueType ReadArrayItemSimpleTypeFromXml(XmlElement ParentElement, int ItemIndex, string ItemTypeName)
		{
			XmlElement ArrayItem= (XmlElement)ParentElement.ChildNodes.Item(ItemIndex);//at this moment you know that this field is a value type
			string ItemValue= ArrayItem.InnerXml;
			string XsiType= ((XmlElement)ArrayItem).GetAttribute(cXsiType);
			if(XsiType != "")
				return GetValueTypeFromXsdType(XsiType, ItemValue);
			else
				return GetValueTypeFromString(ItemTypeName, ItemValue);
		}

		public ReferenceTypes GetReferenceType(string FieldName, XmlElement ParentElement, ref XmlElement RefElement)
		{          
			XmlElement ArrayItem= (XmlElement)ParentElement.GetElementsByTagName(FieldName).Item(0);
			RefElement= ArrayItem;
			ReferenceTypes Result= ReferenceTypes.Object_Type;
			if(ArrayItem.GetAttribute(cId) != "") //is an string 
				Result= ReferenceTypes.String_Type;
			else
			{
				string RefIndex= ArrayItem.GetAttribute(cSoapRef);
				if(RefIndex != "") //is a other reference
				{		
					int Id= RefIndex.IndexOf("-");
					Id= Convert.ToInt32(RefIndex.Substring(Id + 1, RefIndex.Length - 1 - Id));
					string RefName= GetReferenceNameFromId(Convert.ToInt32(Id), ref RefElement);					
					if(RefName== cSoapEncArray) //is an array 
						Result= ReferenceTypes.Array_Type;
					else
						if(RefName == "")
						Result= ReferenceTypes.String_Type;
					else
					{
						if((RefName.IndexOf(cDelegateSerClass) != -1)&&(RefElement.ChildNodes.Item(0).Name== cDelegateType)) //is a delegates
							Result= ReferenceTypes.Delegate_Type;
						else
							Result= ReferenceTypes.Object_Type;
					}
				}
			}        
			return Result;
		}

		public ReferenceTypes GetReferenceType(int index, XmlElement ParentElement, ref XmlElement RefElement)
		{
			XmlElement ArrayItem= (XmlElement)ParentElement.ChildNodes.Item(index);
			RefElement= ArrayItem;
			ReferenceTypes Result= ReferenceTypes.Object_Type;
			if(ArrayItem.GetAttribute(cId) != "") //is an string 
				Result= ReferenceTypes.String_Type;
			else
			{
				string RefIndex= ArrayItem.GetAttribute(cSoapRef);
				if(RefIndex != "") //is a other reference
				{		
					int Id= RefIndex.IndexOf("-");
					Id= Convert.ToInt32(RefIndex.Substring(Id + 1, RefIndex.Length - 1 - Id));
					string RefName= GetReferenceNameFromId(Convert.ToInt32(Id), ref RefElement);
					if(RefName== cSoapEncArray) //is an array 
						Result= ReferenceTypes.Array_Type;
					else
						if(RefName == "")
						Result= ReferenceTypes.String_Type;
					else
					{
						if((RefName.IndexOf(cDelegateSerClass) != -1)&&(RefElement.ChildNodes.Item(0).Name== cDelegateType)) //is a delegates
							Result= ReferenceTypes.Delegate_Type;
						else
							Result= ReferenceTypes.Object_Type;
					}
				}
			}        
			return Result;
		}

		public string GetFullObjectLocation(string XsdType, out string NsName)
		{
			string AssemblyName= ReadAssemblyNameFromXml(XsdType);
			int StartIndex= XsdType.LastIndexOf(":");
			string NsIndex= XsdType.Substring(1, StartIndex - 1);
			NsName= ReadNamespaceFromXml(NsIndex);
			NsName= NsName +   "." + XsdType.Substring(StartIndex + 1, XsdType.Length - StartIndex - 1);
			return AssemblyName; 
		}

		private ValueType GetValueTypeFromNotSimpleType(string XsdType, string ItemValue)
		{
			string NsName;
			string AssemblyName= GetFullObjectLocation(XsdType, out NsName);		    
			Type ItemType= Assembly.Load(AssemblyName).GetType(NsName);
			object Result;
			if(ItemType.IsEnum)//is an enum
				Result= Enum.Parse(ItemType, ItemValue);
			else //is a char
			{
				if(ItemValue == cDefaultValue)				
					Result=  new char();
				else				 
					Result= Char.Parse(ItemValue);				
			}
			return (ValueType)Result;             
		}

		private string GetCLRTypeFromXsdType(string XsdType)
		{
			string Result= "";
			switch(XsdType)
			{				
				case "xsd:int"          :Result= "Int32";
					break;
				case "xsd:short"        :Result= "Int16";
					break;
				case "xsd:long"         :Result= "Int64";
					break;
				case "xsd:unsignedInt"  :Result= "UInt32";
					break;
				case "xsd:unsignedShort":Result= "UInt16";
					break;
				case "xsd:unsignedLong" :Result= "UInt64";
					break;
				case "xsd:byte"         :Result= "Byte";
					break;
				case "xsd:decimal"      :Result= "Decimal";
					break;
				case "xsd:double"       :Result= "Double"; 
					break;				
				case "xsd:boolean"      :Result= "Boolean";
					break;
				case "xsd:dateTime"     :Result= "DateTime";
					break;
				case "xsd:string"       :Result= "String";
					break;
			}
			return Result;
		}

		public ValueType GetValueTypeFromXsdType(string XsdType, string ItemValue)
		{
			ValueType Result= null;
			switch(XsdType)
			{				
				case "xsd:int"          : Result= Convert.ToInt32(ItemValue);
					break;
				case "xsd:short"        :Result= Convert.ToInt16(ItemValue);
					break;
				case "xsd:long"         :Result= Convert.ToInt64(ItemValue);
					break;
				case "xsd:unsignedInt"  :Result= Convert.ToUInt32(ItemValue);
					break;
				case "xsd:unsignedShort":Result= Convert.ToUInt16(ItemValue);
					break;
				case "xsd:unsignedLong" :Result= Convert.ToUInt64(ItemValue);
					break;
				case "xsd:byte"         :Result= Convert.ToByte(ItemValue);
					break;
				case "xsd:decimal"      :Result= Convert.ToDecimal(ItemValue);
					break;
				case "xsd:double"       :Result= Convert.ToDouble(ItemValue); 
					break;				
				case "xsd:boolean"      :Result= Convert.ToBoolean(ItemValue);
					break;
				case "xsd:dateTime"     :Result= Convert.ToDateTime(ItemValue);
					break;
				default                 :Result= GetValueTypeFromNotSimpleType(XsdType, ItemValue);
					break;
			}
			return Result;
		}

		/**Value types reader**/
		public string ReadValueTypeFromXml(string FieldName, XmlElement ParentElement/*string ParentElement, string ParentElementId*/)
		{
			XmlNode XmlField= ParentElement.GetElementsByTagName(FieldName).Item(0);///*FCurrentElement*/GetCurrentElement(ParentElement, ParentElementId).GetElementsByTagName(FieldName).Item(0); 
			if(XmlField!= null)
				return XmlField.InnerXml;
			else
				return null;
		}
		
		public ValueType GetValueTypeFromString(string fieldtype, string fieldvalue)
		{
			ValueType result= null;
			switch(fieldtype)
			{
				case "Int32"    : result= Convert.ToInt32(fieldvalue);
					break;
				case "Int16"    : result= Convert.ToInt16(fieldvalue);
					break;
				case "Int64"    : result= Convert.ToInt64(fieldvalue);
					break;
				case "UInt32"   : result= Convert.ToUInt32(fieldvalue);
					break;
				case "UInt16"   : result= Convert.ToUInt16(fieldvalue);
					break;
				case "UInt64"   : result= Convert.ToUInt64(fieldvalue);
					break;
				case "Byte"     : result= Convert.ToByte(fieldvalue);
					break;
				case "Decimal"  : result= Convert.ToDecimal(fieldvalue);
					break;
				case "Double"   : result= Convert.ToDouble(fieldvalue);
					break;				
				case "Boolean"  : result= Convert.ToBoolean(fieldvalue);
					break;
				case "DateTime" : result= Convert.ToDateTime(fieldvalue);
					break;
				case "Char"     : result= Convert.ToChar(fieldvalue);
					break;
			}
			return result;
		}		

		/**Structs reader**/
		public string ReadStructValueFieldFromXml(/*string XmlParentElement, */string FieldName, XmlElement ParentElement/*string XmlParentElementId, */, string StructName, bool NestedStruct)
		{
			XmlElement FieldElement;		
			FieldElement= (XmlElement)ParentElement.GetElementsByTagName(FieldName).Item(0);			
			if(FieldElement != null)
				return FieldElement.InnerXml;
			else
				return null;
		}

		public void ReadStructParentElementFromXml(string StructName, XmlElement ParentElement, bool NestedStruct)
		{
			if(!NestedStruct)//is not a nested struct
				DeepElement= (XmlElement)ParentElement.GetElementsByTagName(StructName).Item(0);//GetCurrentElement(XmlParentElement, XmlParentElementId).GetElementsByTagName(StructName).Item(0);
			else			
				DeepElement= (XmlElement)DeepElement.GetElementsByTagName(StructName).Item(0);
		}

		/**Assemblies reader**/
		public string GetAssemblyNameFromId(int id)
		{		  
			XmlNodeList ObjList= ((XmlElement)FXmlDoc.DocumentElement.GetElementsByTagName("Body", xmlns_SOAP_ENV).Item(0)).ChildNodes;
			bool Continue= true;
			int index= 0;
			string AssemblyName= "";
			while((Continue)&&(index<= ObjList.Count - 1))
			{
				string refid= ((XmlElement)ObjList.Item(index)).GetAttribute(cId);			  
				int StartIndex= refid.IndexOf("-");
				refid=  refid.Substring(StartIndex + 1, refid.Length - 1 - StartIndex);
				if(refid== id.ToString())
				{
					Continue= false;				  
					AssemblyName= ReadAssemblyNameFromXml(((XmlElement)ObjList.Item(index)).Name);
				}
				else
					index++;
			}
			return AssemblyName;
		}

		private string GetReferenceNameFromId(int id, ref XmlElement RefElement)
		{
			XmlNodeList ObjList= ((XmlElement)FXmlDoc.DocumentElement.GetElementsByTagName("Body", xmlns_SOAP_ENV).Item(0)).ChildNodes;
			bool Continue= true;
			int index= 0;
			string Result= "";
			while((Continue)&&(index<= ObjList.Count - 1))
			{
				string refid= ((XmlElement)ObjList.Item(index)).GetAttribute(cId);			  
				int StartIndex= refid.IndexOf("-");
				refid=  refid.Substring(StartIndex + 1, refid.Length - 1 - StartIndex);
				if(refid== id.ToString())
				{
					Continue= false;				  
					Result= ((XmlElement)ObjList.Item(index)).Name;
					RefElement= (XmlElement)ObjList.Item(index);
				}
				else
					index++;
			}
			return Result;
		}

		public string ReadAssemblyNameFromXml(string ParentElementName)
		{
			string RefName= ParentElementName.Substring(1, ParentElementName.LastIndexOf(":") - 1);
			string XmlNamespaceName= startxmlns + RefName;
			string AttributeName= FXmlDoc.DocumentElement.Attributes.GetNamedItem(XmlNamespaceName).Value;
			int StartIndex= AttributeName.LastIndexOf("/");
			string AssemblyName= AttributeName.Substring(StartIndex + 1, AttributeName.Length - StartIndex - 1);
			if(AssemblyName == "System")
				AssemblyName= basicassembly;
			return AssemblyName;
		}

		/**Namespace reader**/
		public string ReadNamespaceFromXml(string ReferenceName)
		{
			string XmlNamespaceName= startxmlns + ReferenceName; 
			string AttributeName= FXmlDoc.DocumentElement.Attributes.GetNamedItem(XmlNamespaceName).Value;
			int StartIndex= AttributeName.LastIndexOf("/");
			string Result= "";
			string NsName= AttributeName.Substring(StartIndex + 1, AttributeName.Length - StartIndex - 1);
			if(NsName == "System")
				Result= NsName;
			else
			{
				string TmpStr= AttributeName.Substring(0, StartIndex);
				StartIndex= TmpStr.LastIndexOf("/");
				Result= TmpStr.Substring(StartIndex + 1, TmpStr.Length - StartIndex - 1);
			}
			return Result;
		  
		}

		/**Utils**/
		public XmlElement GetCurrentElement(string ElementName, string ElementId)
		{
			string RefId= cObjectRef + ElementId;
			XmlNodeList NodeList = ((XmlElement)FXmlDoc.DocumentElement.GetElementsByTagName("Body", xmlns_SOAP_ENV).Item(0)).GetElementsByTagName(ElementName);
			bool Continue= true;
			int index= 0;
			string Result= "";
			XmlElement ActElement = null;
			while((Continue)&&(index <= NodeList.Count - 1))
			{
				ActElement= (XmlElement)NodeList.Item(index);
				if(ActElement.GetAttribute("id")== RefId)//the attributes match
					Continue= false;				
				else
					index++;
			}
			if(!Continue)
				return ActElement;
			else
				return null;
		}
		
		private int ReadFieldIdValueFromXml(string FieldName, XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.GetElementsByTagName(FieldName).Item(0);//(XmlElement)GetCurrentElement(XmlParentElement, XmlParentElementId).GetElementsByTagName(FieldName).Item(0);
			if(FieldElement.GetAttribute(cXsiNull)== "") //if it is not a null field
			{
				string refvalue= FieldElement.GetAttribute(cId);
				int StartIndex= refvalue.IndexOf("-");
				return Convert.ToInt32(refvalue.Substring(StartIndex + 1, refvalue.Length - 1 - StartIndex));
			}
			else
				return -1;
		}

		private int ReadFieldIdValueFromXml(int ItemIndex, XmlElement ParentElement)
		{
			XmlElement FieldElement= (XmlElement)ParentElement.ChildNodes.Item(ItemIndex);
			if(FieldElement.GetAttribute(cXsiNull)== "") //if it is not a null field
			{
				string refvalue= FieldElement.GetAttribute(cId);
				int StartIndex= refvalue.IndexOf("-");
				return Convert.ToInt32(refvalue.Substring(StartIndex + 1, refvalue.Length - 1 - StartIndex));
			}
			else
				return -1;
		}
	}	
}
