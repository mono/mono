//
// System.Web.WebCategoryAttribute.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Web
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class WebCategoryAttribute : CategoryAttribute
	{
		public WebCategoryAttribute (string category)
			: base (category)
		{
		}

		protected override string GetLocalizedString (string value)
		{
			return Locale.GetText (value);
		}
	}
}
