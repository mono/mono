//
// VsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Reflection;
using Microsoft.Vsa;
using System.Collections;

namespace Microsoft.JScript.Vsa {

	public class VsaEngine : BaseVsaEngine,  IRedirectOutput {
		
		static private Hashtable options;
		internal VsaScriptScope global_scope;
		internal Stack globals;

		public VsaEngine ()
			: this (true)
		{
			InitOptions ();
		}

		public VsaEngine (bool b)
			: base ("JScript", "0.0.1", true)
		{
			globals = new Stack (4);			
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

		public static GlobalScope CreateEngineAndGetGlobalScope (bool fast, string [] assembly_names)
		{
			int i, n;
			GlobalScope scope;

			VsaEngine engine = new VsaEngine (fast);			
			engine.InitVsaEngine ("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa",
					      new DefaultVsaSite ());
			n = assembly_names.Length;

			for (i = 0; i < n; i++) {
				string assembly_name = assembly_names [i];
				VsaReferenceItem r = (VsaReferenceItem) engine.Items.CreateItem (assembly_name,
												 VsaItemType.Reference,
												 VsaItemFlag.None);
				r.AssemblyName = assembly_name;
			}
			scope = (GlobalScope) engine.GetGlobalScope ().GetObject ();
			return scope;
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
			if (global_scope == null) {
				global_scope = new VsaScriptScope (this, "Global", null);
			}

			return global_scope;
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
			RootMoniker = moniker;
			Site = site;
			InitNewCalled = true;
			RootNamespace = "JScript.DefaultNamespace";
			IsDirty = true;
			compiled = false;
		}

		public virtual void Interrupt ()
		{
			throw new NotImplementedException ();
		}


		public override bool IsValidIdentifier (string ident)
		{
			throw new NotImplementedException ();
		}


		public LenientGlobalObject LenientGlobalObject {
			get { throw new NotImplementedException (); }
		}

		public ScriptObject PopScriptObject ()
		{
			ScriptObject script_obj = null;

			try {
				script_obj = (ScriptObject) globals.Pop ();
			} catch (NullReferenceException e) {
			}
			return script_obj;
		}

		public void PushScriptObject (ScriptObject obj)
		{
			try {
				globals.Push (obj);
			} catch (NullReferenceException e) {
			}
		}

		public virtual void RegisterEventSource (string name)
		{
			throw new NotImplementedException ();
		}

		public override void Reset ()
		{
			throw new NotImplementedException ();
		}

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

		internal static void InitOptions ()
		{
			options = new Hashtable ();
		
			options.Add ("AlwaysGenerateIL", false);
			options.Add ("CLSCompliant", false);
			options.Add ("DebugDirectory", "");
			options.Add ("Defines", new Hashtable ());
			options.Add ("Fast", true);
			// FIXME: "ManagedResources"
			options.Add ("Print", true);
			options.Add ("UseContextRelativeStatics", false);
			options.Add ("VersionSafe", false);
			options.Add ("WarnAsError", false);
			options.Add ("WarningLevel", 1);
			options.Add ("Win32Resource", "");
		}

		protected override object GetSpecificOption (string name)
		{
			object opt;

			try {
				opt = options [name];
			} catch (NotSupportedException e) {
				throw new VsaException (VsaError.OptionNotSupported);
			}
			return opt;
		}

		protected override void SetSpecificOption (string name, object val)
		{
			try {
				options [name] = val;
			} catch (NotSupportedException e) {
				throw new VsaException (VsaError.OptionNotSupported);
			}
		}
	}

	class DefaultVsaSite : BaseVsaSite {		
	}
}
