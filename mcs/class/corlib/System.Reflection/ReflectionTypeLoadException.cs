// System.Reflection.ReflectionTypeLoadException
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
	public sealed class ReflectionTypeLoadException : SystemException
	{

		[MonoTODO]
		public Type[] Types {
			get {
				// FIXME
				return null;
			}
		}

		[MonoTODO]
		public Exception[] LoaderExceptions {
			get {
				// FIXME
				return null;
			}
		}
	}
}
