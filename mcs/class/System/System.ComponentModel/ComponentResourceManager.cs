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

using System.Resources;
using System.Globalization;

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
			ApplyResources (value, objectName, null);
		}

		[MonoTODO("Implement")]
		public virtual void ApplyResources (object value, string objectName, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
