//
// MSXslScriptManager.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C)2003 Novell inc.
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
using Microsoft.CSharp;
using Microsoft.VisualBasic;

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

		public abstract CodeDomProvider CodeDomProvider { get; }

		public abstract string Extension { get; }

		public abstract string SourceTemplate { get; }

		public virtual string GetCompilerArguments (string targetFileName)
		{
			return String.Concat (DefaultCompilerOptions, " ", targetFileName);
		}


		public virtual Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			PermissionSet ps = SecurityManager.ResolvePolicy (evidence);
			if (ps != null)
				ps.Demand ();

			ICodeCompiler compiler = CodeDomProvider.CreateCompiler ();
			CompilerParameters parameters = new CompilerParameters ();
			parameters.CompilerOptions = DefaultCompilerOptions;

			// get source filename
			string filename = String.Empty;
			try {
				if (scriptNode.BaseURI != String.Empty)
					filename = new Uri (scriptNode.BaseURI).LocalPath;
			} catch (FormatException) {
			}
			if (filename == String.Empty)
				filename = "__baseURI_not_supplied__";

			// get source location
			IXmlLineInfo li = scriptNode as IXmlLineInfo;
			string lineInfoLine = 
				li != null && li.LineNumber > 0 ?
				String.Format (CultureInfo.InvariantCulture, "\n#line {0} \"{1}\"", li.LineNumber, filename) :
				String.Empty;

			string source = SourceTemplate.Replace ("{0}", DateTime.Now.ToString ()).Replace ("{1}", classSuffix).Replace ("{2}", lineInfoLine + code);

			CompilerResults res = compiler.CompileAssemblyFromSource (parameters, source);
			if (res.Errors.Count != 0)
				throw new XsltCompileException ("Stylesheet script compile error: \n" + FormatErrorMessage (res) /*+ "Code :\n" + source*/, null, scriptNode);
			if (res.CompiledAssembly == null)
				throw new XsltCompileException ("Cannot compile stylesheet script", null, scriptNode);
			return res.CompiledAssembly.GetType ("GeneratedAssembly.Script" + classSuffix);
		}

		private string FormatErrorMessage (CompilerResults res)
		{
			string s = String.Empty;
			foreach (CompilerError e in res.Errors) {
				object [] parameters = new object [] {"\n",
					e.FileName,
					e.Line > 0 ? " line " + e.Line : String.Empty,
					e.IsWarning ? " WARNING: " : " ERROR: ",
					e.ErrorNumber,
					": ",
					e.ErrorText};
				s += String.Concat (parameters);
			}
			return s;
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
			this.DefaultCompilerOptions = "/t:library /r:System.dll /r:System.Xml.dll /r:Microsoft.VisualBasic.dll";
		}

		public override CodeDomProvider CodeDomProvider {
			get { return new CSharpCodeProvider (); }
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

		public override CodeDomProvider CodeDomProvider {
			get { return new VBCodeProvider (); }
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
		static Type providerType;

		public JScriptCompilerInfo ()
		{
			this.CompilerCommand = "mjs";
#if MS_NET
			this.CompilerCommand = "jsc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library /r:Microsoft.VisualBasic.dll";
		}

		public override CodeDomProvider CodeDomProvider {
			get {
				// no need for locking
				if (providerType == null) {
					Assembly jsasm = Assembly.LoadWithPartialName ("Microsoft.JScript", null);
					if (jsasm != null)
						providerType = jsasm.GetType ("Microsoft.JScript.JScriptCodeProvider");
				}
				return (CodeDomProvider) Activator.CreateInstance (providerType); 
			}
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
	}
}

