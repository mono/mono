// 
// System.Web.HttpHelper
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;
using System.IO;

namespace System.Web {
   internal class HttpHelper {
      internal static string [] ParseMultiValueHeader(string header) {
         if (null == header) {
            return null;
         }

         if (header.Length == 0) {
            return null;
         }

         // Parse the , chars
         ArrayList oValues = new ArrayList();

         string sValue;

         int iLastPos = -1;
         int iPos = header.IndexOf(",");

         while (iPos != -1) {
            sValue = header.Substring(iLastPos + 1, iPos - iLastPos - 1).Trim();
            iLastPos = iPos;

            iPos = header.IndexOf(",", iPos + 1);
            oValues.Add(sValue);
         }

         sValue = header.Substring(iLastPos + 1).Trim();
         oValues.Add(sValue);

         string [] arrValues = new string[oValues.Count];

         Array.Copy(oValues.ToArray(), 0, arrValues, 0, oValues.Count);

         return arrValues;
      }
   }
}
