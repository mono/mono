// System.Reflection.ReflectionTypeLoadException
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
	public sealed class ReflectionTypeLoadException : SystemException
	{

		public Type[] Types {
			get {
				// FIXME
				return null;
			}
		}

		public Exception[] LoaderExceptions {
			get {
				// FIXME
				return null;
			}
		}
	}
}
