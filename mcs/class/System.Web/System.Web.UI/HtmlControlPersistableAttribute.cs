//
// System.Web.UI.HtmlControlPersistableAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Property)]
	internal sealed class HtmlControlPersistableAttribute : Attribute
	{
		bool persist;
		
		public HtmlControlPersistableAttribute (bool persist)
		{
			this.persist = persist;
		}

		public bool Persist {
			get { return persist; }
		}
	}
}
	
