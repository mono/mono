// System.Reflection.ParameterInfo
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
	public class ParameterInfo
	{
		public virtual object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return null;
		}

		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return null;
		}
	}
}
