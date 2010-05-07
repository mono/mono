using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		List<string> messages = new List<string> ();
		AppDomain ad = AppDomain.CurrentDomain;

		try {
			RunTest ("myFile.cache", "1", messages);

			// Subdirs not allowed
			RunTest ("subdir/myFile.cache", "2", messages);

			// File doesn't exist
			RunTest ("myAnotherFile.cache", "3", messages, true);

			//
			RunTest (null, "4", messages);

			RunTest (String.Empty, "5", messages);

			var sb = new StringBuilder ();
			foreach (string s in messages)
				sb.AppendLine (s);

			log.InnerText = sb.ToString ();
		} finally {
			ad.SetData ("TestRunData", messages);
		}
	}

	void Log (List<string> messages, string format, params object [] parms)
	{
		if (parms == null || parms.Length == 0)
			messages.Add (format);
		else
			messages.Add (String.Format (format, parms));
	}

	void RunTest (string fileName, string logTag, List <string> messages, bool noCreate = false)
	{
		string codeGenDir = null;
		if (!noCreate) {
			try {
				codeGenCreate.InnerText = codeGenDir = HttpRuntime.CodegenDir;
				Log (messages, "create[{0}]: codeGen", logTag);
			} catch (Exception ex) {
				Log (messages, "create[{0}]: error codeGen ({1})", logTag, ex.GetType ());
			}

			try {
				using (FileStream st = BuildManager.CreateCachedFile (fileName) as FileStream) {
					if (st != null) {
						string path = st.Name;
						Log (messages, "create[{0}]: fileStream", logTag);
						filePathCreate.InnerText = path;

						Log (messages, "create[{0}]: can{1} read", logTag, st.CanRead ? String.Empty : "not");
						Log (messages, "create[{0}]: can{1} write", logTag, st.CanWrite ? String.Empty : "not");

						if (codeGenDir != null && path.StartsWith (codeGenDir))
							Log (messages, "create[{0}]: pathSubdirOfCodeGen", logTag);

						if (Path.GetFileName (path) == fileName)
							Log (messages, "create[{0}]: our file name", logTag);
						using (var sw = new StreamWriter (st)) {
							sw.Write ("test");
						}
					} else
						Log (messages, "create[{0}]: stream is null", logTag);
				}
			} catch (Exception ex) {
				Log (messages, "create[{0}]: error write ({1})", logTag, ex.GetType ());
			}
		}

		try {
			codeGenRead.InnerText = codeGenDir = HttpRuntime.CodegenDir;
			Log (messages, "read[{0}]: codeGen", logTag);
		} catch (Exception ex) {
			Log (messages, "read[{0}]: error codeGen ({1})", logTag, ex.GetType ());
		}

		try {
			using (FileStream st = BuildManager.ReadCachedFile (fileName) as FileStream) {
				if (st != null) {
					string path = st.Name;
					Log (messages, "read[{0}]: fileStream", logTag);
					filePathRead.InnerText = path;

					Log (messages, "read[{0}]: can{1} read", logTag, st.CanRead ? String.Empty : "not");
					Log (messages, "read[{0}]: can{1} write", logTag, st.CanWrite ? String.Empty : "not");

					if (codeGenDir != null && path.StartsWith (codeGenDir))
						Log (messages, "read[{0}]: pathSubdirOfCodeGen", logTag);

					if (Path.GetFileName (path) == fileName)
						Log (messages, "read[{0}]: our file name", logTag);

					string contents;
					using (var sr = new StreamReader (st)) {
						contents = sr.ReadToEnd ();
					}

					if (contents != null && contents == "test")
						Log (messages, "read[{0}]: contents ok", logTag);
				} else
					Log (messages, "read[{0}]: stream is null", logTag);
			}
		} catch (Exception ex) {
			Log (messages, "read[{0}]: error read ({1})", logTag, ex.GetType ());
		}
	}
}