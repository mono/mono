//
// XsltCompiledContext.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (atsushi@ximian.com)
// (C) 2003 Ben Maurer
// (C) 2004 Atsushi Enomoto
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
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl
{
	internal abstract class XPFuncImpl : IXsltContextFunction 
	{
		int minargs, maxargs;
		XPathResultType returnType;
		XPathResultType [] argTypes;

		public XPFuncImpl () {}
		public XPFuncImpl (int minArgs, int maxArgs, XPathResultType returnType, XPathResultType[] argTypes)
		{
			this.Init(minArgs, maxArgs, returnType, argTypes);
		}
		
		protected void Init (int minArgs, int maxArgs, XPathResultType returnType, XPathResultType[] argTypes)
		{
			this.minargs	= minArgs;
			this.maxargs	= maxArgs;
			this.returnType = returnType;
			this.argTypes	= argTypes;
		}

		public int Minargs { get { return this.minargs; }}
		public int Maxargs { get { return this.maxargs; }}
		public XPathResultType ReturnType { get { return this.returnType; }}
		public XPathResultType [] ArgTypes { get { return this.argTypes; }}
		public object Invoke (XsltContext xsltContext, object [] args, XPathNavigator docContext)
		{
			return Invoke ((XsltCompiledContext)xsltContext, args, docContext);
		}
		
		public abstract object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext);
		
		public static XPathResultType GetXPathType (Type type, XPathNavigator node) {
			switch (Type.GetTypeCode(type)) {
			case TypeCode.String:
				return XPathResultType.String;
			case TypeCode.Boolean:
				return XPathResultType.Boolean;
			case TypeCode.Object:
				if (typeof (XPathNavigator).IsAssignableFrom (type) || typeof (IXPathNavigable).IsAssignableFrom (type))
					return XPathResultType.Navigator;
				
				if (typeof (XPathNodeIterator).IsAssignableFrom (type))
					return XPathResultType.NodeSet;
				
				return XPathResultType.Any;
			case TypeCode.DateTime :
				throw new XsltException ("Invalid type DateTime was specified.", null, node);
			default: // Numeric
				return XPathResultType.Number;
			} 
		}
	}
	
	class XsltExtensionFunction : XPFuncImpl 
	{
		private object extension;
		private MethodInfo method;
		private TypeCode [] typeCodes;

		public XsltExtensionFunction (object extension, MethodInfo method, XPathNavigator currentNode)
		{
			this.extension = extension;
			this.method = method;

			ParameterInfo [] parameters = method.GetParameters ();
			int minArgs = parameters.Length;
			int maxArgs = parameters.Length;
			
			this.typeCodes = new TypeCode [parameters.Length];
			XPathResultType[] argTypes = new XPathResultType [parameters.Length];
			
			bool canBeOpt = true;
			for (int i = parameters.Length - 1; 0 <= i; i--) { // optionals at the end
				typeCodes [i] = Type.GetTypeCode (parameters [i].ParameterType);
				argTypes [i] = GetXPathType (parameters [i].ParameterType, currentNode);
				if (canBeOpt) {
					if (parameters[i].IsOptional)
						minArgs --;
					else
						canBeOpt = false;
				}
			}
			base.Init (minArgs, maxArgs, GetXPathType (method.ReturnType, currentNode), argTypes);
		}

		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			try {
				ParameterInfo [] pis = method.GetParameters ();
				object [] castedArgs = new object [pis.Length];
				for (int i = 0; i < args.Length; i++) {
					Type t = pis [i].ParameterType;
					switch (t.FullName) {
					case "System.Int16":
					case "System.UInt16":
					case "System.Int32":
					case "System.UInt32":
					case "System.Int64":
					case "System.UInt64":
					case "System.Single":
					case "System.Decimal":
						castedArgs [i] = Convert.ChangeType (args [i], t);
						break;
					default:
						castedArgs [i] = args [i];
						break;
					}
				}

				object result = null;
				switch (method.ReturnType.FullName) {
				case "System.Int16":
				case "System.UInt16":
				case "System.Int32":
				case "System.UInt32":
				case "System.Int64":
				case "System.UInt64":
				case "System.Single":
				case "System.Decimal":
					result = (double) method.Invoke (extension, castedArgs);
					break;
				default:
					result = method.Invoke(extension, castedArgs);
					break;
				}
				IXPathNavigable navigable = result as IXPathNavigable;
				if (navigable != null)
					return navigable.CreateNavigator ();

				return result;
			} catch (Exception ex) {
				throw new XsltException ("Custom function reported an error.", ex);
//				Debug.WriteLine ("****** INCORRECT RESOLUTION **********");
			}
		}
	}
	
	class XsltCurrent : XPathFunction 
	{
		public XsltCurrent (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("current takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}

		public override object Evaluate (BaseIterator iter)
		{
			return new SelfIterator ((iter.NamespaceManager as XsltCompiledContext).Processor.CurrentNode, null);
		}
	}
	
	class XsltDocument : XPathFunction 
	{
		Expression arg0, arg1;
		XPathNavigator doc;
		
		public XsltDocument (FunctionArguments args, Compiler c) : base (args)
		{
			if (args == null || (args.Tail != null && args.Tail.Tail != null))
				throw new XPathException ("document takes one or two args");
			
			arg0 = args.Arg;
			if (args.Tail != null)
				arg1 = args.Tail.Arg;
			doc = c.Input.Clone ();
		}
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			string baseUri = null;
			if (arg1 != null) {
				XPathNodeIterator it = arg1.EvaluateNodeSet (iter);
				if (it.MoveNext())
					baseUri = it.Current.BaseURI;
				else
					baseUri = VoidBaseUriFlag;
			}

			object o = arg0.Evaluate (iter);
			if (o is XPathNodeIterator)
				return GetDocument ((iter.NamespaceManager as XsltCompiledContext), (XPathNodeIterator)o, baseUri);
			else
				return GetDocument ((iter.NamespaceManager as XsltCompiledContext), o is IFormattable ? ((IFormattable) o).ToString (null, CultureInfo.InvariantCulture) : o.ToString (), baseUri);
		}
		
		static string VoidBaseUriFlag = "&^)(*&%*^$&$VOID!BASE!URI!";
		
		Uri Resolve (string thisUri, string baseUri, XslTransformProcessor p)
		{
//			Debug.WriteLine ("THIS: " + thisUri);
//			Debug.WriteLine ("BASE: " + baseUri);
			XmlResolver r = p.Resolver;
			if (r == null)
				return null;
			Uri uriBase = null;
			if (! object.ReferenceEquals (baseUri, VoidBaseUriFlag) && baseUri != String.Empty)
				uriBase = r.ResolveUri (null, baseUri);
				
			return r.ResolveUri (uriBase, thisUri);
		}
		
		XPathNodeIterator GetDocument (XsltCompiledContext xsltContext, XPathNodeIterator itr, string baseUri)
		{
			ArrayList list = new ArrayList ();
			Hashtable got = new Hashtable ();
			
			while (itr.MoveNext()) {
				Uri uri = Resolve (itr.Current.Value, baseUri != null ? baseUri : /*itr.Current.BaseURI*/doc.BaseURI, xsltContext.Processor);
				if (!got.ContainsKey (uri)) {
					got.Add (uri, null);
					if (uri != null && uri.ToString () == "") {
						XPathNavigator n = doc.Clone ();
						n.MoveToRoot ();
						list.Add (n);
					} else
						list.Add (xsltContext.Processor.GetDocument (uri));
				}
			}
			
			return new ListIterator (list, xsltContext, false);
		}
	
		XPathNodeIterator GetDocument (XsltCompiledContext xsltContext, string arg0, string baseUri)
		{
			Uri uri = Resolve (arg0, baseUri != null ? baseUri : doc.BaseURI, xsltContext.Processor);
			XPathNavigator n;
			if (uri != null && uri.ToString () == "") {
				n = doc.Clone ();
				n.MoveToRoot ();
			} else
				n = xsltContext.Processor.GetDocument (uri);
			
			return new SelfIterator (n, xsltContext);
		}
	}
	
	class XsltElementAvailable : XPathFunction 
	{
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltElementAvailable (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("element-available takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);

			return (
				(name.Namespace == Compiler.XsltNamespace) &&
				(
					//
					// A list of all the instructions (does not include top-level-elements)
					//
					name.Name == "apply-imports" ||
					name.Name == "apply-templates" ||
					name.Name == "call-template" ||
					name.Name == "choose" ||
					name.Name == "comment" ||
					name.Name == "copy" ||
					name.Name == "copy-of" ||
					name.Name == "element" ||
					name.Name == "fallback" ||
					name.Name == "for-each" ||
					name.Name == "message" ||
					name.Name == "number" ||
					name.Name == "processing-instruction" ||
					name.Name == "text" ||
					name.Name == "value-of" ||
					name.Name == "variable"
				)
			);
		}
	}

	class XsltFormatNumber : XPathFunction 
	{
		Expression arg0, arg1, arg2;
		XmlNamespaceManager nsm;
		
		public XsltFormatNumber (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail == null || (args.Tail.Tail != null && args.Tail.Tail.Tail != null))
				throw new XPathException ("format-number takes 2 or 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			if (args.Tail.Tail != null) {
				arg2= args.Tail.Tail.Arg;
				nsm = ctx.GetNsm ();
			}
		}
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			double d = arg0.EvaluateNumber (iter);
			string s = arg1.EvaluateString (iter);
			QName nm = QName.Empty;
			
			if (arg2 != null)
				nm = XslNameUtil.FromString (arg2.EvaluateString (iter), nsm);
			
			try {
				return (iter.NamespaceManager as XsltCompiledContext).Processor.CompiledStyle
				.LookupDecimalFormat (nm).FormatNumber (d, s);
			} catch (ArgumentException ex) {
				throw new XsltException (ex.Message, ex, iter.Current);
			}
		}
	}
	
	class XsltFunctionAvailable : XPathFunction 
	{
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltFunctionAvailable (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("element-available takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			
			string name = arg0.EvaluateString (iter);
			int colon = name.IndexOf (':');
			// extension function
			if (colon > 0)
				return (iter.NamespaceManager as XsltCompiledContext).ResolveFunction (
					XslNameUtil.FromString (name, nsm),
					null) != null;
			
			return (
				//
				// XPath
				//
                                name == "boolean" ||
                                name == "ceiling" ||
                                name == "concat" ||
                                name == "contains" ||
                                name == "count" ||
                                name == "false" ||
                                name == "floor" ||
                                name == "id"||
                                name == "lang" ||
                                name == "last" ||
                                name == "local-name" ||
                                name == "name" ||
                                name == "namespace-uri" ||
                                name == "normalize-space" ||
                                name == "not" ||
                                name == "number" ||
                                name == "position" ||
                                name == "round" ||
                                name == "starts-with" ||
                                name == "string" ||
                                name == "string-length" ||
                                name == "substring" ||
                                name == "substring-after" ||
                                name == "substring-before" ||
                                name == "sum" ||
                                name == "translate" ||
                                name == "true" ||
				// XSLT
				name == "document" ||
				name == "format-number" ||
				name == "function-available" ||
				name == "generate-id" ||
				name == "key" ||
				name == "current" ||
				name == "unparsed-entity-uri" ||
				name == "element-available" ||
				name == "system-property"
			);
		}
	} 

	class XsltGenerateId : XPathFunction 
	{
		Expression arg0;
		public XsltGenerateId (FunctionArguments args) : base (args)
		{
			if (args != null) {
				if (args.Tail != null)
					throw new XPathException ("generate-id takes 1 or no args");
				arg0 = args.Arg;
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			XPathNavigator n;
			if (arg0 != null) {
				XPathNodeIterator itr = arg0.EvaluateNodeSet (iter);
				if (itr.MoveNext ())
					n = itr.Current.Clone ();
				else
					return string.Empty; // empty nodeset == empty string
			} else
				n = iter.Current.Clone ();
			
			StringBuilder sb = new StringBuilder ("Mono"); // Ensure begins with alpha
			sb.Append (XmlConvert.EncodeLocalName (n.BaseURI));
			sb.Replace ('_', 'm'); // remove underscores from EncodeLocalName
			sb.Append (n.NodeType);
			sb.Append ('m');

			do {
				sb.Append (IndexInParent (n));
				sb.Append ('m');
			} while (n.MoveToParent ());
			
			return sb.ToString ();
		}
		
		int IndexInParent (XPathNavigator nav)
		{
			int n = 0;
			while (nav.MoveToPrevious ())
				n++;
			
			return n;
		}
	} 
	
	class XsltKey : XPathFunction 
	{
		Expression arg0, arg1;
		XmlNamespaceManager nsm;
		XslKey key;
		
		public XsltKey (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail == null)
				throw new XPathException ("key takes 2 args");
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			nsm = ctx.GetNsm ();
		}
		public Expression KeyName { get { return arg0; } }
		public Expression Field { get { return arg1; } }
		public XmlNamespaceManager NamespaceManager { get { return nsm; } }
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);
			XsltCompiledContext ctx = iter.NamespaceManager as XsltCompiledContext;
			if (key == null)
				key = ctx.Processor.CompiledStyle.Style.FindKey (name);

			ArrayList result = new ArrayList ();
			object o = arg1.Evaluate (iter);
			XPathNodeIterator it = o as XPathNodeIterator;
			
			if (it != null) {
				while (it.MoveNext())
					FindKeyMatch (ctx, it.Current.Value, result, iter.Current);
			} else {
				FindKeyMatch (ctx, XPathFunctions.ToString (o), result, iter.Current);
			}
			
			return new ListIterator (result, (iter.NamespaceManager as XsltCompiledContext), true);
		}
		
		void FindKeyMatch (XsltCompiledContext xsltContext, string value, ArrayList result, XPathNavigator context)
		{
			XPathNavigator searchDoc = context.Clone ();
			searchDoc.MoveToRoot ();
			if (key != null) {
				XPathNodeIterator desc = searchDoc.SelectDescendants (XPathNodeType.All, true);

				while (desc.MoveNext ()) {
					if (key.Matches (desc.Current, xsltContext, value))
						AddResult (result, desc.Current);
					
					if (desc.Current.MoveToFirstAttribute ()) {
						do {
							if (key.Matches (desc.Current, xsltContext, value))
								AddResult (result, desc.Current);	
						} while (desc.Current.MoveToNextAttribute ());
						
						desc.Current.MoveToParent ();
					}
				}
			}
		}

		void AddResult (ArrayList result, XPathNavigator nav)
		{
			for (int i = 0; i < result.Count; i++) {
				XmlNodeOrder docOrder = nav.ComparePosition (((XPathNavigator)result [i]));
				if (docOrder == XmlNodeOrder.Same)
					return;
				
				if (docOrder == XmlNodeOrder.Before) {
					result.Insert(i, nav.Clone ());
					return;
				}
			}
			result.Add (nav.Clone ());
		}
	}
	
	class XsltSystemProperty : XPathFunction 
	{
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltSystemProperty (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("system-property takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);
			
			if (name.Namespace == Compiler.XsltNamespace) {
				switch (name.Name) {
					case "version": return "1.0";
					case "vendor": return "Mono";
					case "vendor-url": return "http://www.go-mono.com/";
				}
			}
			
			return "";
		}
	} 

	class XsltUnparsedEntityUri : XPathFunction 
	{
		Expression arg0;
		
		public XsltUnparsedEntityUri (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("unparsed-entity-uri takes 1 arg");
			
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			IHasXmlNode xn = iter.Current as IHasXmlNode;
			if (xn == null)
				return String.Empty;
			XmlNode n = xn.GetNode ();
			XmlDocumentType doctype = n.OwnerDocument.DocumentType;
			if (doctype == null)
				return String.Empty;
			XmlEntity ent = doctype.Entities.GetNamedItem (arg0.EvaluateString (iter)) as XmlEntity;
			if (ent == null)
				return String.Empty;
			else
				return ent.BaseURI;
		}
	}

	class MSXslNodeSet : XPathFunction
	{
		Expression arg0;

		public MSXslNodeSet (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("element-available takes 1 arg");
			
			arg0 = args.Arg;
		}

		public override XPathResultType ReturnType {
			get {
				return XPathResultType.NodeSet;
			}
		}

		public override object Evaluate (BaseIterator iter)
		{
			XsltCompiledContext ctx = iter.NamespaceManager as XsltCompiledContext;
			XPathNavigator loc = iter.Current != null ? iter.Current.Clone () : null;
			XPathNavigator nav = arg0.EvaluateAs (iter, XPathResultType.Navigator) as XPathNavigator;
			if (nav == null) {
				if (loc != null)
					return new XsltException ("Cannot convert the XPath argument to a result tree fragment.", null, loc);
				else
					return new XsltException ("Cannot convert the XPath argument to a result tree fragment.", null);
			}
			ArrayList al = new ArrayList ();
			al.Add (nav);
			return new ListIterator (al, ctx, false);
		}
	}
}
