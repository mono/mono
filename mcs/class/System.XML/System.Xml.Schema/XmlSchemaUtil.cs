using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaUtil.
	/// </summary>
	public class XmlSchemaUtil
	{
		private XmlSchemaUtil()
		{}

		[MonoTODO]
		public static bool CheckID(string id)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#ID
			// 1. ID must be a NCName
			// 2. ID must be unique in the schema
			if(!CheckNCName(id)) 
				return false;
			//If !unique

			return true;
		}
		[MonoTODO]
		public static bool CheckAnyUri(string uri)
		{
			 //check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#anyURI
			 return true;
		}
		public static bool CheckToken(string token)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#token
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
		public static string[] SplitList(string ns)
		{
			return ns.Split(new char[]{' '});
		}

		// To Be Removed
		public static XmlQualifiedName GetRandomQName()
		{
			return new XmlQualifiedName(new Random().Next(int.MaxValue).ToString());
		}
	}
}
