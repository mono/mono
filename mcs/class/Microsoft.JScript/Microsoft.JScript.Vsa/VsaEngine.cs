//
// VsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// Copyright (C) 2005 Novell, Inc (http://novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

		public override bool Compile ()
		{
			ArrayList code_items = new ArrayList ();
			
			foreach (IVsaItem item in Items) {
				if (item is IVsaCodeItem)
					code_items.Add (item);
				else if (item is IVsaReferenceItem)
					continue;
				else
					throw new Exception ("FIXME: VsaItemType.AppGlobal");
			}
			Parser parser = new Parser (code_items);
			ScriptBlock [] blocks = parser.ParseAll ();
			if (blocks != null) {
				SemanticAnalyser.Run (blocks, (Assembly []) GetOption ("assemblies"));
				object outfile = GetOption ("out");

				if (outfile == null)
					CodeGenerator.Run ((string) GetOption ("first_source"), blocks);
				else
					CodeGenerator.Run ((string) outfile, blocks);

				Console.WriteLine ("Compilation succeeded");
			}
			return false;
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
			return ArrayConstructor.Ctr;
		}

		public ObjectConstructor GetOriginalObjectConstructor ()
		{
			return ObjectConstructor.Ctr;
		}

		public RegExpConstructor GetOriginalRegExpConstructor ()
		{
			return RegExpConstructor.Ctr;
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
			} catch (NullReferenceException) {
			}
			return script_obj;
		}

		public void PushScriptObject (ScriptObject obj)
		{
			try {
				globals.Push (obj);
			} catch (NullReferenceException) {
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
			return (ScriptObject) globals.Peek ();
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
			} catch (NotSupportedException) {
				throw new VsaException (VsaError.OptionNotSupported);
			}
			return opt;
		}

		public override void SetOption (string name, object value)
		{
			SetSpecificOption (name, value);
		}

		protected override void SetSpecificOption (string name, object val)
		{
			try {
				options [name] = val;
			} catch (NotSupportedException) {
				throw new VsaException (VsaError.OptionNotSupported);
			}
		}

		internal Parser GetParser ()
		{
			return new Parser ();
		}
	}

	class DefaultVsaSite : BaseVsaSite {		
	}
}
