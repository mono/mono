// XslCompiledTransform.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell Inc. http://www.novell.com
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
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Xml.XPath;
using Mono.Xml.Xsl;

namespace System.Xml.Xsl
{
	[MonoTODO]
	// FIXME: Of cource this is just a stub for now.
	public sealed class XslCompiledTransform
	{
		bool enable_debug;
		object debugger;
		CompiledStylesheet s;
#if !TARGET_JVM && !MOBILE
//		TempFileCollection temporary_files;
#endif
		XmlWriterSettings output_settings = new XmlWriterSettings ();

		public XslCompiledTransform ()
			: this (false)
		{
		}

		public XslCompiledTransform (bool enableDebug)
		{
			enable_debug = enableDebug;
			if (enable_debug)
				debugger = new NoOperationDebugger ();
			output_settings.ConformanceLevel = ConformanceLevel.Fragment;
		}

		[MonoTODO]
		public XmlWriterSettings OutputSettings {
			get { return output_settings; }
		}

#if !TARGET_JVM && !MOBILE
		[MonoTODO]
		public TempFileCollection TemporaryFiles {
			get { return null; /*temporary_files;*/ }
		}
#endif

		#region Transform

		public void Transform (string inputUri, string resultsFile)
		{
			using (Stream outStream = File.Create (resultsFile)) {
				Transform (new XPathDocument (inputUri, XmlSpace.Preserve), null, outStream);
			}
		}

		public void Transform (string inputUri, XmlWriter results)
		{
			Transform (inputUri, null, results);
		}

		public void Transform (string inputUri, XsltArgumentList arguments, Stream results)
		{
			Transform (new XPathDocument (inputUri, XmlSpace.Preserve), arguments, results);
		}

		public void Transform (string inputUri, XsltArgumentList arguments, TextWriter results)
		{
			Transform (new XPathDocument (inputUri, XmlSpace.Preserve), arguments, results);
		}

		public void Transform (string inputUri, XsltArgumentList arguments, XmlWriter results)
		{
			Transform (new XPathDocument (inputUri, XmlSpace.Preserve), arguments, results);
		}

		public void Transform (XmlReader input, XmlWriter results)
		{
			Transform (input, null, results);
		}

		public void Transform (XmlReader input, XsltArgumentList arguments, Stream results)
		{
			Transform (new XPathDocument (input, XmlSpace.Preserve), arguments, results);
		}

		public void Transform (XmlReader input, XsltArgumentList arguments, TextWriter results)
		{
			Transform (new XPathDocument (input, XmlSpace.Preserve), arguments, results);
		}

		public void Transform (XmlReader input, XsltArgumentList arguments, XmlWriter results)
		{
			Transform (input, arguments, results, null);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList arguments, TextWriter results)
		{
			Transform (input.CreateNavigator (), arguments, results);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList arguments, Stream results)
		{
			Transform (input.CreateNavigator (), arguments, results);
		}

		public void Transform (IXPathNavigable input, XmlWriter results)
		{
			Transform (input, null, results);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList arguments, XmlWriter results)
		{
			Transform (input.CreateNavigator (), arguments, results, null);
		}

		public void Transform (XmlReader input, XsltArgumentList arguments, XmlWriter results, XmlResolver documentResolver)
		{
			Transform (new XPathDocument (input, XmlSpace.Preserve).CreateNavigator (), arguments, results, documentResolver);
		}

		void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			if (s == null)
				throw new XsltException ("No stylesheet was loaded.", null);

			Outputter outputter = new GenericOutputter (output, s.Outputs, null);
			new XslTransformProcessor (s, debugger).Process (input, outputter, args, resolver);
			output.Flush ();
		}

		void Transform (XPathNavigator input, XsltArgumentList args, Stream output)
		{
			XslOutput xslOutput = (XslOutput)s.Outputs[String.Empty];
			Transform (input, args, new StreamWriter (output, xslOutput.Encoding));
		}

		void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			if (s == null)
				throw new XsltException ("No stylesheet was loaded.", null);

			Outputter outputter = new GenericOutputter(output, s.Outputs, output.Encoding);
			new XslTransformProcessor (s, debugger).Process (input, outputter, args, null);
			outputter.Done ();
			output.Flush ();
		}

		#endregion

		#region Load

		private XmlReader GetXmlReader (string url)
		{
			XmlResolver res = new XmlUrlResolver ();
			Uri uri = res.ResolveUri (null, url);
			Stream s = res.GetEntity (uri, null, typeof (Stream)) as Stream;
			XmlTextReader xtr = new XmlTextReader (uri.ToString (), s);
			xtr.XmlResolver = res;
			XmlValidatingReader xvr = new XmlValidatingReader (xtr);
			xvr.XmlResolver = res;
			xvr.ValidationType = ValidationType.None;
			return xvr;
		}

		public void Load (string stylesheetUri)
		{
			using (XmlReader r = GetXmlReader (stylesheetUri)) {
				Load (r);
			}
		}

		public void Load (XmlReader stylesheet)
		{
			Load (stylesheet, null, null);
		}

		public void Load (IXPathNavigable stylesheet)
		{
			Load (stylesheet.CreateNavigator(), null, null);
		}

		public void Load (IXPathNavigable stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
		{
			Load (stylesheet.CreateNavigator(), settings, stylesheetResolver);
		}

		public void Load (XmlReader stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
		{
			Load (new XPathDocument (stylesheet, XmlSpace.Preserve).CreateNavigator (), settings, stylesheetResolver);
		}

		public void Load (string stylesheetUri, XsltSettings settings, XmlResolver stylesheetResolver)
		{
			Load (new XPathDocument (stylesheetUri, XmlSpace.Preserve).CreateNavigator (), settings, stylesheetResolver);
		}

		private void Load (XPathNavigator stylesheet,
			XsltSettings settings, XmlResolver stylesheetResolver)
		{
			s = new Compiler (debugger).Compile (stylesheet, stylesheetResolver, null);
		}

		#endregion
	}

		class NoOperationDebugger
		{
			protected void OnCompile (XPathNavigator input)
			{
			}

			protected void OnExecute (XPathNodeIterator currentNodeset, XPathNavigator style, XsltContext context)
			{
				//ShowLocationInTrace (style);
			}
/*
			string ShowLocationInTrace (XPathNavigator style)
			{
				IXmlLineInfo li = style as IXmlLineInfo;
				return li != null ? String.Format ("at {0} ({1},{2})", style.BaseURI, li.LineNumber, li.LinePosition) : "(no debug info available)";
			}
*/
		}
}
