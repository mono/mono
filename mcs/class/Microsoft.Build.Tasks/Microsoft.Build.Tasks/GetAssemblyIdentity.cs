//
// GetAssemblyIdentity.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.Reflection;
using System.Globalization;
using Microsoft.Build.Utilities;
using System.Text;

namespace Microsoft.Build.Tasks {
	public class GetAssemblyIdentity : TaskExtension {

		ITaskItem [] assemblies;
		ITaskItem [] assembly_files;

		public GetAssemblyIdentity ()
		{
		}

		[MonoTODO ("Error handling")]
		public override bool Execute ()
		{
			assemblies = new ITaskItem [assembly_files.Length];

			for (int i = 0; i < assemblies.Length; i++) {
				string file = assembly_files [i].ItemSpec;
				AssemblyName an = AssemblyName.GetAssemblyName (file);
				TaskItem item = new TaskItem (an.FullName);

				item.SetMetadata ("Version", an.Version.ToString ());

				byte[] pk = an.GetPublicKeyToken ();
				string pkStr = pk != null? ByteArrayToString (pk) : "null";
				item.SetMetadata ("PublicKeyToken", pkStr);

				CultureInfo culture = an.CultureInfo;
				if (culture != null) {
					string cn;
					if (culture.LCID == CultureInfo.InvariantCulture.LCID)
						cn = "neutral";
					else
						cn = culture.Name;
					item.SetMetadata ("Culture", cn);
				}

				assemblies[i] = item;
			}

			return true;
		}

		static string ByteArrayToString (byte[] arr)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < arr.Length; i++)
				sb.Append (arr[i].ToString ("x2"));
			return sb.ToString ();
		}

		[Output]
		public ITaskItem [] Assemblies {
			get { return assemblies; }
			set { assemblies = value; }
		}

		[Required]
		public ITaskItem [] AssemblyFiles {
			get { return assembly_files; }
			set { assembly_files = value; }
		}
	}
}
