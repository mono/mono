//
// System.Web.UI.IgnoreUnknownContentAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Property)]
	internal sealed class IgnoreUnknownContentAttribute : Attribute
	{
		
		public IgnoreUnknownContentAttribute ()
		{
		}
	}
}
	
