//
// TestResourceHelper.cs
//
// Author:
//	Alexander Köplinger (alkpli@microsoft.com)
//
// Copyright (C) Microsoft
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
using System.Globalization;
using System.Reflection;
using System.IO;

using NUnit.Framework;

namespace MonoTests.Helpers
{
	public static class TestResourceHelper
	{
		static string tempFolder;
		static Assembly currentAssembly;

		static TestResourceHelper ()
		{
			// create temp directory for extracting all the test resources to disk
			tempFolder = Path.Combine (Path.GetTempPath(), Path.GetRandomFileName ());
			Directory.CreateDirectory (tempFolder);

			currentAssembly = Assembly.GetExecutingAssembly ();
			foreach (string resourceName in currentAssembly.GetManifestResourceNames ())
			{
				// skip non-test assets
				if (!resourceName.StartsWith ("Test/"))
					continue;

				// persist the resource to disk
				var stream = currentAssembly.GetManifestResourceStream (resourceName);
				var resourcePath = Path.Combine (tempFolder, resourceName);
				Directory.CreateDirectory (Path.GetDirectoryName (resourcePath));

				using (var file = File.Create (resourcePath))
				{
					stream.CopyTo (file);
				}
			}

			// delete the temp directory at the end of the test process
			AppDomain.CurrentDomain.ProcessExit += (s,e) =>
			{
				try { Directory.Delete (tempFolder, true); }
				catch { }
			};
		}

		public static string GetFullPathOfResource (string resourceName)
		{
			return Path.Combine (tempFolder, resourceName);
		}

		public static Stream GetStreamOfResource (string resourceName)
		{
			return currentAssembly.GetManifestResourceStream (resourceName);
		}
	}
}