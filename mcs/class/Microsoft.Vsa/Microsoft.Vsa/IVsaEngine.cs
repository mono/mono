//
// IVsaEngine.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
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
		
