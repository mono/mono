//
// System.ComponentModel.LocalizableAttribute.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
//

using System;

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class LocalizableAttribute : Attribute
	{

		private bool localizable;
		
		public static readonly LocalizableAttribute Default = new LocalizableAttribute (false);
		public static readonly LocalizableAttribute No = new LocalizableAttribute (false);
		public static readonly LocalizableAttribute Yes = new LocalizableAttribute (true);

		
		public LocalizableAttribute (bool localizable)
		{
			this.localizable = localizable;
		}

		public bool IsLocalizable {
			get {
				return localizable;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is LocalizableAttribute))
				return false;
			if (obj == this)
				return true;
			return ((LocalizableAttribute) obj).IsLocalizable == localizable;
		}

		public override int GetHashCode ()
		{
			return localizable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return localizable == LocalizableAttribute.Default.IsLocalizable;
		}
	}
}

