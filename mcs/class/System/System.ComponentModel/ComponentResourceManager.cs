//
// System.ComponentModel.ComponentResourceManager.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Gert Driesen (drieseng@users.sourceforge.net)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace System.ComponentModel
{
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
			ApplyResources (value, objectName, (CultureInfo) null);
		}

		public virtual void ApplyResources (object value, string objectName, CultureInfo culture)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (objectName == null)
				throw new ArgumentNullException ("objectName");
			if (culture == null)
				culture = CultureInfo.CurrentUICulture;

			Hashtable resources = IgnoreCase ? CollectionsUtil.CreateCaseInsensitiveHashtable ()
				: new Hashtable ();
			BuildResources (culture, resources);

			string objKey = objectName + ".";
			CompareOptions compareOptions = IgnoreCase ?
				CompareOptions.IgnoreCase : CompareOptions.None;
			Type type = value.GetType ();

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			if (IgnoreCase)
				bindingFlags |= BindingFlags.IgnoreCase;

			foreach (DictionaryEntry de in resources) {
				string resName = (string) de.Key;
				if (!culture.CompareInfo.IsPrefix (resName, objKey, compareOptions))
					continue;

				string propName = resName.Substring (objKey.Length);
				PropertyInfo pinfo = type.GetProperty (propName,
					bindingFlags);
				if (pinfo == null || !pinfo.CanWrite)
					continue;
				object resValue = de.Value;
				if (resValue == null || pinfo.PropertyType.IsInstanceOfType (resValue))
					pinfo.SetValue (value, resValue, null);
			}
		}

		void BuildResources (CultureInfo culture, Hashtable resources)
		{
			// first load resources for less specific culture
			if (culture != culture.Parent) {
				BuildResources (culture.Parent, resources);
			}

			ResourceSet rs = GetResourceSet (culture, true, false);
			if (rs != null) {
				foreach (DictionaryEntry de in rs)
					resources [(string) de.Key] = de.Value;
			}
		}
	}
}
