//
// System.Diagnostics.FileVersionInfo.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class FileVersionInfo {
		/* There is no public constructor for this class, it
		 * is initialised by the runtime.  All the private
		 * variables here are looked up by name, so dont
		 * change them without also changing the runtime
		 */
		private string comments;
		private string companyname;
		private string filedescription;
		private string filename;
		private string fileversion;
		private string internalname;
		private string language;
		private string legalcopyright;
		private string legaltrademarks;
		private string originalfilename;
		private string privatebuild;
		private string productname;
		private string productversion;
		private string specialbuild;
		private bool isdebug;
		private bool ispatched;
		private bool isprerelease;
		private bool isprivatebuild;
		private bool isspecialbuild;
		private int filemajorpart;
		private int fileminorpart;
		private int filebuildpart;
		private int fileprivatepart;
		private int productmajorpart;
		private int productminorpart;
		private int productbuildpart;
		private int productprivatepart;

		private FileVersionInfo ()
		{
#if NET_2_0
			// no nulls (for unavailable items)
			comments = null;
			companyname = null;
			filedescription = null;
			filename = null;
			fileversion = null;
			internalname = null;
			language = null;
			legalcopyright = null;
			legaltrademarks = null;
			originalfilename = null;
			privatebuild = null;
			productname = null;
			productversion = null;
			specialbuild = null;
#else
			// no nulls (for unavailable items)
			comments = String.Empty;
			companyname = String.Empty;
			filedescription = String.Empty;
			filename = String.Empty;
			fileversion = String.Empty;
			internalname = String.Empty;
			language = String.Empty;
			legalcopyright = String.Empty;
			legaltrademarks = String.Empty;
			originalfilename = String.Empty;
			privatebuild = String.Empty;
			productname = String.Empty;
			productversion = String.Empty;
			specialbuild = String.Empty;
#endif
			// This is here just to shut the compiler up
			isdebug=false;
			ispatched=false;
			isprerelease=false;
			isprivatebuild=false;
			isspecialbuild=false;
			filemajorpart=0;
			fileminorpart=0;
			filebuildpart=0;
			fileprivatepart=0;
			productmajorpart=0;
			productminorpart=0;
			productbuildpart=0;
			productprivatepart=0;
		}
		
		
		public string Comments {
			get {
				return(comments);
			}
		}

		public string CompanyName {
			get {
				return(companyname);
			}
		}

		public int FileBuildPart {
			get {
				return(filebuildpart);
			}
		}

		public string FileDescription {
			get {
				return(filedescription);
			}
		}

		public int FileMajorPart {
			get {
				return(filemajorpart);
			}
		}
		
		public int FileMinorPart {
			get {
				return(fileminorpart);
			}
		}

		public string FileName {
			get {
#if !NET_2_1
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, filename).Demand ();
				}
#endif
				return filename;
			}
		}

		public int FilePrivatePart {
			get {
				return(fileprivatepart);
			}
		}

		public string FileVersion {
			get {
				return(fileversion);
			}
		}

		public string InternalName {
			get {
				return(internalname);
			}
		}

		public bool IsDebug {
			get {
				return(isdebug);
			}
		}

		public bool IsPatched {
			get {
				return(ispatched);
			}
		}

		public bool IsPreRelease {
			get {
				return(isprerelease);
			}
		}
		
		public bool IsPrivateBuild {
			get {
				return(isprivatebuild);
			}
		}

		public bool IsSpecialBuild {
			get {
				return(isspecialbuild);
			}
		}

		public string Language {
			get {
				return(language);
			}
		}

		public string LegalCopyright {
			get {
				return(legalcopyright);
			}
		}

		public string LegalTrademarks {
			get {
				return(legaltrademarks);
			}
		}

		public string OriginalFilename {
			get {
				return(originalfilename);
			}
		}

		public string PrivateBuild {
			get {
				return(privatebuild);
			}
		}

		public int ProductBuildPart {
			get {
				return(productbuildpart);
			}
		}

		public int ProductMajorPart {
			get {
				return(productmajorpart);
			}
		}

		public int ProductMinorPart {
			get {
				return(productminorpart);
			}
		}

		public string ProductName {
			get {
				return(productname);
			}
		}

		public int ProductPrivatePart {
			get {
				return(productprivatepart);
			}
		}

		public string ProductVersion {
			get {
				return(productversion);
			}
		}

		public string SpecialBuild {
			get {
				return(specialbuild);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void GetVersionInfo_internal(string fileName);
		
		public static FileVersionInfo GetVersionInfo (string fileName)
		{
#if !NET_2_1
			if (SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.Read, fileName).Demand ();
			}
#endif

			string absolute = Path.GetFullPath (fileName);
			if (!File.Exists (absolute))
				throw new FileNotFoundException (fileName);

			FileVersionInfo fvi = new FileVersionInfo ();
			fvi.GetVersionInfo_internal (fileName);
			return fvi;
		}

		// use our own AppendFormat because NET_2_1 have only this overload
		static void AppendFormat (StringBuilder sb, string format, params object [] args)
		{
			sb.AppendFormat (format, args);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			// we use the FileName property so we don't skip the security check
			AppendFormat (sb, "File:             {0}{1}", FileName, Environment.NewLine);
			// the other informations aren't protected so we can use the members directly
			AppendFormat (sb, "InternalName:     {0}{1}", internalname, Environment.NewLine);
			AppendFormat (sb, "OriginalFilename: {0}{1}", originalfilename, Environment.NewLine);
			AppendFormat (sb, "FileVersion:      {0}{1}", fileversion, Environment.NewLine);
			AppendFormat (sb, "FileDescription:  {0}{1}", filedescription, Environment.NewLine);
			AppendFormat (sb, "Product:          {0}{1}", productname, Environment.NewLine);
			AppendFormat (sb, "ProductVersion:   {0}{1}", productversion, Environment.NewLine);
			AppendFormat (sb, "Debug:            {0}{1}", isdebug, Environment.NewLine);
			AppendFormat (sb, "Patched:          {0}{1}", ispatched, Environment.NewLine);
			AppendFormat (sb, "PreRelease:       {0}{1}", isprerelease, Environment.NewLine);
			AppendFormat (sb, "PrivateBuild:     {0}{1}", isprivatebuild, Environment.NewLine);
			AppendFormat (sb, "SpecialBuild:     {0}{1}", isspecialbuild, Environment.NewLine);
			AppendFormat (sb, "Language          {0}{1}", language, Environment.NewLine);

			return sb.ToString ();
		}
	}
}
