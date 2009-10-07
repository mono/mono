//
// System.Web.Compilation.AppCodeCompiler: A compiler for the App_Code folder
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
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace System.Web.Util
{
	internal sealed class FileUtils
	{
		internal delegate object CreateTempFile (string path);

		static Random rnd = new Random ();
		
		internal static object CreateTemporaryFile (string tempdir, CreateTempFile createFile)
		{
			return CreateTemporaryFile (tempdir, null, null, createFile);
		}

		internal static object CreateTemporaryFile (string tempdir, string extension, CreateTempFile createFile)
		{
			return CreateTemporaryFile (tempdir, null, extension, createFile);
		}

		internal static object CreateTemporaryFile (string tempdir, string prefix, string extension, CreateTempFile createFile)
		{
			if (tempdir == null || tempdir.Length == 0)
				return null;
			if (createFile == null)
				return null;
			string path = null;
			object ret = null;
			int num;
			
			do {
				lock (rnd) {
					num = rnd.Next ();
				}

                                path = Path.Combine (tempdir,
						     String.Format ("{0}{1}{2}", (prefix != null) ? prefix + "." : "",
								    num.ToString ("x", Helpers.InvariantCulture),
								    (extension != null) ? "." + extension : ""));
                                        
                                try {
                                        ret = createFile (path);
                                } catch (System.IO.IOException) {
                                } catch { throw; }
                        } while (ret == null);

			return ret;
		}

		[Conditional ("DEVEL")]
		public static void WriteLineLog (string logFilePath, string format, params object[] parms)
		{
			WriteLog (logFilePath, format + Environment.NewLine, parms);
		}
		
		[Conditional ("DEVEL")]
		public static void WriteLog (string logFilePath, string format, params object[] parms)
		{
			string path = logFilePath != null && logFilePath.Length > 0 ? logFilePath :
				Path.Combine (Path.GetTempPath (), "System.Web.log");
			using (TextWriter tw = new StreamWriter (path, true)) {
				if (parms != null && parms.Length > 0)
					tw.Write (format, parms);
				else
					tw.Write (format);
			}
		}
	}
}
