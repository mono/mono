//
// MSXslScriptManager.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C)2003 Novell inc.
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl
{
	internal abstract class ScriptCompilerInfo
	{
		string compilerCommand;
		string defaultCompilerOptions;

		public virtual string CompilerCommand {
			get { return compilerCommand; }
			set { compilerCommand = value; }
		}

		public virtual string DefaultCompilerOptions {
			get { return defaultCompilerOptions; }
			set { defaultCompilerOptions = value; }
		}

		public abstract string Extension { get; }

		public abstract string SourceTemplate { get; }

		public virtual string GetCompilerArguments (string targetFileName)
		{
			return String.Concat (DefaultCompilerOptions, " ", targetFileName);
		}

		[MonoTODO ("Should use Assembly.LoadFile() instead of LoadFrom() after its implementation has finished.")]
		public virtual Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			string tmpPath = Path.GetTempPath ();
			if (!tmpPath.EndsWith (Path.DirectorySeparatorChar.ToString ()))
				tmpPath += Path.DirectorySeparatorChar;
			string tmpbase = tmpPath + Guid.NewGuid ();
			ProcessStartInfo psi = new ProcessStartInfo ();
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			Process proc = new Process ();
			proc.StartInfo = psi;
			StreamWriter sw = null;
			try {
				SecurityManager.ResolvePolicy (evidence).Demand ();
				sw = File.CreateText (tmpbase + Extension);
				sw.WriteLine (SourceTemplate.Replace ("{0}", DateTime.Now.ToString ()).Replace ("{1}", classSuffix).Replace ("{2}", code));

				sw.Close ();
				psi.FileName = CompilerCommand;
				psi.Arguments = String.Concat (GetCompilerArguments (tmpbase + Extension));
				psi.WorkingDirectory = tmpPath;
				proc.Start ();
//				Console.WriteLine (proc.StandardOutput.ReadToEnd ());
//				Console.WriteLine (proc.StandardError.ReadToEnd ());
				proc.WaitForExit (); // FIXME: should we configure timeout?
				Assembly generated = Assembly.LoadFrom (tmpbase + ".dll");

				if (generated == null)
					throw new XsltCompileException ("Could not load script assembly", null, scriptNode);
				return generated.GetType ("GeneratedAssembly.Script" + classSuffix);
			} catch (Exception ex) {
				throw new XsltCompileException ("Script compilation error: " + ex.Message, ex, scriptNode);
			} finally {
				try {
					File.Delete (tmpbase + Extension);
					File.Delete (tmpbase + ".dll");
					if (sw != null)
						sw.Close ();
				} catch (Exception) {
				}
			}
		}
	}

	internal class CSharpCompilerInfo : ScriptCompilerInfo
	{
		public CSharpCompilerInfo ()
		{
			this.CompilerCommand = "mcs";
#if MS_NET
			this.CompilerCommand = "csc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library /r:Microsoft.VisualBasic.dll";
		}

		public override string Extension {
			get { return ".cs"; }
		}

		public override string SourceTemplate {
			get {
				return @"// This file is automatically created by Mono managed XSLT engine.
// Created time: {0}
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.VisualBasic;

namespace GeneratedAssembly
{
public class Script{1}
{
	{2}
}
}";
			}
		}
	}

	internal class VBCompilerInfo : ScriptCompilerInfo
	{
		public VBCompilerInfo ()
		{
			this.CompilerCommand = "mbas";
#if MS_NET
			this.CompilerCommand = "vbc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library  /r:System.dll /r:System.XML.dll /r:Microsoft.VisualBasic.dll";
		}

		public override string Extension {
			get { return ".vb"; }
		}

		public override string SourceTemplate {
			get {
				return @"' This file is automatically created by Mono managed XSLT engine.
' Created time: {0}
imports System
imports System.Collections
imports System.Text
imports System.Text.RegularExpressions
imports System.Xml
imports System.Xml.XPath
imports System.Xml.Xsl
imports Microsoft.VisualBasic

namespace GeneratedAssembly
public Class Script{1}
	{2}
end Class
end namespace
";
			}
		}
	}

	internal class JScriptCompilerInfo : ScriptCompilerInfo
	{
		public JScriptCompilerInfo ()
		{
			this.CompilerCommand = "dummy-jscript-compiler.exe";
#if MS_NET
			this.CompilerCommand = "jsc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library /r:Microsoft.VisualBasic.dll";
		}

		public override string Extension {
			get { return ".js"; }
		}

		public override string SourceTemplate {
			get {
				return @"// This file is automatically created by Mono managed XSLT engine.
// Created time: {0}
import System;
import System.Collections;
import System.Text;
import System.Text.RegularExpressions;
import System.Xml;
import System.Xml.XPath;
import System.Xml.Xsl;
import Microsoft.VisualBasic;

package GeneratedAssembly
{
class Script{1} {
	{2}
}
}
";
			}
		}

#if !MS_NET
		public override Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			SecurityManager.ResolvePolicy (evidence).Demand ();
			Assembly jsasm = Assembly.LoadWithPartialName ("Microsoft.JScript.dll", evidence);
			Type providerType = jsasm.GetType ("Microsoft.JScript.JScriptCodeProvider");
			CodeDomProvider provider = (CodeDomProvider) Activator.CreateInstance (providerType);

			ICodeCompiler compiler = provider.CreateCompiler ();
			CompilerParameters parameters = new CompilerParameters ();
			parameters.CompilerOptions = DefaultCompilerOptions;
			CompilerResults res = compiler.CompileAssemblyFromSource (parameters, SourceTemplate.Replace ("{0}", DateTime.Now.ToString ()).Replace ("{1}", classSuffix).Replace ("{2}", code));
			if (res.CompiledAssembly == null)
				throw new XsltCompileException ("Cannot compile stylesheet script", null, scriptNode);
			return res.CompiledAssembly.GetType ("GeneratedAssembly.Script" + classSuffix);
		}
#endif
	}
}

