/****************************************************/
/*ObjectSerializer class implementation             */
/*Author: Jesús M. Rodríguez de la Vega             */
/*gsus@brujula.net                                  */
/****************************************************/

using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace System.Runtime.Serialization.Formatters.soap
{
	internal class ObjectSerializer
	{		
		/*******const's section******/		
		const string cStringType       = "System.String";
		const string startxmlns        = "xmlns:a";
		const string startdoc          =  "<SOAP-ENV:Envelope " + 
			"xmlns:xsi= \"http://www.w3.org/2001/XMLSchema-instance\""  + " " + 
			"xmlns:xsd= \"http://www.w3.org/2001/XMLSchema\""            + " " + 
			"xmlns:SOAP-ENC= \"http://schemas.xmlsoap.org/soap/encoding/\""  + " " + 
			"xmlns:SOAP-ENV= \"http://schemas.xmlsoap.org/soap/envelope/\""  + " " + 
			"SOAP-ENV:encodingStyle= \"http://schemas.xmlsoap.org/soap/encoding/\">" + " " + 
			"<SOAP-ENV:Body>"    + " " + 
			"</SOAP-ENV:Body>"   + " " + 
			"</SOAP-ENV:Envelope>";
		const string xmlnsassem         = "http://schemas.microsoft.com/clr/nsassem/";
		const string xmlns              = "http://schemas.microsoft.com/clr/ns/";
		const string basicassembly      = "mscorlib";
		/*****Delegates const******/
		const string cDelegatesClass    = "System.MulticastDelegate";
		const string cDelegateSerClass  = "DelegateSerializationHolder"; 
		const string cDelegateType      = "DelegateType";
		const string cDelegateAssembly  = "DelegateAssembly";
		const string cTarget            = "Target";
		const string cTargetTypAssem    = "TargetTypeAssembly";
		const string cTargetTypName     = "TargetTypeName";
		const string cMethodName        = "MethodName";
		const string cDefaultValue      = "_0x00_";
		/******field's sections******/
		private Stream FCurrentStream;
		private ArrayList AssemblyList;  //the assemblies's been serialized		
		public  ArrayList XmlObjectList; //the list of the xml representation of all objects 			    
		private ArrayList FObjectList;   //the listof  the object been seralized
		private SoapWriter ObjectWrt;
		private string FCurrentXml;// the object's xml representation 
		/******method's section******/
		private void AddAssemblyToXml(string assemns)
		{
			XmlDocument xmldoc = new XmlDocument();			
			xmldoc.LoadXml(FCurrentXml);
			XmlElement RootElemt = xmldoc.DocumentElement;
			string xmlns = startxmlns + AssemblyList.Count.ToString();
			XmlAttribute NewAttr= xmldoc.CreateAttribute(xmlns);
			RootElemt.SetAttributeNode(NewAttr);
			RootElemt.SetAttribute(xmlns, assemns);
			FCurrentXml= xmldoc.InnerXml;
		}

		private int AddAssembly(string assname, string nespname)			
		{		
			string XmlAssNs;

			if(assname == basicassembly)			 
				XmlAssNs= xmlns + nespname;			
			else
				XmlAssNs= xmlnsassem + nespname + '/' + assname;
			int Result= AssemblyList.IndexOf(XmlAssNs);
			if(Result< 0)	
			{
				Result= AssemblyList.Add(XmlAssNs);	 			  
				AddAssemblyToXml(XmlAssNs);
			}
			return Result;
		}

		private int AddObject(object graph, out bool AlreadyExists)
		{
			int index= FObjectList.IndexOf(graph);
			AlreadyExists= true;
			if(index < 0)  //is a new object
			{
				AlreadyExists= false;
				index= FObjectList.Add(graph); 
			}			
			return index;			
		}

		private int AddString(object StrObject, out bool AlreadyExists)
		{
			int index= FObjectList.IndexOf(StrObject);
			AlreadyExists= true;
			if(index < 0)  //is a new object
			{
				AlreadyExists= false;
				index= FObjectList.Add(StrObject); 
			}
			return index;
		}
		/******Xml Writer Methods******/
		private void AddObjectTagToXml(object ObjectField, int ParentIndex, string ObjectName)
		{
		}
		private void AddSimpleTagToXml()
		{
		}
		private int AddAssemblytoXml(Type ObjectType)
		{
			string assname, nespname;
			assname  = ObjectType.Assembly.GetName().Name;
			nespname = ObjectType.Namespace;
			return AddAssembly(assname, nespname); 
		}
		/******Serialization Methods******/
		private void SerializeEnum(object ActualObject, FieldInfo field, int ObjectIndex)
		{
			string FieldName= field.Name;
			string FieldValue= field.GetValue(ActualObject).ToString();
			ObjectWrt.WriteValueTypeToXml(FieldName, FieldValue, ObjectIndex);
		}

		private void SerializeStruct(object StructValue, string FieldName, int ObjectIndex, bool IsArrayItem)
		{	
			int AssemblyIndex= AddAssemblytoXml(StructValue.GetType()) + 1;
			string StructTypeName= StructValue.GetType().Name;
			ObjectWrt.WriteStructInitTagToXml(FieldName, ObjectIndex, IsArrayItem, AssemblyIndex, StructTypeName);
			FieldInfo[] fieldtypes = StructValue.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance); //get the fields	
			for(int index = 0; index<= fieldtypes.Length - 1; index++)
			{
				if(!fieldtypes[index].IsNotSerialized)
				{
					AssemblyIndex= AddAssemblytoXml(fieldtypes[index].FieldType);
					if(fieldtypes[index].FieldType.IsValueType)//if the field is a value type					
						SerializeValueTypes(StructValue, fieldtypes[index], ObjectIndex);
					else					
						SerializeReferenceTypes(ObjectIndex, fieldtypes[index].GetValue(StructValue), fieldtypes[index].FieldType, fieldtypes[index].Name);
					
				}
			}
			ObjectWrt.WriteStructEndTagToXml(FieldName, ObjectIndex);
		}

		private void SerializeValueTypes(object ActualObject, FieldInfo field, int ObjectIndex) //Serialize the value types
		{	
			if(field.FieldType.IsEnum)			  
				SerializeEnum(ActualObject, field, ObjectIndex);
			else
			{	
				if(field.FieldType.Assembly.GetName().Name == basicassembly)//is a simple field
				{
					string FieldName= field.Name;
					string FieldValue= field.GetValue(ActualObject).ToString();
					if(FieldValue.ToString().CompareTo("")==0)
						FieldValue= cDefaultValue;
					ObjectWrt.WriteValueTypeToXml(FieldName, FieldValue, ObjectIndex); 
				}
				else //is a struct field
				{
					object StructValue= field.GetValue(ActualObject);
					SerializeStruct(StructValue, field.Name, ObjectIndex, false);
				} 
			} 
		}	
	
		private void SerializeArrayItemValueType(object ArrayItem, int ArrayIndex, int AssemblyIndex, string ArrayItemsType)
		{
			if((ArrayItem.GetType().IsEnum)|(ArrayItem.GetType().Assembly.GetName().Name== basicassembly))
			{
				int ItemAssemblyIndex= AddAssemblytoXml(ArrayItem.GetType()) + 1;
				string ItemValue= ArrayItem.ToString();
				if(ArrayItem.ToString().CompareTo("")==0)
					ItemValue= cDefaultValue;
				ObjectWrt.WriteArrayValueItemToXml(ArrayItem.GetType().Name, ItemValue, ArrayIndex, ArrayItemsType, ItemAssemblyIndex); 
			}
			else //is an struct
				SerializeStruct(ArrayItem, "item", ArrayIndex, true);
		}
		
		private void SerializeReferenceTypes(int ObjectIndex, object Instance, Type InstanceType, string InstanceName)
		{
			if(InstanceType.IsArray)
			{
				Array ArrayField= (Array)Instance;
				if(ArrayField!= null)
				{
					int ArrayIndex= SerializeArray(ArrayField);
					ObjectWrt.WriteObjectFieldToXml(InstanceName, ObjectIndex, ArrayIndex);
				}
				else
					ObjectWrt.WriteNullObjectFieldToXml(InstanceName, ObjectIndex);
			}
			else
			{
				if(InstanceType.FullName == cStringType)
					SerializeString(Instance, ObjectIndex, InstanceName);
				else
				{
					if((InstanceType.BaseType != null)&&(InstanceType.BaseType.ToString() == cDelegatesClass))//is a delegate's field					
					{
						if(Instance!= null)
						{
							int DlgIndex= SerialializedDelegates(Instance, ObjectIndex);
							ObjectWrt.WriteObjectFieldToXml(InstanceName, ObjectIndex, DlgIndex);
						}
						else
							ObjectWrt.WriteNullObjectFieldToXml(InstanceName, ObjectIndex);
					}
					else
					{						
						if((InstanceType.IsClass)||(InstanceType.IsInterface)) //if the field is a class's instance or an interface
						{							
							if(Instance != null)
							{
								int FieldIndex= SerializeObject(Instance, ObjectIndex);  
								ObjectWrt.WriteObjectFieldToXml(InstanceName, ObjectIndex, FieldIndex);                   
							}
							else 
								ObjectWrt.WriteNullObjectFieldToXml(InstanceName, ObjectIndex);						
						}							
					}
				}
			}
		}

		private int SerializeArray(Array ArrayField)
		{			
			int Length= ArrayField.Length;
			int AssemblyIndex= AddAssemblytoXml(ArrayField.GetType()) + 1;
			bool AlreadyExist;
			int ArrayIndex= AddObject(ArrayField, out AlreadyExist) + 1;
			if(!AlreadyExist)
			{
				string ArrayType= ArrayField.GetType().Name;
				string ArrayItemsType= ArrayField.GetType().Name.Substring(0, ArrayField.GetType().Name.IndexOf("["));
				string XmlSchemaArrayType= ObjectWrt.GenerateSchemaArrayType(ArrayType, Length, AssemblyIndex);
				ObjectWrt.WriteArrayToXml(XmlSchemaArrayType, ArrayIndex);
				object ItemValue;
				for(int index= 0; index<= Length - 1; index++)
				{
					ItemValue= ArrayField.GetValue(index);
					if(ItemValue== null)
						ObjectWrt.WriteNullObjectFieldToXml("item", ArrayIndex);
					else
					{
						if(ItemValue.GetType().IsValueType)
						{
							SerializeArrayItemValueType(ItemValue, ArrayIndex, AssemblyIndex, ArrayItemsType);   
						} 
						else//is a reference type
						{
							SerializeReferenceTypes(ArrayIndex, ItemValue, ItemValue.GetType(), "item");
						}
					}
				}
			}
			return ArrayIndex;
		}

		private void SerializeString(object StringObject, int ObjectIndex, string StringName)
		{
			bool AlreadyExits;
			int StringIndex= AddString(StringObject, out AlreadyExits) + 1;
			if(!AlreadyExits)
			{
				ObjectWrt.WriteStringTypeToXml(StringName, StringObject.ToString(), ObjectIndex, StringIndex);
				XmlObjectList.Add("");
			}
			else
				ObjectWrt.WriteObjectFieldToXml(StringName, ObjectIndex, StringIndex);		  
		}
        
		private void SerializeStringField(string StringName, string StringValue, int ObjectIndex)
		{
			bool AlreadyExits;
			int StringIndex= AddString(StringValue, out AlreadyExits) + 1;
			if(!AlreadyExits)
			{
				ObjectWrt.WriteStringTypeToXml(StringName, StringValue, ObjectIndex, StringIndex);
				XmlObjectList.Add("");
			}
			else
				ObjectWrt.WriteObjectFieldToXml(StringName, ObjectIndex, StringIndex);		  
		}


		private int SerialializedDelegates(object DelegateObject, int ParentObjectIndex)
		{
			bool AlreadyExits;
			int AssemblyIndex= AddAssembly(basicassembly, "System") + 1;
			int DelegatesIndex= AddObject(DelegateObject, out AlreadyExits) + 1;
			if(!AlreadyExits)
			{
				MulticastDelegate DelegateObj= (MulticastDelegate)DelegateObject;
				if(DelegateObj != null)
				{
					ObjectWrt.WriteObjectToXml(AssemblyIndex, DelegatesIndex, cDelegateSerClass); //write the delegates's init
					SerializeStringField(cDelegateType, DelegateObj.GetType().FullName, DelegatesIndex); //the delegate type
					SerializeStringField(cDelegateAssembly, DelegateObj.GetType().Assembly.FullName, DelegatesIndex); //the delegate assembly				     
					int FieldIndex= SerializeObject(DelegateObj.Target, DelegatesIndex);   //Serialize the target
					ObjectWrt.WriteObjectFieldToXml(cTarget, DelegatesIndex, FieldIndex);
					SerializeStringField(cTargetTypAssem, DelegateObj.Target.GetType().Assembly.FullName, DelegatesIndex);
					SerializeStringField(cTargetTypName, DelegateObj.Target.GetType().FullName, DelegatesIndex);
					SerializeStringField(cMethodName, DelegateObj.Method.Name, DelegatesIndex);
				}
			}
			return DelegatesIndex;
		}

		private int SerializeObject(object graph, int ParentIndex)
		{			
			string ClassName;
			int AssemblyIndex, ObjectIndex;
			bool AlreadyExits;
			if(graph.GetType().IsSerializable)
			{
				object ActualObject= graph;
				ObjectIndex= AddObject(ActualObject, out AlreadyExits) + 1;  //add the object to the object's list						
				if(!AlreadyExits)
				{
					AssemblyIndex= AddAssemblytoXml(ActualObject.GetType()) + 1;//add the assembly to the assemblies's list
					ClassName= graph.GetType().Name; //the class's name
					ObjectWrt.WriteObjectToXml(AssemblyIndex, ObjectIndex, ClassName); //write the object to the xml list
					FieldInfo[] fieldtypes = ActualObject.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance); //get the fields
					for(int index = 0; index<= fieldtypes.Length - 1; index++)
					{		
						if(!fieldtypes[index].IsNotSerialized)
						{
							AssemblyIndex= AddAssemblytoXml(fieldtypes[index].FieldType);											
							if(fieldtypes[index].FieldType.IsValueType)//if the field is a value type					
								SerializeValueTypes(ActualObject, fieldtypes[index], ObjectIndex);
							else
								SerializeReferenceTypes(ObjectIndex, fieldtypes[index].GetValue(ActualObject), fieldtypes[index].FieldType, fieldtypes[index].Name);
						}
					}		
				}
				return ObjectIndex;
			}
			else
				return -15000;
		}

		public void BeginWrite() //writes the basic elements of a soap message
		{
			FCurrentXml = startdoc;			
		}

		public ObjectSerializer(Stream store) //assign the current stream
		{
			FCurrentStream = store;
			AssemblyList   = new ArrayList(); //Init the lists
			XmlObjectList  = new ArrayList();			
			FObjectList    = new ArrayList();
			ObjectWrt      = new SoapWriter();
			ObjectWrt.FXmlObjectList= XmlObjectList;			
		}

		public void CleatLists()
		{
			AssemblyList.Clear();
			XmlObjectList.Clear();			
			FObjectList.Clear();			
			FCurrentXml= "";
		}

		public void Serialize(object graph)
		{ 			
			SerializeObject(graph, 0); 
			ObjectWrt.WriteObjectListToXml(FCurrentXml, FCurrentStream);                                                                                                                                                              
			CleatLists();
		}
	}
}
