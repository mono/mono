//
// XsltCompiledContext.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	
// (C) 2003 Ben Maurer
//

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using Mono.Xml.Xsl.Functions;
using Mono.Xml.Xsl.Operations;
using System.Reflection;
using BF = System.Reflection.BindingFlags;

using QName = System.Xml.XmlQualifiedName;


namespace Mono.Xml.Xsl {

	internal class XsltCompiledContext : XsltContext {
		protected static Hashtable xsltFunctions = new Hashtable ();

		static XsltCompiledContext ()
		{
			xsltFunctions.Add ("current", new XsltCurrent ());
			xsltFunctions.Add ("document", new XsltDocument ());
			xsltFunctions.Add ("element-available", new XsltElementAvailable ());
			xsltFunctions.Add ("format-number", new XsltFormatNumber ());
			xsltFunctions.Add ("function-available", new XsltFunctionAvailable ());
			xsltFunctions.Add ("generate-id", new XsltGenerateId ());
			xsltFunctions.Add ("key", new XsltKey ());
			xsltFunctions.Add ("system-property", new XsltSystemProperty ());
			xsltFunctions.Add ("unparsed-entity-uri", new XsltUnparsedEntityUri ());
		}
			
		XslTransformProcessor p;
		VariableScope v;
		XPathNavigator doc;
			
		public XslTransformProcessor Processor { get { return p; }}
			
		public XsltCompiledContext (XslTransformProcessor p, VariableScope v, XPathNavigator doc)
		{
			this.p = p;
			this.v = v;
			this.doc = doc;
		}

		public override string DefaultNamespace { get { return String.Empty; }}


		public override string LookupNamespace (string prefix)
		{
			if (prefix == "" || prefix == null)
				return "";
			
			return this.doc.GetNamespace (prefix);
		}
		
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType[] argTypes)
		{
			IXsltContextFunction func = null;
			if (prefix == String.Empty || prefix == null) {
				return xsltFunctions [name] as IXsltContextFunction;
			} else {
				string ns = this.LookupNamespace (prefix);

				if (ns == null || p.Arguments == null) return null;

				object extension = p.Arguments.GetExtensionObject (ns);
					
				if (extension == null)
					return null;			
				
				MethodInfo method = FindBestMethod (extension.GetType (), name, argTypes);
				
				if (method != null) 
					return new XsltExtensionFunction (extension, method);
				return null;
				
			}
		}
		
		MethodInfo FindBestMethod (Type t, string name, XPathResultType [] argTypes)
		{
			int free, length;
			
			MethodInfo [] mi = t.GetMethods (BF.Public | BF.Instance | BF.Static);
			if (mi.Length == 0)
				return null;
			
			if (argTypes == null)
				return mi [0]; // if we dont have info on the arg types, nothing we can do


			free = 0;
			// filter on name + num args
			int numArgs = argTypes.Length;
			for (int i = 0; i < mi.Length; i ++) {
				if (mi [i].Name == name && mi [i].GetParameters ().Length == numArgs) 
					mi [free++] = mi [i];
			}
			length = free;
			
			// No method
			if (length == 0)
				return null;
			
			// Thats it!
			if (length == 1)
				return mi [0];
			
			free = 0;
			for (int i = 0; i < length; i ++) {
				bool match = true;
				ParameterInfo [] pi = mi [i].GetParameters ();
				
				for (int par = 0; par < pi.Length; par++) {
					XPathResultType required = argTypes [par];
					if (required == XPathResultType.Any)
						continue; // dunno what it is
					
					XPathResultType actual = XPFuncImpl.GetXPathType (pi [par].ParameterType);
					if (actual != required && actual != XPathResultType.Any) {
						match = false;
						break;
					}
					
					if (actual == XPathResultType.Any) {
						// try to get a stronger gind
						if (required != XPathResultType.NodeSet && !(pi [par].ParameterType == typeof (object)))
						{
							match = false;
							break;
						}
					}
				}
				if (match) return mi [i]; // TODO look for exact match
			}
			return null;
		}
			

		public override System.Xml.Xsl.IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			if (v != null) {
				XslGeneralVariable var = v.Resolve (p, new QName (name));
	
				if (var != null)
					return var;
			}
			return p.CompiledStyle.ResolveVariable (new QName (name));
		}

		public override int CompareDocument (string baseUri, string nextBaseUri) { throw new NotImplementedException (); }
		public override bool PreserveWhitespace (XPathNavigator nav) { throw new NotImplementedException (); }
		public override bool Whitespace { get { throw new NotImplementedException (); }}
	}


}
namespace Mono.Xml.Xsl.Functions {

	internal abstract class XPFuncImpl : IXsltContextFunction {
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
		
