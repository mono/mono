/**
 * Namespace: System.Web.UI
 * Class:     ToolboxDataAttribute
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;

namespace System.Web.UI
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ToolboxDataAttribute : Attribute
	{
		public static readonly ToolboxDataAttribute Default = new ToolboxDataAttribute("");

		private string data;

		public ToolboxDataAttribute(string data)
		{
			this.data = data;
		}

		public string Data
		{
			get
			{
				return data;
			}
		}

		public override bool IsDefaultAttribute()
		{
			return Default.Equals(this);
		}

		public override bool Equals(object obj)
		{
			if(obj != null && obj is ToolboxDataAttribute)
			{
				ToolboxDataAttribute tda = (ToolboxDataAttribute)obj;
				return (tda.Data == Data);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
