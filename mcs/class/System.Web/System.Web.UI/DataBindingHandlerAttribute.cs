/**
 * Namespace: System.Web.UI
 * Class:     DataBindingHandlerAttribute
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
using System.Reflection;

namespace System.Web.UI
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DataBindingHandlerAttribute : Attribute
	{
		public static readonly DataBindingHandlerAttribute Default;

		private string handlerTypeName;

		public DataBindingHandlerAttribute()
		{
			handlerTypeName = String.Empty;
		}

		public DataBindingHandlerAttribute(string typeName)
		{
			handlerTypeName = typeName;
		}

		public DataBindingHandlerAttribute(Type type)
		{
			handlerTypeName = type.AssemblyQualifiedName;
		}

		public string HandlerTypeName
		{
			get
			{
				return handlerTypeName;
			}
		}
	}
}
