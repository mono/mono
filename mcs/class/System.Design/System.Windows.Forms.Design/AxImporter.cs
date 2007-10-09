//
// System.Windows.Forms.Design.AxImporter.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Design
{
	[MonoTODO]
	public class AxImporter
	{
		#region Public Instance Constructors

		[MonoTODO]
		public AxImporter (AxImporter.Options options)
		{
			this.options = options;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		[MonoTODO]
		public string[] GeneratedAssemblies
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string[] GeneratedSources
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Public Instance Properties

		#region Public Instance Methods

		[MonoTODO]
		public TYPELIBATTR[] GeneratedTypeLibAttributes
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string GenerateFromFile (FileInfo file)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateFromTypeLibrary (UCOMITypeLib typeLib)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateFromTypeLibrary (UCOMITypeLib typeLib, Guid clsid)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Public Static Methods

		[MonoTODO]
		public static string GetFileOfTypeLib (ref TYPELIBATTR tlibattr)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Static Methods

		#region Internal Instance Fields

		internal AxImporter.Options options;

		#endregion Internal Instance Fields

		public sealed class Options
		{
			#region Public Instance Constructors

			public Options ()
			{
			}

			#endregion Public Instance Constructors

			#region Public Instance Fields

			[MonoTODO]
			public bool delaySign;
			[MonoTODO]
			public bool genSources;
			[MonoTODO]
			public string keyContainer;
			[MonoTODO]
			public string keyFile;
			[MonoTODO]
			public StrongNameKeyPair keyPair;

			[MonoTODO]
			public bool noLogo;
			[MonoTODO]
			public string outputDirectory;
			[MonoTODO]
			public string outputName;
			[MonoTODO]
			public bool overwriteRCW;
			[MonoTODO]
			public byte[] publicKey;
			[MonoTODO]
			public AxImporter.IReferenceResolver references;
			[MonoTODO]
			public bool silentMode;
			[MonoTODO]
			public bool verboseMode;
#if NET_2_0
			[MonoTODO]
			public bool msBuildErrors;
#endif

			#endregion Public Instance Fields
		}

		public interface IReferenceResolver
		{
			string ResolveActiveXReference (UCOMITypeLib typeLib);
			string ResolveComReference (AssemblyName name);
			string ResolveComReference (UCOMITypeLib typeLib);
			string ResolveManagedReference (string assemName);
		}
	}
}
