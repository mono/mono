//
// BaseVsaEngine.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.Vsa {

	public abstract class BaseVsaEngine : IVsaEngine {
		
		private const int ROOT_MONIKER_MAX_SIZE = 256;
		private bool monikerAlreadySet;
		private string rootMoniker;

		private bool closed;
		private bool busy;

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

		// FIXME: We have to check that the moniker is not already in use.	    
		public string RootMoniker {
			get { return rootMoniker; }
			set {
				if (monikerAlreadySet)
					throw new VsaException (VsaError.RootMonikerAlreadySet);
				else if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else {
					MonikerState state = ValidateRootMoniker (value);

					switch (state) {
					case MonikerState.Valid:
						rootMoniker = value;
						monikerAlreadySet = true;
						break;

					case MonikerState.Invalid:
						throw new VsaException (VsaError.RootMonikerInvalid);

					case MonikerState.ProtocolInvalid:
						throw new VsaException (VsaError.RootMonikerProtocolInvalid);
					}
				}
			}	
		}

		internal static MonikerState ValidateRootMoniker (string n)
		{
			if (n == null || n == "" || n.Length > ROOT_MONIKER_MAX_SIZE)
				return MonikerState.Invalid;

			try {
				Uri uri = new Uri (n);
				string protocol = uri.Scheme;

				if (protocol == "http" || protocol == "file" || 
				    protocol == "ftp" || protocol == "gopher" || 
				    protocol == "https" || protocol == "mailto")
					return MonikerState.ProtocolInvalid;

				return MonikerState.Valid;

			} catch (UriFormatException e) {
				return MonikerState.Invalid;
			}
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

	public class BaseVsaSite : IVsaSite {

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


	public abstract class BaseVsaStartup {

		public void SetSite (IVsaSite site)
		{
			throw new NotImplementedException ();
		}

		public abstract void Startup ();
		
		public abstract void Shutdown ();
	}

	internal enum MonikerState {
		Valid,
		Invalid,
		ProtocolInvalid
	}		
}
