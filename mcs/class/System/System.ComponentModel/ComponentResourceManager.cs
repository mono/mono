//
// System.ComponentModel.ComponentResourceManager.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//
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

#if NET_1_1

using System;
using System.Resources;
using System.Globalization;
using System.Collections;
using System.Reflection;

namespace System.ComponentModel {

	public class ComponentResourceManager : ResourceManager
	{
		public ComponentResourceManager ()
		{
		}

		public ComponentResourceManager (Type t)
			: base (t)
		{
		}

		public void ApplyResources (object value, string objectName)
		{
			ApplyResources (value, objectName, System.Threading.Thread.CurrentThread.CurrentCulture);
		}

		public virtual void ApplyResources (object value, string objectName, CultureInfo culture)
		{
			string objKey = objectName + ".";
			Type type = value.GetType ();
			
			ResourceSet rset = GetResourceSet (culture, true, true);
			foreach (DictionaryEntry di in rset)
			{
				string key = di.Key as string;
				if (key.StartsWith (objKey)) {
					key = key.Substring (objKey.Length);
					PropertyInfo pi = type.GetProperty (key);
					if (pi != null && pi.CanWrite)
						pi.SetValue (value, Convert.ChangeType (di.Value, pi.PropertyType), null);
				}
			}
			rset.Close ();
		}
	}
}

#endif
