//
// BaseVsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using Microsoft.Vsa;
	using System;

	public abstract class BaseVsaEngine : IVsaEngine
	{
		public BaseVsaEngine (string language, string version, bool supportDebug)
		{
			throw new NotImplementedException ();
		}

		public System._AppDomain AppDomain {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public System.Security.Policy.Evidence Evidence {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string ApplicationBase {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public System.Reflection.Assembly Assembly {
			get { throw new NotImplementedException (); }
		}

		public bool GenerateDebugInfo {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool IsCompiled {
			get { throw new NotImplementedException (); }
		}

		public bool IsDirty {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool IsRunning {
			get { throw new NotImplementedException (); }
		}

		public IVsaItems Items {
			get { throw new NotImplementedException (); }
		}

		public string Language {
			get { throw new NotImplementedException (); }
		}

		public int LCID {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string Name {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string RootMoniker {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string RootNamespace {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public IVsaSite Site {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string Version {
			get { throw new NotImplementedException (); }
		}

		public virtual void Close ()
		{
			throw new NotImplementedException ();
		}

		public virtual bool Compile ()
		{
			throw new NotImplementedException ();
		}

		public virtual object GetOption (string name)
		{
			throw new NotImplementedException ();
		}

		public virtual void InitNew ()
		{
			throw new NotImplementedException ();
		}

		public virtual void LoadSourceState (IVsaPersistSite site)
		{
			throw new NotImplementedException ();
		}

		public virtual void Reset ()
		{
			throw new NotImplementedException ();
		}

		public virtual void RevokeCache ()
		{
			throw new NotImplementedException ();
		}

		public virtual void Run ()
		{
			throw new NotImplementedException ();
		}

		public virtual void SetOption (string name, object value)
		{
			throw new NotImplementedException ();
		}

		public virtual void SaveCompiledState (out byte [] pe, out byte [] debugInfo)
		{
			throw new NotImplementedException ();
		}

		public virtual void SaveSourceState (IVsaPersistSite site)
		{
			throw new NotImplementedException ();
		}

		public abstract bool IsValidIdentifier (string ident);
	}

	public class BaseVsaSite : IVsaSite
	{
		public virtual byte [] Assembly {
			get { throw new NotImplementedException (); }
		}

		public virtual byte [] DebugInfo {
			get { throw new NotImplementedException (); }
		}

		public virtual void GetCompiledState (out byte [] pe, out byte [] debugInfo)
		{
			throw new NotImplementedException ();
		}

		public virtual object GetEventSourceInstance (string itemName, string eventSourceName)
		{
			throw new NotImplementedException ();
		}

		public virtual object GetGlobalInstance (string name)
		{
			throw new NotImplementedException ();
		}

		public virtual void Notify (string notify, object optional)
		{
			throw new NotImplementedException ();
		}

		public virtual bool OnCompilerError (IVsaError error)
		{
			throw new NotImplementedException ();
		}
	}


	public abstract class BaseVsaStartup
	{
		public void SetSite (IVsaSite site)
		{
			throw new NotImplementedException ();
		}

		public abstract void Startup ();
		
		public abstract void Shutdown ();
	}
}							