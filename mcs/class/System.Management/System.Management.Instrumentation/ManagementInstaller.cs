//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Collections;
using System.Configuration.Install;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime;
using System.Security.Permissions;
using System.Text;

namespace System.Management.Instrumentation
{
	public class ManagementInstaller : Installer
	{
		private static bool helpPrinted;

		private string mof;

		public override string HelpText
		{
			get
			{
				if (!ManagementInstaller.helpPrinted)
				{
					ManagementInstaller.helpPrinted = true;
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("/MOF=[filename]\n");
					stringBuilder.Append(string.Concat(" ", RC.GetString("FILETOWRITE_MOF"), "\n\n"));
					stringBuilder.Append("/Force or /F\n");
					stringBuilder.Append(string.Concat(" ", RC.GetString("FORCE_UPDATE")));
					return string.Concat(stringBuilder.ToString(), base.HelpText);
				}
				else
				{
					return base.HelpText;
				}
			}
		}

		static ManagementInstaller()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementInstaller()
		{
		}

		public override void Commit(IDictionary savedState)
		{
			base.Commit(savedState);
			if (base.Context.Parameters.ContainsKey("mof"))
			{
				string item = base.Context.Parameters["mof"];
				if (item == null || item.Length == 0)
				{
					item = base.Context.Parameters["assemblypath"];
					if (item == null || item.Length == 0)
					{
						item = "defaultmoffile";
					}
					else
					{
						item = Path.GetFileName(item);
					}
				}
				if (item.Length >= 4)
				{
					if (string.Compare(item.Substring(item.Length - 4, 4), ".mof", StringComparison.OrdinalIgnoreCase) != 0)
					{
						item = string.Concat(item, ".mof");
					}
				}
				else
				{
					item = string.Concat(item, ".mof");
				}
				base.Context.LogMessage(string.Concat(RC.GetString("MOFFILE_GENERATING"), " ", item));
				using (StreamWriter streamWriter = new StreamWriter(item, false, Encoding.Unicode))
				{
					streamWriter.WriteLine("//**************************************************************************");
					streamWriter.WriteLine("//* {0}", item);
					streamWriter.WriteLine("//**************************************************************************");
					streamWriter.WriteLine(this.mof);
				}
			}
		}

		public override void Install(IDictionary savedState)
		{
			FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.Read, base.Context.Parameters["assemblypath"]);
			fileIOPermission.Demand();
			base.Install(savedState);
			base.Context.LogMessage(RC.GetString("WMISCHEMA_INSTALLATIONSTART"));
			string item = base.Context.Parameters["assemblypath"];
			Assembly assembly = Assembly.LoadFrom(item);
			SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assembly);
			schemaNaming.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
			if (schemaNaming != null)
			{
				if (!schemaNaming.IsAssemblyRegistered() || base.Context.Parameters.ContainsKey("force") || base.Context.Parameters.ContainsKey("f"))
				{
					base.Context.LogMessage(string.Concat(RC.GetString("REGESTRING_ASSEMBLY"), " ", schemaNaming.DecoupledProviderInstanceName));
					schemaNaming.RegisterNonAssemblySpecificSchema(base.Context);
					schemaNaming.RegisterAssemblySpecificSchema();
				}
				this.mof = schemaNaming.Mof;
				base.Context.LogMessage(RC.GetString("WMISCHEMA_INSTALLATIONEND"));
				return;
			}
			else
			{
				return;
			}
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);
		}
	}
}