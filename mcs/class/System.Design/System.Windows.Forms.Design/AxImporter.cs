//
// System.Windows.Forms.Design.AxImporter.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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

			public bool delaySign;
			public bool genSources;
			public string keyContainer;
			public string keyFile;
			public StrongNameKeyPair keyPair;

			public bool noLogo;
			public string outputDirectory;
			public string outputName;
			public bool overwriteRCW;
			public byte[] publicKey;
			public AxImporter.IReferenceResolver references;
			public bool silentMode;
			public bool verboseMode;

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
