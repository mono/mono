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
			
		// Properties
		public Type[] Types
		{
			get { return types; }
		}

		public Exception[] LoaderExceptions
		{
			get { return loaderExceptions; }
		}

		// Method
		[MonoTODO]
		//
		// This one is a bit tough because need to serialize two arrays.
		// The serialization output comes out as
		// <Types href="#ref-4" /> 
                // <Exceptions href="#ref-5" />
		// and then goes on and appends new SOAP-ENCs, etc...
		//
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	
	}
}
