//
// XsltDebuggerWrapper.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	
// Copyright (C) 2007 Novell, Inc.
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
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.Xsl 
{
	internal class XsltDebuggerWrapper
	{
		readonly MethodInfo on_compile, on_execute;
		readonly object impl;

		public XsltDebuggerWrapper (object impl)
		{
			this.impl = impl;
			on_compile = impl.GetType ().GetMethod ("OnCompile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (on_compile == null)
				throw new InvalidOperationException ("INTERNAL ERROR: the debugger does not look like what System.Xml.dll expects. OnCompile method was not found");
			on_execute = impl.GetType ().GetMethod ("OnExecute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (on_execute == null)
				throw new InvalidOperationException ("INTERNAL ERROR: the debugger does not look like what System.Xml.dll expects. OnExecute method was not found");
		}

		public void DebugCompile (XPathNavigator style)
		{
			on_compile.Invoke (impl, new object [] {style.Clone ()});
		}

		public void DebugExecute (XslTransformProcessor p, XPathNavigator style)
		{
			on_execute.Invoke (impl, new object [] {p.CurrentNodeset.Clone (), style.Clone (), p.XPathContext});
		}
	}
}
