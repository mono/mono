using System;
using System.Xml;
using System.Collections;

namespace System.Xml.Schema
{
	/// <summary>
	///  All Methods in this class should use XmlConvert. Some Methods are not present in the
	///  MS Implementation. We should provide them.
	/// </summary>
	internal class XmlSchemaUtil
	{
		private XmlSchemaUtil()
		{}

		public static void CompileID(string id,  XmlSchemaObject xso, Hashtable idCollection, ValidationEventHandler h)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#ID
			// 1. ID must be a NCName
			// 2. ID must be unique in the schema
			if(id == null)
				return;
			if(!CheckNCName(id)) 
				xso.error(h,id+" is not a valid id attribute");
			else if(idCollection.ContainsKey(id))
				xso.error(h,"Duplicate id attribute "+id);
			else
				idCollection.Add(id,xso);
		}

		[MonoTODO]
		public static bool CheckAnyUri(string uri)
		{
			 return true;
		}

		public static bool CheckToken(string token)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#token
			return true;
		}

		public static bool CheckNormalizedString(string token)
		{
			return true;
		}

		public static bool CheckLanguage(string lang)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#language
			return true;
		}
		public static bool CheckNCName(string name)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#NCName
			try
			{
				XmlConvert.VerifyNCName(name);
				return true;
			}
			catch(Exception ex)
			{
				return false;
			}
		}

		public static bool CheckQName(XmlQualifiedName qname)
		{
			return true;
		}
		public static bool IsValidQName(string qname)
		{
			foreach(string part in qname.Split(new char[]{':'},2))
			{
				if(!CheckNCName(part))
					return false;
			}
			return true;
		}

		//FIXME: First remove all the multiple instances of whitespace and then return the strings.
		//The current method returns empty strings if there are two or more consecutive whitespaces.
		public static string[] SplitList(string list)
		{
			if(list == null || list == string.Empty)
				return new String[0];

			string[] listarr = list.Split(new char[]{' ','\t','\n'});
			int pos=0;
			int i = 0;
			for(i=0;i<listarr.Length;i++)
			{
				if(listarr[i] != null && listarr[i] != String.Empty)
				{
					listarr[pos++] = listarr[i];
				}
			}
			if(pos == i)
				return listarr;
			string[] retarr = new String[pos];
			if(pos!=0)
				Array.Copy(listarr, retarr, pos);
			return retarr;
		}

		public static void ReadUnhandledAttribute(XmlReader reader, XmlSchemaObject xso)
		{
			if(reader.Prefix == "xmlns")
				xso.Namespaces.Add(reader.LocalName, reader.Value);
			else if(reader.Name == "xmlns")
				xso.Namespaces.Add("",reader.Value);
			else
			{
				if(xso.unhandledAttributeList == null)
					xso.unhandledAttributeList = new System.Collections.ArrayList();
				XmlAttribute attr = new XmlDocument().CreateAttribute(reader.LocalName,reader.NamespaceURI);
				attr.Value = reader.Value;
				xso.unhandledAttributeList.Add(attr);
			}
		}

		public static bool ReadBoolAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			try
			{
				bool val = XmlConvert.ToBoolean(reader.Value);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return false;
			}
		}
		public static decimal ReadDecimalAttribute(XmlReader reader,  out Exception innerExcpetion)
		{
			innerExcpetion = null;
			try
			{
				decimal val = XmlConvert.ToDecimal(reader.Value);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return decimal.Zero;
			}
		}

		// Is some value is read, return it.
		// If no values return empty.
		// If exception, return none
		public static XmlSchemaDerivationMethod ReadDerivationAttribute(XmlReader reader, out Exception innerExcpetion, string name)
		{
			innerExcpetion = null;
			try
			{
				string list = reader.Value;
				string warn = "";
				XmlSchemaDerivationMethod val = 0;
				
				if(list.IndexOf("#all") != -1 && list.Trim() != "#all")
				{
					innerExcpetion = new Exception(list+" is not a valid value for "+ name +". #all if present must be the only value");
					return XmlSchemaDerivationMethod.All;
				}
				foreach(string xsdm in XmlSchemaUtil.SplitList(list))
				{
					switch(xsdm)
					{
						case "":
							val |= XmlSchemaDerivationMethod.Empty; break;
						case "#all":
							val |= XmlSchemaDerivationMethod.All; break;
						case "substitution":
							val |= XmlSchemaDerivationMethod.Substitution; break;
						case "extension":
							val |= XmlSchemaDerivationMethod.Extension; break;
						case "restriction":
							val |= XmlSchemaDerivationMethod.Restriction; break;
						case "list":
							val |= XmlSchemaDerivationMethod.List; break;
						case "union":
							val |= XmlSchemaDerivationMethod.Union; break;
						default:
							warn += xsdm + " "; break;
					}
				}
				if(warn != "")
						innerExcpetion = new Exception(warn + "is/are not valid values for " + name);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return XmlSchemaDerivationMethod.None;
			}
		}

		public static XmlSchemaForm ReadFormAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaForm val = XmlSchemaForm.None;
			switch(reader.Value)
			{
				case "qualified":
					val = XmlSchemaForm.Qualified; break;
				case "unqualified":
					val = XmlSchemaForm.Unqualified; break;
				default:
					innerExcpetion = new Exception("only qualified or unqulified is a valid value"); break;
			}
			return val;
		}

		public static XmlSchemaContentProcessing ReadProcessingAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaContentProcessing val = XmlSchemaContentProcessing.None;
			switch(reader.Value)
			{
				case "lax":
					val = XmlSchemaContentProcessing.Lax; break;
				case "strict":
					val = XmlSchemaContentProcessing.Strict; break;
				case "skip":
					val = XmlSchemaContentProcessing.Skip; break;
				default:
					innerExcpetion = new Exception("only lax , strict or skip are valid values for processContents");
					break;
			}
			return val;
		}

		public static XmlSchemaUse ReadUseAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaUse val = XmlSchemaUse.None;
			switch(reader.Value)
			{
				case "optional":
					val = XmlSchemaUse.Optional; break;
				case "prohibited":
					val = XmlSchemaUse.Prohibited; break;
				case "required":
					val = XmlSchemaUse.Required; break;
				default:
					innerExcpetion = new Exception("only optional , prohibited or required are valid values for use");
					break;
			}
			return val;
		}
		public static XmlQualifiedName ReadQNameAttribute(XmlReader reader, out Exception innerEx)
		{
			return ToQName(reader, reader.Value, out innerEx);
		}

		//While Creating a XmlQualifedName, we should check:
		// 1. If a prefix is present, its namespace should be resolvable.
		// 2. If a prefix is not present, and if the defaultNamespace is set, 
		public static XmlQualifiedName ToQName(XmlReader reader, string qnamestr, out Exception innerEx)
		{

			string ns;
			string name;
			XmlQualifiedName qname;
			innerEx = null;
			
			if(!IsValidQName(qnamestr))
			{
				innerEx = new Exception(qnamestr + " is an invalid QName. Either name or namespace is not a NCName");
				return XmlQualifiedName.Empty;
			}

			string[] values = qnamestr.Split(new char[]{':'},2);

			if(values.Length == 2)
			{
				ns = reader.LookupNamespace(values[0]);
				if(ns == null)
				{
					innerEx = new Exception("Namespace Prefix '"+values[0]+"could not be resolved");
					return XmlQualifiedName.Empty;
				}
				name = values[1];
			}
			else
			{
				//Default Namespace
				ns = reader.LookupNamespace("");
				name = values[0];
			}

			qname = new XmlQualifiedName(name,ns);
			return qname;
		}
	}
}
