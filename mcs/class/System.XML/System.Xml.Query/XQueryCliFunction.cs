//
// XQueryCliFunction.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

//
// Runtime type method wrapper for XPath2 function.
//
#if NET_2_0
using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Query;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.XPath2
{

	// Ideas:
	// declare function namespace cli = "http://mono-project.com/xquery/function/cli"
	// declare variable v = cli:invoke (cli:new (Microsoft.CSharp:CSharpCodeProvider), CreateCompiler);
	// declare variable v2 = System:Math.Abs (0.5);
	//

	public class XQueryCliFunction : XQueryFunction
	{
		internal static XQueryCliFunction CreateFromMethodInfo (MethodInfo mi)
		{
			return CreateFromMethodInfo (null, mi);
		}

		internal static XQueryCliFunction CreateFromMethodInfo (XmlQualifiedName name, MethodInfo mi)
		{
			return CreateFromMethodInfo (name, new MethodInfo [] {mi});
		}

		internal static XQueryCliFunction CreateFromMethodInfo (MethodInfo [] methods)
		{
			return CreateFromMethodInfo (null, methods);
		}

		internal static XQueryCliFunction CreateFromMethodInfo (XmlQualifiedName name, MethodInfo [] methodList)
		{
			if (methodList == null || methodList.Length == 0)
				throw new ArgumentException (String.Format ("Argument methods is missing or zero-length array. Name is {0}", name));

			Type cliReturnType = null;
			ArrayList arguments = new ArrayList ();

			if (name == null || name == XmlQualifiedName.Empty)
				name = new XmlQualifiedName (methodList [0].Name, methodList [0].DeclaringType.FullName);

			int maxArgs = 0;
			int minArgs = -1;
			Hashtable methods = new Hashtable ();

			foreach (MethodInfo mi in methodList) {
				if (cliReturnType == null)
					cliReturnType = mi.ReturnType;
				else if (mi.ReturnType != cliReturnType)
					throw new ArgumentException (String.Format ("Argument methodList have different return types. Method name is {0}.", mi.Name));
				ParameterInfo [] prms = mi.GetParameters ();

				int args = prms.Length;

				// Whether it takes "current context" or not.
				bool hasContextArg = (args > 0 && prms [0].ParameterType == typeof (XQueryContext));
				if (hasContextArg)
					args--;
				if (methods [args] != null)
					throw new ArgumentException (String.Format ("XQuery does not allow functions that accepts such methods that have the same number of parameters in different types. Method name is {0}", mi.Name));
				methods.Add (args, mi);
				if (args < minArgs || minArgs < 0)
					minArgs = args;
				if (args > maxArgs)
					maxArgs = args;
			}

			MethodInfo m = (MethodInfo) methods [maxArgs];
			if (m == null)
				throw new NotImplementedException ("Should not happen: maxArgs is " + maxArgs);
			ParameterInfo [] pl = m.GetParameters ();
			for (int i = 0; i < pl.Length; i++)
				if (pl [i].ParameterType != typeof (XQueryContext))
					arguments.Add (
						new XQueryFunctionArgument (new XmlQualifiedName (pl [i].Name), SequenceType.Create (pl [i].ParameterType)));

			return new XQueryCliFunction (name,
				arguments.ToArray (typeof (XQueryFunctionArgument)) as XQueryFunctionArgument [],
				SequenceType.Create (cliReturnType),
				methods,
				minArgs,
				maxArgs);
		}

		private XQueryCliFunction (XmlQualifiedName name, 
			XQueryFunctionArgument [] args,
			SequenceType returnType,
			Hashtable methods,
			int minArgs,
			int maxArgs)
			: base (name, args, returnType)
		{
			this.methods = methods;
			this.maxArgs = maxArgs;
			this.minArgs = minArgs;
		}

		// instance members

		// [int argsize] -> MethodInfo (according to the spec 1.1,
		// there should be no overloads that accepts the same parameter
		// count in different types).
		Hashtable methods = new Hashtable ();
		int maxArgs = 0;
		int minArgs = -1;
		SequenceType returnType;

		public override int MinArgs {
			get { return minArgs; }
		}

		public override int MaxArgs {
			get { return maxArgs; }
		}

		public override object Invoke (XQueryContext context, object [] args, XPathItem currentItem)
		{
			MethodInfo mi = methods [args.Length] as MethodInfo;
			if (mi == null)
				throw new ArgumentException ("The number of custom function parameter does not match with the registered method's signature.");
			ParameterInfo [] prms = mi.GetParameters ();
			if (prms.Length > 0 && prms [0].ParameterType == typeof (XQueryContext)) {
				ArrayList pl = new ArrayList (args);
				pl.Insert (0, context);
				return mi.Invoke (null, pl.ToArray ());
			}
			else
				return mi.Invoke (null, args);
		}
	}

}
#endif
