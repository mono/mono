//
// System.ComponentModel.ComponentResourceManager.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//
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
