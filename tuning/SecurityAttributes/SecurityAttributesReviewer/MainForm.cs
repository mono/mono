using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CoreClr.Tools;

namespace SecurityAttributesReviewer
{
	public partial class MainForm : Form, CoreClr.Tools.UserInterface
	{
		private readonly string _profileDirectory;
		private readonly string _rapportDirecotry;

		public MainForm()
		{
			_profileDirectory = FindProfileDirectory();
			_rapportDirecotry = FindRapportDirectory();
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			foreach (var assembly in PlatformCode.Assemblies)
				_assemblyList.Items.Add(assembly);

			_assemblyList.SelectedIndex = 0;
		}

		private static string FindProfileDirectory()
		{
			string dir = FindTuningDirectory();
			return Path.Combine(dir, "TuningInput/Security");
		}

		private static string FindTuningDirectory()
		{
			string dir = Environment.CurrentDirectory;
			while (Directory.Exists(dir) && !Directory.Exists(Path.Combine(dir, "TuningInput")))
				dir = Path.GetDirectoryName(dir);
			return dir;
		}

		private static string FindRapportDirectory()
		{
			return Path.Combine(FindTuningDirectory(), "GeneratedSecurityReports");
		}

		private void _browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			var url = HttpUtility.UrlDecode(e.Url.OriginalString);
			const string signatureMarker = "api:";
			var signatureMarkerIndex = url.IndexOf(signatureMarker);
			if (signatureMarkerIndex < 0)
				return;

			e.Cancel = true;

			var methoddata = url.Substring(signatureMarkerIndex + signatureMarker.Length);
			
			ShowContextMenuFor(methoddata);
		}

		private void ShowContextMenuFor(string signature)
		{
			_menuItemAddDeclaringTypeToCriticalTypeList.Text = string.Format("Add Declaring Type '{0}' to Critical Type List",
			                                                                 DeclaringTypeFromSignature(signature));

			_menuItemAddToAuditedSafeList.Text = string.Format("Add Method '{0}' to Audited Safe List", signature);

			_menuItemAddToReviewedMethodList.Text = string.Format("Add Method '{0}' to Reviewed Method List", signature);

			_contextMenu.Tag = signature;
			_contextMenu.Show(Cursor.Position);
		}

		private static string DeclaringTypeFromSignature(string signature)
		{
			int declaringBegin = signature.IndexOf(' ') + 1;
			int declaringEnd = signature.IndexOf("::", declaringBegin);
			return signature.Substring(declaringBegin, declaringEnd - declaringBegin);
		}

		private void _assemblyList_SelectedIndexChanged(object sender, EventArgs e)
		{
			NavigateToSelectedAssembly();
		}

		private void NavigateToSelectedAssembly()
		{
			_browser.Navigate(DetectMethodPrivileges.Program.Files.PublicApisInfoFor(SelectedAssembly(), _rapportDirecotry));
		}

		private void _refreshButton_Click(object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem(state => RunMethodPrivilegeDetection());
		}

		private void RunMethodPrivilegeDetection()
		{
			ClearLog();
			UseWaitCursor = true;
			try
			{
				DetectMethodPrivileges.Program.UI = this;
				DetectMethodPrivileges.Program.Main();
			}
			catch (Exception x)
			{
				LogWithColor(Color.Red, "Error: {0}", x);
			}
			finally
			{
				RefreshBrowser();
			}
			UseWaitCursor = false;
		}

		public void Info(string format, params object[] args)
		{
			LogWithColor(Color.Blue, format, args);
		}

		public void Warning(string format, params object[] args)
		{
			LogWithColor(Color.Red, format, args);
		}
		public void ClearLog()
		{
			try
			{
				_log.Clear();
			} catch (Exception)
			{
			}
		}

		private void LogWithColor(Color color, string format, params object[] args)
		{
			if (_log.InvokeRequired)
			{
				Action a = () => LogWithColor(color, format, args);
				_log.BeginInvoke(a);
				return;
			}

			var text = string.Format(format, args);

			_log.SelectionColor = color;
			_log.AppendText(text);
			_log.AppendText(Environment.NewLine);

			_statusLabel.ForeColor = color;
			_statusLabel.Text = text.NonEmptyLines().First();
		}

		private void RefreshBrowser()
		{
			Action refresh = () => _browser.Refresh(WebBrowserRefreshOption.Completely);
			_browser.BeginInvoke(refresh);
		}

		private void _menuItemAddDeclaringTypeToCriticalTypeList_Click(object sender, EventArgs e)
		{
			var signature = SelectedSignature();

			var criticalType = DeclaringTypeFromSignature(signature);
			AppendLineToFileWithComment(
				"Why should it be marked critical?",
				CriticalTypesFileForSelectedAssembly(),
				criticalType);
		}

