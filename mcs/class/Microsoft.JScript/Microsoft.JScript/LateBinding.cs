//
// LateBinding.cs:
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

	public sealed class LateBinding
	{
		public object obj;

		public LateBinding (string name)
		{
			throw new NotImplementedException (); 
		}


		public LateBinding (string name, object obj)
		{
			throw new NotImplementedException ();
		}


		public object Call (object [] arguments, bool construct, bool brackets,
				    VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static object CallValue (object thisObj, object val, object [] arguments,
						bool construct, bool brackets, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static object CallValue2 (object val, object thisObj, object [] arguments,
						 bool construct, bool brackets, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public bool Delete ()
		{
			throw new NotImplementedException ();
		}


		public static bool DeleteMember (object obj, string name)
		{
			throw new NotImplementedException ();
		}


		public object GetNonMissingValue ()
		{
			throw new NotImplementedException ();
		}


		public object GetValue2 ()
		{
			throw new NotImplementedException ();
		}


		public static void SetIndexedPropertyValueStatic (object obj, object [] arguments,
								  object value)
		{
			throw new NotImplementedException ();
		}


		public void SetValue (object value)
		{
			throw new NotImplementedException ();
		}
	}
}