//
// Convert.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.JScript.Vsa;

	public sealed class Convert
	{
		public static bool IsBadIndex (AST ast)
		{
			throw new NotImplementedException ();
		}


		public static double CheckIfDoubleIsInteger (double d)
		{
			throw new NotImplementedException ();
		}


		public static Single CheckIfSingleIsInteger (Single s)
		{
			throw new NotImplementedException ();
		}


		public static object Coerce (object value, object type)
		{
			throw new NotImplementedException ();
		}


		public static object CoerceT (object value, Type t, bool explicitOK)
		{
			throw new NotImplementedException ();
		}


		public static object Coerce2 (object value, TypeCode target, 
					      bool truncationPermitted)
		{
			throw new NotImplementedException ();
		}

		public static void ThrowTypeMismatch (object val)
		{
			throw new NotImplementedException ();
		}


		public static bool ToBoolean (double d)
		{
			throw new NotImplementedException ();
		}


		public static bool ToBoolean (object value)
		{
			throw new NotImplementedException ();
		}


		public static bool ToBoolean (object value, bool explicitConversion)
		{
			throw new NotImplementedException ();
		}


		public static object ToForInObject (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static int ToInt32 (object value)
		{
			throw new NotImplementedException ();
		}


		public static double ToNumber (object value)
		{
			throw new NotImplementedException ();
		}


		public static double ToNumber (string str)
		{
			throw new NotImplementedException ();
		}


		public static object ToNativeArray (object value, RuntimeTypeHandle handle)
		{
			throw new NotImplementedException ();
		}


		public static object ToObject (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static object ToObject2 (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static string ToString (object value, bool explicitOK)
		{
			throw new NotImplementedException ();
		}


		public static string ToString (bool b)
		{
			throw new NotImplementedException ();
		}


		public static string ToString (double d)
		{	
			throw new NotImplementedException ();
		}			
	}
}