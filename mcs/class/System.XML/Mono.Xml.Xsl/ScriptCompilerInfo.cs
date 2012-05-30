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

		public abstract string FormatSource (IXmlLineInfo li, string file, string code);

		public virtual string GetCompilerArguments (string targetFileName)
		{
			return String.Concat (DefaultCompilerOptions, " ", targetFileName);
		}


		public virtual Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			PermissionSet ps = SecurityManager.ResolvePolicy (evidence);
			if (ps != null)
				ps.Demand ();

			// The attempt to use an already pre-compiled
			// class assumes the caller has computed the
			// classSuffix as a hash of the code
			// string. MSXslScriptManager.cs does that.
			// The mechanism how exactly such pre-compiled
			// classes should be produced are not
			// specified here.
			string scriptname = "Script" + classSuffix;
			string typename = "GeneratedAssembly." + scriptname;
			try {
				Type retval = Type.GetType (typename);
				if (retval != null)
					return retval;
			} catch {
			}

			try {
				Type retval =  Assembly.LoadFrom (scriptname + ".dll").GetType (typename);
				if (retval != null)
					return retval;
			} catch {
			}

			// OK, we have to actually compile the script.
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

			string source = SourceTemplate.Replace ("{0}",
				DateTime.Now.ToString (CultureInfo.InvariantCulture))
				.Replace ("{1}", classSuffix)
				.Replace ("{2}", code);
			source = FormatSource (li, filename, source);

			CompilerResults res = compiler.CompileAssemblyFromSource (parameters, source);
			foreach (CompilerError err in res.Errors)
				if (!err.IsWarning)
					// Actually it should be
					// XsltCompileException, but to match 
					// with silly MS implementation...
//					throw new XsltCompileException ("Stylesheet script compile error: \n" + FormatErrorMessage (res) /*+ "Code :\n" + source*/, null, scriptNode);
					throw new XsltException ("Stylesheet script compile error: \n" + FormatErrorMessage (res) /*+ "Code :\n" + source*/, null, scriptNode);

			if (res.CompiledAssembly == null)
				throw new XsltCompileException ("Cannot compile stylesheet script", null, scriptNode);
			return res.CompiledAssembly.GetType (typename);
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
			this.DefaultCompilerOptions = "/t:library /r:System.dll /r:System.Xml.dll";
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

		public override string FormatSource (IXmlLineInfo li, string file, string source)
		{
			if (li == null)
				return source;
			return String.Format (CultureInfo.InvariantCulture, "#line {0} \"{1}\"\n{2}", li.LineNumber, file, source);
		}
	}

	internal class VBCompilerInfo : ScriptCompilerInfo
	{
		public VBCompilerInfo ()
		{
			this.CompilerCommand = "mbas";
			this.DefaultCompilerOptions = "/t:library";
#if MS_NET
			this.CompilerCommand = "vbc.exe";
			this.DefaultCompilerOptions = "/t:library  /r:System.dll /r:System.Xml.dll /r:Microsoft.VisualBasic.dll";
#endif
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

		public override string FormatSource (IXmlLineInfo li, string file, string source)
		{
			if (li == null)
				return source;
			return String.Format (CultureInfo.InvariantCulture,
				"#ExternalSource (\"{1}\", {0})\n{2}\n#end ExternalSource",
				li.LineNumber, new FileInfo (file).Name, source);
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
			this.DefaultCompilerOptions = "/t:library";
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

		public override string FormatSource (IXmlLineInfo li, string file, string source)
		{
#if true // remove when mjs got @set @position support
			return source;
#else
			if (li == null)
				return source;
			return String.Format (CultureInfo.InvariantCulture,
				"@set @position ({0}{1}{2}line={3};column={4})\n{5}",
				file != null ? "file=" : String.Empty,
				file,
				file != null ? "; " : String.Empty,
				li.LineNumber,
				li.LinePosition,
				source);
#endif
		}
	}
}

