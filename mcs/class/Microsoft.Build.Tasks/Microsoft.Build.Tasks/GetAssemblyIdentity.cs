//
// GetAssemblyIdentity.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Yuta Sato       (cannotdebug@gmail.com)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class GetAssemblyIdentity : TaskExtension {

		ITaskItem [] assemblies;
		ITaskItem [] assembly_files;

		public GetAssemblyIdentity ()
		{
		}

		public override bool Execute ()
		{
			List <ITaskItem> assembliesList = new List <ITaskItem> ();
			ITaskItem [] items = this.AssemblyFiles;

			foreach (ITaskItem i in items)
			{
				AssemblyName asmName;

				try
				{
					asmName = AssemblyName.GetAssemblyName (i.ItemSpec);
				}

				catch (Exception ex)
				{
					base.Log.LogErrorWithCodeFromResources ("GetAssemblyIdentity.CouldNotGetAssemblyName", new object[]
					{
						i.ItemSpec,
						ex.Message
					});
					continue;
				}

				ITaskItem j = new TaskItem (asmName.FullName);
				j.SetMetadata ("Name", asmName.Name);

				byte[] publicKeyToken = asmName.GetPublicKeyToken ();
				if (publicKeyToken != null)
				{
					string hexToken = BytesToHex (publicKeyToken);
					j.SetMetadata ("PublicKeyToken", hexToken);
				}

				if (asmName.Version != null)
					j.SetMetadata ("Version", asmName.Version.ToString());

				if (asmName.CultureInfo != null)
					j.SetMetadata ("Culture", asmName.CultureInfo.ToString());

				i.CopyMetadataTo (j);
				assembliesList.Add (j);
			}
			
			this.Assemblies = assembliesList.ToArray ();
			return !base.Log.HasLoggedErrors;
		}

		private static string BytesToHex (byte[] bytes)
		{
			StringBuilder builder = new StringBuilder ();
			foreach (byte b in bytes)
				builder.Append (b.ToString ("X02", CultureInfo.InvariantCulture) );
			return builder.ToString();
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

#endif
