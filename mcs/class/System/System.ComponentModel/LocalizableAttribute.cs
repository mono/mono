//
// System.ComponentModel.LocalizableAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class LocalizableAttribute : Attribute {
		bool localizable;
		
		public static readonly LocalizableAttribute No;
		public static readonly LocalizableAttribute Yes;

		static LocalizableAttribute ()
		{
			No = new LocalizableAttribute (false);
			Yes = new LocalizableAttribute (false);
		}
		
		public LocalizableAttribute (bool localizable)
		{
			this.localizable = localizable;
		}

		public bool IsLocalizable {
			get {
				return localizable;
			}
		}
		
	}
}
