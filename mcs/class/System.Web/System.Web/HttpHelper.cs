// 
// System.Web.HttpHelper
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
