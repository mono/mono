//
// IVsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;
	using System.Reflection;
	using System.Security.Policy;

	//[Guid ("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaEngine
	{
		//[Guid ("")]
		Assembly Assembly {
			get;
		}


		//[Guid ("")]
		Evidence Evidence {
			get;
			set;
		}


		//[Guid ("")]
		bool GenerateDebugInfo {
			get;
			set;
		}


		//[Guid ("")]
		bool IsCompiled {
			get;
		}


		//[Guid ("")]
		bool IsDirty {
			get;
		}


		//[Guid ("")]
		bool IsRunning {
			get;
		}


		//[Guid ("")]
		IVsaItems Items {
			get;
		}


		//[Guid ("")]
		string Language {
			get;
		}

	
		//[Guid ("")]
		int LCID {
			get;
			set;	
		}


		//[Guid ("")]
		string Name {
			get;
			set;
		}


		//[Guid ("")]
		string RootMoniker {
			get;
			set;
		}


		//[Guid ("")]
		string RootNamespace {
			get;
			set;
		}


		//[Guid ("")]
		IVsaSite Site {
			get;
			set;
		}


		//[Guid ("")]
		string Version {
			get;
		}
			


		// public methods

		//[Guid ("")]
		void Close ();

		
		//[Guid ("")]
		bool Compile ();


		//[Guid ("")]
		object GetOption (string name);


		//[Guid ("")]
		void InitNew ();


		//[Guid ("")]
		bool IsValidIdentifier (string identifier);

	
		//[Guid ("")]
		void LoadSourceState (IVsaPersistSite site);


		//[Guid ("")]
		void Reset ();

		
		//[Guid ("")]
		void RevokeCache ();


		//[Guid ("")]
		void Run ();

		
		//[Guid ("")]
		void SaveCompiledState (out byte [] pe, out byte [] pdb);


		//[Guid ("")]
		void SaveSourceState (IVsaPersistSite site);


		//[Guid ("")]
		void SetOption (string name, object value);
	}
}
		
