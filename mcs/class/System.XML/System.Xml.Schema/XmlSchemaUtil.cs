using System;

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
			return true;
		}
	}
}
