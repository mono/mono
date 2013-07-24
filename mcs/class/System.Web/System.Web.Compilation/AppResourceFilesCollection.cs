//
// System.Web.Compilation.AppResourceFilesCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Util;

namespace System.Web.Compilation 
{
	internal enum AppResourceFileKind
	{
		NotResource,
		ResX,
		Resource,
		Binary
	};

	internal class AppResourcesLengthComparer<T>: IComparer<T>
	{
		int CompareStrings (string a, string b)
		{
			if (a == null || b == null)
				return 0;
			return (int)b.Length - (int)a.Length;
		}

		int IComparer<T>.Compare (T _a, T _b) 
		{
			string a = null, b = null;
			if (_a is string && _b is string) {
				a = _a as string;
				b = _b as string;
			} else if (_a is List<string> && _b is List<string>) {
				List<string> tmp = _a as List<string>;
				a = tmp [0];
				tmp = _b as List<string>;
				b = tmp [0];
			} else if (_a is AppResourceFileInfo && _b is AppResourceFileInfo) {
				AppResourceFileInfo tmp = _a as AppResourceFileInfo;
				a = tmp.Info.Name;
				tmp = _b as AppResourceFileInfo;
				b = tmp.Info.Name;
			} else
				return 0;
			return CompareStrings (a, b);
		}
	}
	
	internal class AppResourceFilesCollection
	{
		List <AppResourceFileInfo> files;
		bool isGlobal;
		string sourceDir;

		public string SourceDir {
			get { return sourceDir; }
		}
		
		public bool HasFiles {
			get {
				if (String.IsNullOrEmpty (sourceDir))
					return false;
				return files.Count > 0;
			}
		}

		public List <AppResourceFileInfo> Files {
			get { return files; }
		}
		
		public AppResourceFilesCollection (HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			
			this.isGlobal = true;
			this.files = new List <AppResourceFileInfo> ();

			string resourcePath;
			resourcePath = Path.Combine (HttpRuntime.AppDomainAppPath, "App_GlobalResources");
			if (Directory.Exists (resourcePath))
				sourceDir = resourcePath;
		}

		public AppResourceFilesCollection (string parserDir)
		{
			if (String.IsNullOrEmpty (parserDir))
				throw new ArgumentException ("parserDir cannot be empty");
			this.isGlobal = true;
			this.files = new List <AppResourceFileInfo> ();

			string resourcePath;
			resourcePath = Path.Combine (parserDir, "App_LocalResources");
			if (Directory.Exists (resourcePath)) {
				sourceDir = resourcePath;
				HttpApplicationFactory.WatchLocationForRestart (sourceDir, "*");
			}
		}
		
		public void Collect ()
		{
			if (String.IsNullOrEmpty (sourceDir))
			    return;
			DirectoryInfo di = new DirectoryInfo (sourceDir);
			FileInfo[] infos = di.GetFiles ();
			if (infos.Length == 0)
				return;

			string extension;
			AppResourceFileInfo arfi;
			AppResourceFileKind kind;
			
			foreach (FileInfo fi in infos) {
				if (Acceptable (fi, out kind))
					arfi = new AppResourceFileInfo (fi, kind);
				else
					continue;

				files.Add (arfi);
			}

			if (isGlobal && files.Count == 0)
				return;
			AppResourcesLengthComparer<AppResourceFileInfo> lcFiles = new AppResourcesLengthComparer<AppResourceFileInfo> ();
			files.Sort (lcFiles);
		}

		bool Acceptable (FileInfo fileInfo, out AppResourceFileKind kind)
		{
			if ((fileInfo.Attributes & FileAttributes.Hidden) != 0)
			{
				kind = AppResourceFileKind.NotResource;
				return false;
			}
			
			switch (fileInfo.Extension.ToLower (Helpers.InvariantCulture))
			{
				default:
					kind = AppResourceFileKind.NotResource;
					return false;

				case ".resx":
					kind = AppResourceFileKind.ResX;
					return true;

				case ".resource":
					kind = AppResourceFileKind.Resource;
					return true;
			}
		}
	};
};

