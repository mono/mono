//
// StackFrame.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;
	using Microsoft.JScript.Vsa;

	public sealed class StackFrame : ScriptObject, IActivationObject
	{
		public object [] localVars;
		public object closureInstance;


		public object GetDefaultThisObject ()
		{
			throw new NotImplementedException ();
		}


		public FieldInfo GetField (string name, int lexLevel)
		{
			throw new NotImplementedException ();
		}

		
		public GlobalScope GetGlobalScope ()
		{
			throw new NotImplementedException ();
		}


		FieldInfo IActivationObject.GetLocalField (string name)
		{
			throw new NotImplementedException ();
		}


		public override MemberInfo [] GetMember (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public override MemberInfo [] GetMembers (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public object GetMemberValue (string name, int lexLevel)
		{
			throw new NotImplementedException ();
		}


		public static void PushStackFrameForStaticMethod (RuntimeTypeHandle thisClass,
								  JSLocalField [] fields,
								  VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static void PushStackFrameForMethod (object thisObj, JSLocalField [] fields,
							    VsaEngine engine)
		{
			throw new NotImplementedException ();
		}
	}
}