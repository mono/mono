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
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		Assembly Assembly {
			get;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		Evidence Evidence {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		bool GenerateDebugInfo {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		bool IsCompiled {
			get;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		bool IsDirty {
			get;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		bool IsRunning {
			get;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		IVsaItems Items {
			get;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		string Language {
			get;
		}

	
		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		int LCID {
			get;
			set;	
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		string Name {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		string RootMoniker {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		string RootNamespace {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		IVsaSite Site {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		string Version {
			get;
		}
			


		// public methods

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Close ();

		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		bool Compile ();


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		object GetOption (string name);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void InitNew ();


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		bool IsValidIdentifier (string identifier);

	
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void LoadSourceState (IVsaPersistSite site);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Reset ();

		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void RevokeCache ();


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Run ();

		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void SaveCompiledState (out byte [] pe, out byte [] pdb);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void SaveSourceState (IVsaPersistSite site);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void SetOption (string name, object value);
	}
}
		
