//
// VsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Vsa.Tmp
{
	using System;
	using System.Reflection;
	using Microsoft.Vsa;

	public class VsaEngine /* : BaseVsaEngine, IRedirectOutput */
	{
		public VsaEngine ()
		{
			throw new NotImplementedException ();
		}

		public VsaEngine (bool b)
		{
			throw new NotImplementedException ();
		}

		public virtual IVsaEngine Clone (AppDomain appDom)
		{
			throw new NotImplementedException ();
		}

		public virtual bool CompileEmpty ()
		{
			throw new NotImplementedException ();
		}

		public virtual void ConectEvents ()
		{
			throw new NotImplementedException ();
		}

		public static GlobalScope CreateEngineAndGetGlobalScope (bool b, string [] assemblyName)
		{
			throw new NotImplementedException ();
		}

		public static GlobalScope CreateEngineAndGetGlobalScopeWithType (bool b, string [] assemblyNames,
										 RuntimeTypeHandle callTypeHandle)
		{
			throw new NotImplementedException ();
		}

		public static VsaEngine CreateEngine ()
		{
			throw new NotImplementedException ();
		}

		public static VsaEngine CreateEngineWithType (RuntimeTypeHandle callTypeHandle)
		{
			throw new NotImplementedException ();
		}

		public virtual void DisconnectEvents ()
		{
			throw new NotImplementedException ();
		}

		public virtual Assembly GetAssembly ()
		{
			throw new NotImplementedException ();
		}

		public virtual IVsaScriptScope GetGlobalScope ()
		{
			throw new NotImplementedException ();
		}

		public virtual GlobalScope GetMainScope ()
		{
			throw new NotImplementedException ();
		}

		public virtual Module GetModule ()
		{
			throw new NotImplementedException ();
		}

		public ArrayConstructor GetOriginalArrayConstructor ()
		{
			throw new NotImplementedException ();
		}

		public ObjectConstructor GetOriginalObjectConstructor ()
		{
			throw new NotImplementedException ();
		}

		public RegExpConstructor GetOriginalRegExpConstructor ()
		{
			throw new NotImplementedException ();
		}

		public void InitVsaEngine (string moniker, IVsaSite site)
		{
			throw new NotImplementedException ();
		}

		public virtual void Interrupt ()
		{
			throw new NotImplementedException ();
		}


		/*
		public override bool IsValidIdentifier (string ident)
		{
			throw new NotImplementedException ();
		}
		*/


		public LenientGlobalObject LenientGlobalObject {
			get { throw new NotImplementedException (); }
		}

		public ScriptObject PopScriptObject ()
		{
			throw new NotImplementedException ();
		}

		public void PushScriptObject (ScriptObject obj)
		{
			throw new NotImplementedException ();
		}

		public virtual void RegisterEventSource (string name)
		{
			throw new NotImplementedException ();
		}

		/*
		public override void Reset ()
		{
			throw new NotImplementedException ();
		}
		*/

		public virtual void Restart ()
		{
			throw new NotImplementedException ();
		}

		public virtual void RunEmpty ()
		{
			throw new NotImplementedException ();
		}

		public virtual void Run (AppDomain appDom)
		{
			throw new NotImplementedException ();
		}

		public ScriptObject ScriptObjectStackTop ()
		{
			throw new NotImplementedException ();
		}

		public virtual void SetOutputStream (IMessageReceiver output)
		{
			throw new NotImplementedException ();
		}
	}
}