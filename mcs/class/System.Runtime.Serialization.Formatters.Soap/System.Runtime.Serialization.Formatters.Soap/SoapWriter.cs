/****************************************************/
/*SoapWritter class implementation                  */
/*Author: Jes·s M. Rodr­guez de la Vega             */
/*gsus@brujula.net                                  */
/****************************************************/

using System;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;

namespace System.Runtime.Serialization.Formatters.Soap
{
	
		internal class SoapWriter
		{		
			/******const section******/
			const string cNullObject     = "xsi:null=\"1\"/";
			const string cSoapEnv        = "Body";
			const string xmlns_SOAP_ENC	= "http://schemas.xmlsoap.org/soap/encoding/";
			const string xmlns_SOAP_ENV	= "http://schemas.xmlsoap.org/soap/envelope/";
			
			const string cStartTag       = "<";
			const string cEndTag         = ">";
			const string cNumber         = "#";
			const string cEqual          = "=";
			const string cTwoColon       = ":";
			const string cAssId          = "a";
			const string cslash          = "/";	
			const string cSoapRef        = "href";
			const string cObjectRef      = "ref-";
			const string cId             = "id";
			/******Array's serialization section******/
			const string cItem           = "item";
			const string cSoapEncArray   = "SOAP-ENC:Array";
			const string cSoapArrayType  = "SOAP-ENC:arrayType"; 
			const string cXsiType        = "xsi:type";
			/******field's section******/
			public ArrayList FXmlObjectList;
			public  int FReferenceNumber;
			/******method's section******/
			public string ConcatenateObjectList()
			{
				string XmlResult= "";
				object[] XmlList= FXmlObjectList.ToArray();
				for(int index= 0; index<= FXmlObjectList.Count - 1; index++)
					XmlResult= XmlResult + XmlList[index].ToString();
				return XmlResult;
			}			 

			private StringBuilder GetXmlObject(int ObjectIndex)
			{
				object[] XmlList= FXmlObjectList.ToArray();    
				string Actstring= XmlList[ObjectIndex - 1].ToString();
				return new StringBuilder(Actstring);
			}

			public void WriteObjectToXml(int AssemblyIndex, int ReferenceIndex, string ClassName)
			{
				StringBuilder ObjWriter= new StringBuilder();
				string XmlStartTag= cStartTag + cAssId + AssemblyIndex.ToString() + cTwoColon + ClassName + 
					' ' + cId + cEqual + '"' + cObjectRef + ReferenceIndex + '"' + cEndTag;   
				string XmlEndTag= cStartTag + cslash + cAssId + AssemblyIndex.ToString() + cTwoColon + ClassName + cEndTag; 
				ObjWriter.Append(XmlStartTag);			
				ObjWriter.Append(XmlEndTag); 
				FXmlObjectList.Add(ObjWriter.ToString());  
			}	
	
			public void WriteArrayToXml(string XmlSchemaArrayType, int ArrayIndex)
			{
				StringBuilder ObjWriter= new StringBuilder();
				string XmlStartTag= cStartTag + cSoapEncArray + ' ' + cId + cEqual + '"' + 
					cObjectRef + ArrayIndex + '"' + ' ' + cSoapArrayType +
					cEqual + XmlSchemaArrayType + cEndTag;
				string XmlEndTag= cStartTag + cslash + cSoapEncArray + cEndTag;
				ObjWriter.Append(XmlStartTag);
				ObjWriter.Append(XmlEndTag);
				FXmlObjectList.Add(ObjWriter.ToString());
			}

			public void WriteStringTypeToXml(string StringName, string StringValue, int ParentObjectIndex, int StringIndex)
			{
				StringBuilder ObjWriter= GetXmlObject(ParentObjectIndex);
				string StrFieldStartTag= cStartTag + StringName + ' ' + cId + cEqual + '"' + cObjectRef + StringIndex + '"' + cEndTag;
				string StrFieldEndTag  = cStartTag + cslash + StringName + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);
				ObjWriter.Insert(index, StrFieldStartTag + StringValue + StrFieldEndTag);  
				FXmlObjectList.RemoveAt(ParentObjectIndex - 1);
				FXmlObjectList.Insert(ParentObjectIndex - 1, ObjWriter.ToString());
			}		

