using System;
using System.Reflection;

namespace System
{
	public static class PropertyInfoExtensions
	{
		public static object GetValue(this PropertyInfo obj, object handle)
		{
			return obj.GetValue(handle);
		}
	}
}