		public static XPathResultType GetXPathType (Type type) {
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
				throw new Exception ();
			default: // Numeric
				return XPathResultType.Number;
			} 
		}
	}
	
	class XsltExtensionFunction : XPFuncImpl {
		private object extension;
		private MethodInfo method;
		private TypeCode [] typeCodes;

		public XsltExtensionFunction (object extension, MethodInfo method)
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
				argTypes [i] = GetXPathType (parameters [i].ParameterType);
				if (canBeOpt) {
					if (parameters[i].IsOptional)
						minArgs --;
					else
						canBeOpt = false;
				}
			}
			base.Init (minArgs, maxArgs, GetXPathType (method.ReturnType), argTypes);
		}

		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			try {
				object result = method.Invoke(extension, args);
				IXPathNavigable navigable = result as IXPathNavigable;
				if (navigable != null)
					return navigable.CreateNavigator ();

				return result;
			} catch {
				Debug.WriteLine ("****** INCORRECT RESOLUTION **********");
				return "";
			}
		}
	}
		
	class XsltCurrent : XPFuncImpl {
		public XsltCurrent () : base (0, 0, XPathResultType.NodeSet, null) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			return new SelfIterator (xsltContext.Processor.CurrentNode, null);
		}
	}
	
	class XsltDocument : XPFuncImpl {
		public XsltDocument () : base (1, 2, XPathResultType.NodeSet, new XPathResultType [] { XPathResultType.Any, XPathResultType.NodeSet }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
	}
	
	class XsltElementAvailable : XPFuncImpl {
		public XsltElementAvailable () : base (1, 1, XPathResultType.Boolean, new XPathResultType [] { XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			QName name = XslNameUtil.FromString ((string)args [0], xsltContext);

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

	class XsltFormatNumber : XPFuncImpl {
		public XsltFormatNumber () : base (2, 3, XPathResultType.String , new XPathResultType [] { XPathResultType.Number, XPathResultType.String, XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
	}
	
	class XsltFunctionAvailable : XPFuncImpl {
		public XsltFunctionAvailable () : base (1, 1, XPathResultType.Boolean, new XPathResultType [] { XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			
			string name = (string)args [0];
			int colon = name.IndexOf (':');
			// extension function
			if (colon > 0)
				return xsltContext.ResolveFunction (
					name.Substring (0, colon), 
					name.Substring (colon, name.Length - colon), 
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
				xsltContext.ResolveFunction ("", name, null) != null // rest of xslt functions
			);
		}
	} 

	class XsltGenerateId : XPFuncImpl {
		public XsltGenerateId () : base (0, 1, XPathResultType.String , new XPathResultType [] { XPathResultType.NodeSet }) {}
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			XPathNavigator n;
			if (args.Length == 1) {
				XPathNodeIterator itr = (XPathNodeIterator) args [0];
				if (itr.MoveNext ())
					n = itr.Current.Clone ();
				else
					return string.Empty; // empty nodeset == empty string
			} else
				n = docContext.Clone ();
			
			StringBuilder sb = new StringBuilder ("Mono"); // Ensure begins with alpha
			sb.Append (XmlConvert.EncodeLocalName (n.BaseURI));
			sb.Replace ('_', 'm'); // remove underscores from EncodeLocalName
			
			do {
				sb.Append (IndexInParent (n));
				sb.Append ('m');
			} while (n.MoveToParent ());
			
			return sb.ToString ();
		}
		
		int IndexInParent (XPathNavigator nav)
		{
			nav = nav.Clone();
			
			int n = 0;
			while (nav.MoveToPrevious ())
				n++;
			
			return n;
		}
		
	} 
	
	class XsltKey : XPFuncImpl {
		public XsltKey () : base (2, 2, XPathResultType.NodeSet, new XPathResultType [] { XPathResultType.String, XPathResultType.Any }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			ArrayList result = new ArrayList ();
			QName name = XslNameUtil.FromString ((string)args [0], xsltContext);
			XPathNodeIterator it = args [1] as XPathNodeIterator;
			
			if (it != null) {
				while (it.MoveNext())
					FindKeyMatch (xsltContext, name, it.Current.Value, result, docContext);
			} else {
				FindKeyMatch (xsltContext, name, XPathFunctions.ToString (args [1]), result, docContext);
			}
			
			return new EnumeratorIterator (result.GetEnumerator (), xsltContext);
		}
		
		void FindKeyMatch (XsltCompiledContext xsltContext, QName name, string value, ArrayList result, XPathNavigator context)
		{
			XPathNavigator searchDoc = context.Clone ();
			searchDoc.MoveToRoot ();
			foreach (XslKey key in xsltContext.Processor.CompiledStyle.Keys) {
				if (key.Name == name) {
					XPathNodeIterator desc = searchDoc.SelectDescendants (XPathNodeType.All, true);

					while (desc.MoveNext ()) {
						if (key.Matches (desc.Current, value))
							AddResult (result, desc.Current);
						
						if (desc.Current.MoveToFirstAttribute ()) {
							do {
								if (key.Matches (desc.Current, value))
									AddResult (result, desc.Current);	
							} while (desc.Current.MoveToNext ());
							
							desc.Current.MoveToParent ();
						}
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
	
	class XsltSystemProperty : XPFuncImpl {
		public XsltSystemProperty () : base (1, 1, XPathResultType.String , new XPathResultType [] { XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			QName name = XslNameUtil.FromString ((string)args [0], xsltContext);
			
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

	class XsltUnparsedEntityUri : XPFuncImpl {
		public XsltUnparsedEntityUri () : base (1, 1, XPathResultType.String , new XPathResultType [] { XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
	}
}