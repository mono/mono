//
// System.Diagnostics.FileVersionInfo.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System;
using System.Runtime.CompilerServices;

namespace System.Diagnostics {
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

		private FileVersionInfo() {
			/* This is here just to shut the compiler up */
			comments=null;
			companyname=null;
			filedescription=null;
			filename=null;
			fileversion=null;
			internalname=null;
			language=null;
			legalcopyright=null;
			legaltrademarks=null;
			originalfilename=null;
			privatebuild=null;
			productname=null;
			productversion=null;
			specialbuild=null;
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
				return(filename);
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
		
		public static FileVersionInfo GetVersionInfo(string fileName) {
			FileVersionInfo fvi=new FileVersionInfo();

			fvi.GetVersionInfo_internal(fileName);
			
			return(fvi);
		}
		
		public override string ToString() {
			string str;

			str="File:             " + filename + "\n";
			str+="InternalName:     " + internalname + "\n";
			str+="OriginalFilename: " + originalfilename + "\n";
			str+="FileVersion:      " + fileversion + "\n";
			str+="FileDescription:  " + filedescription + "\n";
			str+="Product:          " + productname + "\n";
			str+="ProductVersion:   " + productversion + "\n";
			str+="Debug:            " + isdebug + "\n";
			str+="Patched:          " + ispatched + "\n";
			str+="PreRelease:       " + isprerelease + "\n";
			str+="PrivateBuild:     " + isprivatebuild + "\n";
			str+="SpecialBuild:     " + isspecialbuild + "\n";
			str+="Language          " + language + "\n";
			
			return(str);
		}
	}
}