			public void WriteObjectFieldToXml(string FieldName, int ParentObjectIndex, int ObjectIndex)
			{            
				StringBuilder ObjWriter= GetXmlObject(ParentObjectIndex);
				string XmlField= cStartTag + FieldName + ' ' + cSoapRef + cEqual
					+ '"' + cNumber + cObjectRef + ObjectIndex.ToString() + '"'+ cslash + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);
				ObjWriter.Insert(index, XmlField);
				FXmlObjectList.RemoveAt(ParentObjectIndex - 1);
				FXmlObjectList.Insert(ParentObjectIndex - 1, ObjWriter.ToString());
			}

			public void WriteNullObjectFieldToXml(string FieldName, int ParentObjectIndex)
			{            
				StringBuilder ObjWriter= GetXmlObject(ParentObjectIndex);
				string XmlField= cStartTag + FieldName + ' ' + cNullObject + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);
				ObjWriter.Insert(index, XmlField);
				FXmlObjectList.RemoveAt(ParentObjectIndex - 1);
				FXmlObjectList.Insert(ParentObjectIndex - 1, ObjWriter.ToString()); 
			}
		
			public void WriteValueTypeToXml(string FieldName, string FieldValue, int ObjectIndex)
			{           
				StringBuilder ObjWriter= GetXmlObject(ObjectIndex);
				string XmlField= cStartTag + FieldName + cEndTag + FieldValue +
					cStartTag + cslash + FieldName + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);            
				ObjWriter.Insert(index, XmlField);
				FXmlObjectList.RemoveAt(ObjectIndex - 1);
				FXmlObjectList.Insert(ObjectIndex - 1, ObjWriter.ToString());			 
			}

			public void WriteStructInitTagToXml(string StructName, int ObjectIndex, bool XsiType, int AssemblyIndex, string StructType)
			{
				StringBuilder ObjWriter= GetXmlObject(ObjectIndex);
				string StructInitTag= cStartTag + StructName;
				if(XsiType)
					StructInitTag= StructInitTag + ' ' + cXsiType + cEqual + '"' + "a" + AssemblyIndex + cTwoColon + StructType + '"';  
				StructInitTag= StructInitTag + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);            
				ObjWriter.Insert(index, StructInitTag);
				FXmlObjectList.RemoveAt(ObjectIndex - 1);
				FXmlObjectList.Insert(ObjectIndex - 1, ObjWriter.ToString());
			}

			public void WriteStructEndTagToXml(string StructName, int ObjectIndex)
			{
				StringBuilder ObjWriter= GetXmlObject(ObjectIndex);
				string StructInitTag= cStartTag + cslash + StructName + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);            
				ObjWriter.Insert(index, StructInitTag);
				FXmlObjectList.RemoveAt(ObjectIndex - 1);
				FXmlObjectList.Insert(ObjectIndex - 1, ObjWriter.ToString());
			}

			public void WriteArrayValueItemToXml(string ItemType, string ItemValue, int ArrayIndex, string ArrayItemsType, int AssemblyIndex)
			{
				StringBuilder ObjWriter= GetXmlObject(ArrayIndex);
				string ItemInitTag= cStartTag + cItem;
				if(ArrayItemsType== "Object")
				{
					ItemInitTag= ItemInitTag + ' ' + cXsiType + cEqual + '"' + GenerateXmlSchemaType(ItemType, AssemblyIndex) + '"';
				}
				ItemInitTag= ItemInitTag + cEndTag + ItemValue + cStartTag + cslash + cItem + cEndTag;
				int index= ObjWriter.ToString().LastIndexOf(cStartTag);            
				ObjWriter.Insert(index, ItemInitTag);
				FXmlObjectList.RemoveAt(ArrayIndex - 1);
				FXmlObjectList.Insert(ArrayIndex - 1, ObjWriter.ToString());
			}

			public void WriteStructTypeToXml(string StructName)
			{
			}		

			public string GenerateSchemaArrayType(string ArrayType, int ArrayLength, int AssemblyIndex)
			{
				string StrResult= ArrayType;
				string ArrayItems= ArrayType.Substring(0, ArrayType.IndexOf("["));
				string XmlSchType= GenerateXmlSchemaType(ArrayItems, AssemblyIndex);		  
				StrResult=StrResult.Replace(ArrayItems, XmlSchType);
				StrResult= StrResult.Insert(StrResult.LastIndexOf(']'), ArrayLength.ToString());		  		  
				StrResult= '"' + StrResult + '"';
				return StrResult;
			}

			public string GenerateXmlSchemaType(string TypeName, int AssemblyIndex)
			{
				string XmlSchType;
				switch(TypeName)
				{
					case "Int32"    : XmlSchType= "xsd:int";
						break;
					case "Int16"    : XmlSchType= "xsd:short";
						break;
					case "Int64"    : XmlSchType= "xsd:long";
						break;
					case "UInt32"   : XmlSchType= "xsd:unsignedInt";
						break;
					case "UInt16"   : XmlSchType= "xsd:unsignedShort";
						break;
					case "UInt64"   : XmlSchType= "xsd:unsignedLong";
						break;
					case "Byte"     : XmlSchType= "xsd:byte";
						break;
					case "Decimal"  : XmlSchType= "xsd:decimal";
						break;
					case "Double"   : XmlSchType= "xsd:double";
						break;
					case "String"   : XmlSchType= "xsd:string";
						break;
					case "Boolean"     : XmlSchType= "xsd:boolean";
						break;
					case "DateTime" : XmlSchType= "xsd:dateTime";
						break;
					default         : XmlSchType= "a" + AssemblyIndex + cTwoColon + TypeName;
						break; 					
				}
				return XmlSchType;
			}
		}
}
