//
// Utils.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//   Dennis Hayes (dennish@raytek.com)
//
// Copyright 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
//     2004 Novell
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */
/**
 * The class Utils.
 * 
 * CURRENT LIMITATIONS
 * 1. Method MethodToString(MethodBase Method) is not implemented
 * 2. Method SetTime is not supported (throw exception)
 * 3. Method SetDate is not supported (throw exception)
 */

using System;
using System.Globalization;
using System.Text;
using System.Reflection;

namespace Microsoft.VisualBasic.CompilerServices {
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModule] 
	//msrked as stati cin Mainsoft java code. what should it be here?
	//public static class Utils {
	 public class Utils {
		//public const int SEVERITY_ERROR = ClrInt32.MinValue;
		public const int SEVERITY_ERROR = int.MinValue;// Int32.MinValue;
		public const int FACILITY_CONTROL = 655360;
		public const int FACILITY_RPC = 65536;
		public const int FACILITY_ITF = 262144;
		public const int SCODE_FACILITY = 536805376;
		public const char chPeriod = '.';
		public const char chSpace = ' ';
		public const char chIntlSpace = '\u3000';
		public const char chZero = '0';
		public const char chHyphen = '-';
		public const char chPlus = '+';
		public const char chNull = '\0';
		public const char chLetterA = 'A';
		public const char chLetterZ = 'Z';
		public const char chColon = ':';
		public const char chSlash = '/';
		public const char chBackslash = '\\';
		public const char chTab = '\t';
		public const char chCharH0A = '\n';
		public const char chCharH0B = '\t';
		public const char chCharH0C = '\f';
		public const char chCharH0D = '\r';
		public const char chLineFeed = '\n';
		public const char chDblQuote = '\"';
		//TODO: why id this static, should it be const?
		public static char[] m_achIntlSpace = new char[] { ' ', '\u3000' };

		private const string FILE_NAME = "resources.MicrosoftVisualBasic";
		//TODO:
		//private const ResourceBundle RESOURCE_BUNDLE =
		//	ResourceBundle.getBundle(FILE_NAME);



		//    public static ResourceManager get_VBAResourceManager() throws NotImplementedException
		//    {
		//        throw new NotImplementedException("The method get_VBAResourceManager in class VisualBasic.CompilerServices.Utils is not supported");
		//    }
//Mono code
//		[MonoTODO ("Used by the Mainsoft people")]
//		internal static string GetResourceString (string s)
//		{
//                        return string.Empty;
//		}
		public static string GetResourceString(string key) {
			string str = null;
			//TODO:
			//try {
			//	//str = RESOURCE_BUNDLE.getString(key);
			//}
			//catch (MissingResourceException e) {
			//	//TODO:
			//	//try {
			//	//	str = RESOURCE_BUNDLE.getString("ID95");
			//	//}
			//	//catch (MissingResourceException ex) {
			//	//}
			//}
			//if (str == null)
			//
			//str = RESOURCE_BUNDLE.getString("ID95");

			return str;
		}

		public static string GetResourceString(string key, bool notUsed) {
			string str = null;
			//try {
			//	str = RESOURCE_BUNDLE.getString(key);
			//}
			//catch (MissingResourceException e) {
			//}
			return str;
		}

		public static string GetResourceString(string key, string paramValue) {
			StringBuilder sb = new StringBuilder(GetResourceString(key));
			sb.Replace("|1", paramValue);
			return sb.ToString();
		}

		public static string GetResourceString(
			string key,
			string paramValue1,
			string paramValue2) {
			StringBuilder sb = new StringBuilder(GetResourceString(key));
			sb.Replace("|1", paramValue1);
			sb.Replace("|2", paramValue2);
			return sb.ToString();
		}

		public static string GetResourceString(
			string key,
			string param1,
			string param2,
			string param3) {
			StringBuilder sb = new StringBuilder(GetResourceString(key));
			sb.Replace("|1", param1);
			sb.Replace("|2", param2);
			sb.Replace("|3", param3);
			return sb.ToString();
		}

		public static string GetResourceString(
			string key,
			string param1,
			string param2,
			string param3,
			string param4) {
			StringBuilder sb = new StringBuilder(GetResourceString(key));
			sb.Replace("|1", param1);
			sb.Replace("|2", param2);
			sb.Replace("|3", param3);
			sb.Replace("|4", param4);
			return sb.ToString();
		}

		public static string GetResourceString(int ResourceId) {
			string str = "ID" + StringType.FromInteger(ResourceId);
			return GetResourceString(str);
		}

		public static void ThrowException(int hr) {
			throw ExceptionUtils.VbMakeException(hr);
		}

		/**
		 * Method that returns true if given type is numeric type; otherwise false
		 * @param tc Type to check numerity
		 * @return true if given type is numeric; otherwise false
		 */
		public static bool IsNumericType(Type tc) {
			TypeCode typeCode = Type.GetTypeCode(tc);
			return IsNumericTypeCode((int)typeCode);
		}
    
		public static bool IsNumericTypeCode(int i) {
			switch (i) {
				case (int)TypeCode.Boolean:
				case (int)TypeCode.Byte:
				case (int)TypeCode.Int16:
				case (int)TypeCode.Int32:
				case (int)TypeCode.Int64:
				case (int)TypeCode.Double:
				case (int)TypeCode.Single:
				case (int)TypeCode.Decimal:
					return true;
				default:
					return false;
			}
		}

		/**
		 * This method change the given string which contains the suffix of array
		 * representation so that the the signs '(', ')'  and  ',' will appear in the
		 * end of the string. 
		 * @param sRank the given suffix 
		 * @return string the suffix after the change
		 */
		private static string changeArraySuffix(string sRank) {
			//StringBuffer sb = new StringBuffer(sRank);
			StringBuilder sb = new StringBuilder();
			
			char currentChar;

			for (int i = sRank.Length-1; i >= 0; i--) {
				currentChar = sb[i];
				if (currentChar == '(') { 
					sb.Remove(i,1);
					sb.Append(')');

				}
				else if (currentChar == ')') { 
					sb.Remove(i,1);
					sb.Append('(');

				}
				else if (currentChar == ',') { 
					sb.Remove(i,1);
					sb.Append(',');

				}
			}
			return sb.ToString();
		}
    
		/**
		 * This method returns the type name of this object in vb format
		 * @param obj the given object
		 * @return string the object's vb type name
		 * @limit COM Objects is not supported
		 */
		public static string VBFriendlyName(object obj) {
			if (obj == null)
				return "Nothing";
			//Type type = ObjectStaticWrapper.GetType(obj);//java code
			Type type = obj.GetType();
			return VBFriendlyName(type,obj);
		}

		/**
		 * Return the vb name of this type
		 * @param type the given type
		 * @return string the vb name of this type
		 */
		public static string VBFriendlyName(Type type) {
			//return VBFriendlyNameOfTypeName(type.get_Name());//java version
			return VBFriendlyNameOfTypeName(type.Name);
		}
    
		/**
		 * Return the vb name of this type.
		 * @param type the type of the given object
		 * @param obj the given object
		 * @return string the vb name of this object
		 * @limit COM Objects is not supported
		 */
		public static string VBFriendlyName(Type type, object obj) {
			
			//if (StringStaticWrapper.CompareOrdinal(type.FullName, "System.__ComObject") //java code
			if (string.CompareOrdinal(type.FullName, "System.__ComObject")
				== 0) {
				//this shape of if was writen  since get_IsCOMObject is not implemented
				//yet. in this flow only when the type name is "System.__ComObject" the
				//not implementedException is thrown.
			
				//TODO:
				//if (type.IsCOMObject)
				//	Information.TypeNameOfCOMObject(obj, false);
			}
			return VBFriendlyNameOfTypeName(type.Name);
		}
    
		/**
		 * This method return a Vb representation of the given type name.
		 * If this is an array it is changed to vb presentation with () instead of 
		 * []. 
		 * @param typename the given type name.
		 * @return string the vb presentation of the type name
		 */
		public static string VBFriendlyNameOfTypeName(string typename) {
			string tmpStr = null;
			int bracketIndex = 0;
			int length = typename.Length - 1;
			//if this is array type
			if (typename[length] == ']') {
				bracketIndex = typename.IndexOf("[");
				if (bracketIndex + 1 == length)
					tmpStr = "()";
					//when there is more than one dimention    
				else
					tmpStr =
						typename
						.Substring(bracketIndex, length - bracketIndex + 1)
						.Replace('[', '(')
						.Replace(']', ')');

				typename = typename.Substring(0, bracketIndex);
			}
        
			tmpStr = (tmpStr==null) ? "" :changeArraySuffix(tmpStr);

			//this method returns the C# equivalent name if this type name is vb unique
			//it returns null if the type name is like in C#
			string str = Information.VbTypeName(typename);     
			if (str == null)
				str = typename;
			return str + tmpStr;
		}

		/**
		 * This method change the presentation of a number.
		 * if the decimal separator isn't '.' then it's changed to '.' and the updated
		 * string return, otherwise if the string start in one of the four way : 
		 * "0." , "-0." , "+0." or " 0." ,then the '0' character is removed.
		 * if the input string doesn't contain the decimal separator the input value 
		 * is returned.    
		 * @param s the string that need to be changed
		 * @return string the updated string. 
		 */
		public static string StdFormat(string s) {
			string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

			int index = s.IndexOf(separator);
			if (index == -1)
				return s;


			//StringBuffer sb = new StringBuffer(s);
			StringBuilder sb = new StringBuilder(s);
			if (s[index] != '.') {
				sb[index]= '.';
			}

			if (sb.Length > 1 && sb[0] == '0' && sb[1] == '.')
				return sb.ToString().Substring(0,1);// sb.substring(1);//java version

			if (sb.Length > 2 && 
				(sb[0] == '-' || sb[0] == '+'|| sb[0] == ' ') && 
				sb[1] == '0' && sb[2] == '.') {
				//sb = sb.deleteCharAt(1);//java version
				sb = sb.Remove(0,1);
			}
			return sb.ToString();
		}

		/**
		 * This method returns the octal string representation of the given long
		 * @param Val the given long value
		 * @return string the Octal string representation
		 */
		public static string OctFromLong(long Val) {
			return Convert.ToString(Val, 8);
		}

		public static void SetTime(DateTime dtTime) {
			throw new NotImplementedException("Method SetTime in VisualBasic.CompilerServices.Utils is not supported");
		}

		public static void SetDate(DateTime vDate) {
			throw new NotImplementedException("Method SetDate in VisualBasic.CompilerServices.Utils is not supported");
		}

		//TODO:
		//public static DateTimeFormatInfo GetDateTimeFormatInfo() {
		//	return CultureInfo.get_CurrentCulture().get_DateTimeFormat();
		//}

		/**
		 * This method maps exception id to message id in   
		 * @param lNumber the exception constant
		 * @return int the exception message id in the resource file
		 */
		public static int MapHRESULT(int lNumber) {
			return ExceptionUtils.fromDotNetToVB(lNumber);
		}

		public static CultureInfo GetCultureInfo() {
			return CultureInfo.CurrentCulture;
		}

		public static object SetCultureInfo(CultureInfo culture) {
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			//CultureInfo.set_CurrentCulture(culture);//java code
			//TODO: CultureInfo.CurrentCulture is read only
			//CultureInfo.CurrentCulture = culture;
			return currentCulture;
		}

		public static CultureInfo GetInvariantCultureInfo() {
			return CultureInfo.InvariantCulture;
		}

		public static Encoding GetFileIOEncoding() {
			return Encoding.Default;
		}

		public static int GetLocaleCodePage() {
			return CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
		}
//mono implmentation
//		[MonoTODO]
//		public static System.Array CopyArray (System.Array SourceArray, System.Array DestinationArray)
//		{ 
//#if NET_1_1
//			long SourceLength = SourceArray.LongLength;
//			long DestinationLength = DestinationArray.LongLength;
//			long LengthToCopy = (SourceLength < DestinationLength) ? SourceLength : DestinationLength;
//			Array.Copy(SourceArray, DestinationArray, LengthToCopy); 
//#else
//			int SourceLength = SourceArray.Length;
//			int DestinationLength = DestinationArray.Length;
//			int LengthToCopy = (SourceLength < DestinationLength) ? SourceLength : DestinationLength;
//			Array.Copy(SourceArray, DestinationArray, LengthToCopy); 
//#endif
//			return DestinationArray;
//		}
//
//		[MonoTODO]
//		public static System.string MethodToString (System.Reflection.MethodBase Method)
//		{
//			 throw new NotImplementedException (); 
//		}
		/**
		 * This method copy the information from one array to another.
		 */
		public static object CopyArray(object arySrc, object aryDest) {
			if (arySrc == null || ((Array)arySrc).Length == 0)
				return aryDest;

			//int sourceRank = ArrayStaticWrapper.get_Rank(arySrc);
			int sourceRank = ((Array)arySrc).Rank;
			//if( sourceRank != ArrayStaticWrapper.get_Rank(aryDest))
			if( sourceRank != ((Array)aryDest).Rank)
				throw (InvalidCastException)ExceptionUtils.VbMakeException(new InvalidCastException(GetResourceString("Array_RankMismatch")), 9);

			Array.Copy(((Array)arySrc),((Array)aryDest),((Array)arySrc).Length);
        
			return aryDest;
		}

		public static string MethodToString(MethodBase Method) {
			throw new NotImplementedException("The method MethodToString in class VisualBasic.CompilerServices.Utils is not supported");
		}

		public static string FieldToString(FieldInfo fieldinfo) {
			throw new NotImplementedException("The method FieldToString in class VisualBasic.CompilerServices.Utils is not supported");
		}

		public static string MemberToString(MemberInfo memberinfo) {
			switch (memberinfo.MemberType) {
				case MemberTypes.Method :
					return MethodToString((MethodInfo)memberinfo);

				case MemberTypes.Field :
					return FieldToString((FieldInfo)memberinfo);
			}
			return memberinfo.Name;
		}

	}
}
