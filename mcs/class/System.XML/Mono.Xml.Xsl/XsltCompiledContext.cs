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
			
		public XslTransformProcessor Processor { get { return p; }}
			
		public XsltCompiledContext (XslTransformProcessor p, VariableScope v)
		{
			this.p = p;
			this.v = v;
		}

		public override string DefaultNamespace { get { return String.Empty; }}


		public override string LookupNamespace (string prefix)
		{
			if (prefix == "" || prefix == null)
				return "";
			
			return p.CompiledStyle.NamespaceManager.LookupNamespace (prefix);
		}
		
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType[] argTypes)
		{
			IXsltContextFunction func = null;
			if (prefix == String.Empty || prefix == null) {
			//	return xsltFunctions [name] as IXsltContextFunction;
				return null;
			}
			else {
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


			free = 0;
			// filter on name + num args
			int numArgs = argTypes == null ? 0 : argTypes.Length;
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
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
	} 

	class XsltGenerateId : XPFuncImpl {
		public XsltGenerateId () : base (0, 1, XPathResultType.String , new XPathResultType [] { XPathResultType.NodeSet }) {}
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
	} 
	
	class XsltKey : XPFuncImpl {
		public XsltKey () : base (2, 2, XPathResultType.NodeSet, new XPathResultType [] { XPathResultType.String, XPathResultType.Any }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
	}
	
	class XsltSystemProperty : XPFuncImpl {
		public XsltSystemProperty () : base (1, 1, XPathResultType.String , new XPathResultType [] { XPathResultType.String }) {}
		
		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
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