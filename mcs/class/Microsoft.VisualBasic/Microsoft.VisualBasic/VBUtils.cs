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


using System.Globalization;
using System.Text;
using System;
using System.Reflection;
using System.Resources;

namespace Microsoft.VisualBasic {
	internal class VBUtils {
		private static string VB_FILE_NAME = "VBUtils";
		private static ResourceManager VB_RESOURCE_BUNDLE = new ResourceManager(typeof(VBUtils));

		public static bool isNumber(string str, double[] val) 
		{
			if (str == null)
				str = "";

			try {
				if (str.StartsWith("&H") || str.StartsWith("&h")) {
					val[0] = Convert.ToInt64(str.Substring(2), 16);
					return true;
				}
				else if (str.StartsWith("&O") || str.StartsWith("&o")) {
					val[0] = Convert.ToInt64(str.Substring(2), 8);
					return true;
				}
			}
			catch (Exception e) {
				//TODO:
				e.ToString();//dumb way to fix e not used comiler warning
				return false;
			}
        
			val[0] = double.Parse(str, NumberStyles.Any, null);
			return true;
		}
		
		public static string GetResourceString(string key)
		{
			string str = null;

			try {
				str = VB_RESOURCE_BUNDLE.GetString(key);
			}
			catch(Exception e) {
				// TODO

			}

			if (str == null)
				str = VB_RESOURCE_BUNDLE.GetString("ID95");

			return str;
		}
		
		// 		public static string GetResourceString(string key, string paramValue)
		// 		{
		// 			StringBuilder sb = new StringBuilder(GetResourceString(key));
		// 			sb.Replace("|1", paramValue);
		// 			return sb.toString();
		// 		}
		
		// 		public static string GetResourceString( string key,
		// 			string paramValue1,
		// 			string paramValue2)
		// 			{
		// 				StringBuilder sb = new StringBuilder(GetResourceString(key));
		// 				sb.Replace("|1", paramValue1);
		// 				sb.Replace("|2", paramValue2);
		// 				return sb.toString();
		// 			}
		
		// 		public static string GetResourceString(
		// 			string key,
		// 			string param1,
		// 			string param2,
		// 			string param3)
		// 			{
		// 				StringBuilder sb = new StringBuilder(GetResourceString(key));
		// 				sb.Replace("|1", param1);
		// 				sb.Replace("|2", param2);
		// 				sb.Replace("|3", param3);
		// 				return sb.toString();
		// 			}
		
		// 		public static string GetResourceString(
		// 			string key,
		// 			string param1,
		// 			string param2,
		// 			string param3,
		// 			string param4)
		// 			{
		// 				StringBuilder sb = new StringBuilder(GetResourceString(key));
		// 				sb.Replace("|1", param1);
		// 				sb.Replace("|2", param2);
		// 				sb.Replace("|3", param3);
		// 				sb.Replace("|4", param4);
		// 				return sb.toString();
		// 			}
		
		public static string GetResourceString(int ResourceId)
		{
			string str = "ID" + ResourceId.ToString();
			return GetResourceString(str);
		}
		
		// 		public static string GetResourceString(int ResourceId, string param1)
		// 			{
		// 				string str = "ID" + new Integer(ResourceId).toString();
		// 				return GetResourceString(str, param1);
		// 			}
		
		// 		public static java.lang.Exception VBException(java.lang.Exception ex, int hr)
		// 			{
		// 				Information.Err().SetUnmappedError(hr);
		// 				return ex;
		// 			}

	}
}
