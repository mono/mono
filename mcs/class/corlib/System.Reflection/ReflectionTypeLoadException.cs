//
// System.Reflection.ReflectionTypeLoadException
//
// Sean MacIsaac (macisaac@ximian.com)
// Dunan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	public sealed class ReflectionTypeLoadException : SystemException
	{
		// Fields
		private Exception[] loaderExceptions;
		private Type[] types;
		
		// Constructors
		public ReflectionTypeLoadException (Type[] classes, Exception[] exceptions)
			: base (Locale.GetText ("The classes in the module cannot be loaded."))
		{
			loaderExceptions = exceptions;
			types = classes;
		}

		public ReflectionTypeLoadException (Type[] classes, Exception[] exceptions, string message)
			: base (message)
		{
			loaderExceptions = exceptions;
			types = classes;
		}
			
		private ReflectionTypeLoadException (SerializationInfo info, StreamingContext sc): base (info, sc)
		{
			types = (Type[]) info.GetValue ("Types", typeof (Type[]));
			loaderExceptions = (Exception[]) info.GetValue ("Exceptions", typeof (Exception[]));
		}
		
		// Properties
		public Type[] Types
		{
			get { return types; }
		}

		public Exception[] LoaderExceptions
		{
			get { return loaderExceptions; }
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("Types", types);
			info.AddValue ("Exceptions", loaderExceptions);
		}
	
	}
}
