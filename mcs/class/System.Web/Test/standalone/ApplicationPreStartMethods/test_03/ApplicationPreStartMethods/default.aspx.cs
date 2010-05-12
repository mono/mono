using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ApplicationPreStartMethods
{
	public partial class _default : System.Web.UI.Page
	{
		static List <string> messages = new List <string> ();

		public static List<string> PreApplicationStartMessages {
			get { return messages; }

		}

		protected void Page_Load (object sender, EventArgs e)
		{
			var sb = new StringBuilder ();

			foreach (string s in messages)
				sb.AppendLine (s);

			AppendMessageFromExternalAssembly (sb);
			foreach (object o in BuildManager.GetReferencedAssemblies ()) {
				var asm = o as Assembly;
				if (asm == null)
					continue;

				if (asm.FullName.Contains ("ExternalAssembly1")) {
					sb.AppendLine ("ExternalAssembly1 added");
					break;
				}
			}
			report.InnerText = sb.ToString ();
		}

		void AppendMessageFromExternalAssembly (StringBuilder sb)
		{
			try {
				string path = Path.Combine (HttpRuntime.AppDomainAppPath, "ExternalAssemblies", "ExternalAssembly1.dll");
				if (!File.Exists (path))
					return;

				Assembly asm = Assembly.LoadFrom (path);
				Type t = asm.GetType ("ExternalAssemblyPreStartMethods", false);
				if (t == null)
					return;

				FieldInfo fi = t.GetField ("Message");
				if (fi == null)
					return;

				sb.Append (fi.GetValue (null));
			} catch {
				// ignore
			}
		}
	}
}