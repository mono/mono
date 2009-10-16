//
// System.ComponentModel.Design.DesigntimeLicenseContextSerializer.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Carlo Kok  (ck@remobjects.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2009 Carlo Kok
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

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesigntimeLicenseContextSerializer
	{

		private DesigntimeLicenseContextSerializer ()
		{
		}

		public static void Serialize (Stream o, string cryptoKey, DesigntimeLicenseContext context)
		{
			object [] lData = new object [2];
			lData [0] = cryptoKey;
			Hashtable lNewTable = new Hashtable ();
			foreach (DictionaryEntry et in context.keys) {
				lNewTable.Add (((Type) et.Key).AssemblyQualifiedName, et.Value);
			}
			lData[1] = lNewTable;

			BinaryFormatter lFormatter = new BinaryFormatter ();
			lFormatter.Serialize (o, lData);
		}
	
	}
}