		private void AppendLineToFileWithComment(string prompt, string file, string line)
		{
			using (var commentDialog = new CommentDialog())
			{
				commentDialog.Text = line;
				commentDialog.Prompt = prompt;
				if (commentDialog.ShowDialog() != DialogResult.OK)
					return;
				
				var comment = commentDialog.Comment;
				var formattedLine = comment.Trim().Length > 0
					? PrefixLineWithComment(line, comment)
					: line;

				AppendLineToFile(file, formattedLine);
			}
		}

		private void BrowseToClassLibrarySourceCode(string assembly, string signature)
		{
			if (assembly == "mscorlib") assembly = "corlib";
			var monoroot =new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.Parent.Parent.FullName;

			var signature_start = signature.Substring(0, signature.IndexOf(":"));

			var split = signature_start.Split(' ');
			var returntype = split[0];
			var method = split[1];
			var dotsplit = method.Split('.');
			var typename = dotsplit.Last();
			var ns = method.Substring(0, method.IndexOf(typename)-1);

			var sourcefile = DetectMethodPrivileges.Program.Paths.Combine(monoroot, "mcs","class",assembly, ns, typename+".cs");

			var methodname = signature.Substring(signature.IndexOf("::") + 2);
			methodname = methodname.Substring(0, methodname.IndexOf("("));
			
			int linenumber = 0;
			if (File.Exists(sourcefile))
			{
				var txt = File.ReadAllLines(sourcefile);
				var regex = new Regex(methodname + " ?\\(");
				for (int i = 0; i != txt.Length; i++)
					if (regex.Match(txt[i]).Success)
					{
						linenumber = i;
						break;
					}
				Info("Line: "+linenumber);
			}

			Info("Browsing to: " + assembly + "  with line: " + signature_start + " ns: " + ns + " typename: " + typename + " mcsclass: " + monoroot);
			Info("Sourcefile: " + sourcefile);
			Info("originalline: " + signature);
			Info("methodname: " + methodname);

			var psi = new ProcessStartInfo();
			psi.FileName = "c:\\program files (x86)\\notepad++\\notepad++.exe";
			psi.Arguments = sourcefile + " -n" + linenumber;
			var p = new Process();
			p.StartInfo = psi;
			p.Start();
		}

		private string PrefixLineWithComment(string line, string commentLines)
		{
			var comment = commentLines.SplitLines().Where(l=>l.Length>0).JoinLines("#");
			return "\n" + comment + line;
		}

		private string CriticalTypesFileForSelectedAssembly()
		{
			return DetectMethodPrivileges.Program.Files.CriticalTypesFileFor(SelectedSignatureAssembly(), _profileDirectory);
		}

		private void _menuItemAddToAuditedSafeList_Click(object sender, EventArgs e)
		{
			var selectedSignature = SelectedSignature();
			AppendLineToFileWithComment(
				"Why do you think it is safe?",
				AuditedSafeMethodsFileForSelectedAssembly(),
				selectedSignature);
		}

		private string AuditedSafeMethodsFileForSelectedAssembly()
		{
			return DetectMethodPrivileges.Program.Files.AuditedSafeMethodsFileFor(SelectedSignatureAssembly(), _profileDirectory);
		}

		private void AppendSelectedSignatureTo(string file, bool regexify)
		{
			var sig = SelectedSignature();
			if (regexify)
			{
				sig = sig.Replace("(", "\\(");
				sig = sig.Replace(")", "\\)");
				sig = sig.Replace("[", "\\[");
				sig = sig.Replace("]", "\\]");
			}
			AppendLineToFileWithComment(
				"Why?",
				file,
				sig);
		}

		private void _menuItemAddToKnownUnsafe_Click(object sender, EventArgs e)
		{
			AppendSelectedSignatureTo(
				DetectMethodPrivileges.Program.Files.ReviewedPublicApisFileFor(SelectedSignatureAssembly(), _profileDirectory), true);
		}

		

		private void AppendLineToFile(string file, string line)
		{
			using (var writer = File.AppendText(file))
				writer.WriteLine(line);
			Info("'{0}' appended to '{1}'", line.SplitLines().Last(), file);
		}

		string SelectedAssembly()
		{
			return (string)_assemblyList.SelectedItem;
		}
		
		private string SelectedSignatureAssembly()
		{
			var data = (string)_contextMenu.Tag;
			int space = data.IndexOf(" ");
			return data.Substring(0, space);
		}

		private string SelectedSignature()
		{
			var data = (string)_contextMenu.Tag;
			int space = data.IndexOf(" ");
		    return data.Substring(space+1);
		}

		private void browseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			BrowseToClassLibrarySourceCode(SelectedSignatureAssembly(), SelectedSignature());
		}
	}
}
