// System.Xml.Xsl.XslTransform
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2002 Tim Coleman
// (c) 2003 Ximian Inc. (http://www.ximian.com)
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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Xml.XPath;
using Mono.Xml.Xsl;

namespace System.Xml.Xsl {
	internal class SimpleXsltDebugger
	{
		public void OnCompile (XPathNavigator style)
		{
			Console.Write ("Compiling: ");
			PrintXPathNavigator (style);
			Console.WriteLine ();
		}

		public void OnExecute (XPathNodeIterator currentNodeSet, XPathNavigator style, XsltContext xsltContext)
		{
			Console.Write ("Executing: ");
			PrintXPathNavigator (style);
			Console.WriteLine (" / NodeSet: (type {1}) {0} / XsltContext: {2}", currentNodeSet, currentNodeSet.GetType (), xsltContext);
		}

		void PrintXPathNavigator (XPathNavigator nav)
		{
			IXmlLineInfo li = nav as IXmlLineInfo;
			li = li != null && li.HasLineInfo () ? li : null;
			Console.Write ("({0}, {1}) element {2}", li != null ? li.LineNumber : 0, li != null ? li.LinePosition : 0, nav.Name);
		}
	}

	[Obsolete]
	public sealed class XslTransform {

		internal static readonly bool TemplateStackFrameError;
		internal static readonly TextWriter TemplateStackFrameOutput;

		static XslTransform ()
		{
			string env = Environment.GetEnvironmentVariable ("MONO_XSLT_STACK_FRAME");
			switch (env) {
			case "stdout":
				TemplateStackFrameOutput = Console.Out;
				break;
			case "stderr":
				TemplateStackFrameOutput = Console.Error;
				break;
			case "error":
				TemplateStackFrameError = true;
				break;
			}
		}

		static object GetDefaultDebugger ()
		{
			string env = Environment.GetEnvironmentVariable ("MONO_XSLT_DEBUGGER");
			if (env == null)
				return null;
			if (env == "simple")
				return new SimpleXsltDebugger ();
			object obj = Activator.CreateInstance (Type.GetType (env));
			return obj;
		}

		public XslTransform ()
			: this (GetDefaultDebugger ())
		{
		}

		internal XslTransform (object debugger)
		{
			this.debugger = debugger;
		}

		object debugger;
		CompiledStylesheet s;
		XmlResolver xmlResolver = new XmlUrlResolver ();

		[MonoTODO] // FIXME: audit security check
		public XmlResolver XmlResolver {
			set {
				 xmlResolver = value;
			}
		}
		
		#region Transform
		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args)
		{
			return Transform (input.CreateNavigator (), args, xmlResolver);
		}

		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args, XmlResolver resolver)
		{
			return Transform (input.CreateNavigator (), args, resolver);
		}

		public XmlReader Transform (XPathNavigator input, XsltArgumentList args)
		{
			return Transform (input, args, xmlResolver);
		}

		public XmlReader Transform (XPathNavigator input, XsltArgumentList args, XmlResolver resolver)
		{
			// todo: is this right?
			MemoryStream stream = new MemoryStream ();
			Transform (input, args, new XmlTextWriter (stream, null), resolver);
			stream.Position = 0;
			return new XmlTextReader (stream, XmlNodeType.Element, null);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}
		
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output, XmlResolver resolver)
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}
		
		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}

		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}

		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output)
		{
			Transform (input, args, output, xmlResolver);
		}

		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			if (s == null)
				throw new XsltException ("No stylesheet was loaded.", null);

			Outputter outputter = new GenericOutputter (output, s.Outputs, null);
			new XslTransformProcessor (s, debugger).Process (input, outputter, args, resolver);
			output.Flush ();
		}

		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output)
		{
			Transform (input, args, output, xmlResolver);		
		}
		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
		{
			XslOutput xslOutput = (XslOutput)s.Outputs[String.Empty];
			Transform (input, args, new StreamWriter (output, xslOutput.Encoding), resolver);
		}

		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			Transform (input, args, output, xmlResolver);
		}

		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
		{
			if (s == null)
				throw new XsltException ("No stylesheet was loaded.", null);

			Outputter outputter = new GenericOutputter(output, s.Outputs, output.Encoding);			
			new XslTransformProcessor (s, debugger).Process (input, outputter, args, resolver);
			outputter.Done ();
			output.Flush ();
		}
		
		public void Transform (string inputfile, string outputfile)
		{ 
			Transform (inputfile, outputfile, xmlResolver);
		}

		public void Transform (string inputfile, string outputfile, XmlResolver resolver)
		{
			using (Stream s = new FileStream (outputfile, FileMode.Create, FileAccess.ReadWrite)) {
				Transform(new XPathDocument (inputfile).CreateNavigator (), null, s, resolver);
			}
		}
		#endregion

		#region Load
		public void Load (string url)
		{
			Load (url, null);
		}
		
		public void Load (string url, XmlResolver resolver)
		{
			XmlResolver res = resolver;
			if (res == null)
				res = new XmlUrlResolver ();
			Uri uri = res.ResolveUri (null, url);
			using (Stream s = res.GetEntity (uri, null, typeof (Stream)) as Stream) {
				XmlTextReader xtr = new XmlTextReader (uri.ToString (), s);
				xtr.XmlResolver = res;
				XmlValidatingReader xvr = new XmlValidatingReader (xtr);
				xvr.XmlResolver = res;
				xvr.ValidationType = ValidationType.None;
				Load (new XPathDocument (xvr, XmlSpace.Preserve).CreateNavigator (), resolver, null);
			}
		}

		public void Load (XmlReader stylesheet)
		{
			Load (stylesheet, null, null);
		}

		public void Load (XmlReader stylesheet, XmlResolver resolver)
		{
			Load (stylesheet, resolver, null);
		}

		public void Load (XPathNavigator stylesheet)
		{
			Load (stylesheet, null, null);
		}

		public void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			Load (stylesheet, resolver, null);
		}
		
		public void Load (IXPathNavigable stylesheet)
		{
			Load (stylesheet.CreateNavigator(), null);
		}

		public void Load (IXPathNavigable stylesheet, XmlResolver resolver)
		{
			Load (stylesheet.CreateNavigator(), resolver);
		}

		// Introduced in .NET 1.1
		public void Load (IXPathNavigable stylesheet, XmlResolver resolver, Evidence evidence)
		{
			Load (stylesheet.CreateNavigator(), resolver, evidence);
		}

		public void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence)
		{
			s = new Compiler (debugger).Compile (stylesheet, resolver, evidence);
		}

		public void Load (XmlReader stylesheet, XmlResolver resolver, Evidence evidence)
		{
			Load (new XPathDocument (stylesheet, XmlSpace.Preserve).CreateNavigator (), resolver, evidence);
		}
		#endregion
	}
}
