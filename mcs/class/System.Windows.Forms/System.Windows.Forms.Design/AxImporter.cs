//
// System.Windows.Forms.Design.AxImporter.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
namespace System.Windows.Forms.Design
{
	/// <summary>
	/// Summary description for AxImporter.
	/// </summary>
	[MonoTODO]
	public class AxImporter
	{
		[MonoTODO]
		public 	AxImporter(AxImporter.Options options)
		{
			throw new NotImplementedException ();
			//
			// TODO: Add constructor logic here
			//
		}
		[MonoTODO]
		public string[] GeneratedAsemblies{
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string[] GeneratedSoruces{
			get {
				throw new NotImplementedException ();
			}
		}
// 		[MonoTODO]
// 		public TYPELIBATTR[] GeneratedTypeLibAttribute{
// 			get {
// 				throw new NotImplementedException ();
// 			}
// 		}
		[MonoTODO]
		public string GenerateFromFile(FileInfo file){
			throw new NotImplementedException ();
		}
//		UCOMITTypeLib documented, but not implmented by Microsoft?
//		[MonoTODO]
//		public string GenerateFromTypeLibrary(UCOMITTypeLib typeLib){
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public string GenerateFromTypeLibrary(UCOMITTypeLib typeLib, Guid clsid){
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public static string GetFileOfTypeLib(ref TYPELIBATTR tlibattr){
//			throw new NotImplementedException ();
//		}

		public sealed class Options{
			public Options(){
			}

			//props
			public bool delaySign;
			public bool GenSources;
			public string keyContainer;
			public string keyfile;
			public StrongNameKeyPair keyPair;

			public bool noLogo;
			public string outputDirectory;
			public string outputName;
			public bool overwriteRCW;
			public byte[] publicKey;
//implment
//			public AxImporter.IReferenceResolver references;
			public bool silentMode;
			public bool verboseMode;
		}

	}
}
