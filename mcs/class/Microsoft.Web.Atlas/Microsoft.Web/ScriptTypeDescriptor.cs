//
// Microsoft.Web.ScriptTypeDescriptor
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections.Generic;

namespace Microsoft.Web
{
	public sealed class ScriptTypeDescriptor
	{
		IScriptObject scriptObject;
		bool closed;
		List<ScriptEventDescriptor> events;
		List<ScriptMethodDescriptor> methods;
		List<ScriptPropertyDescriptor> props;

		public ScriptTypeDescriptor (IScriptObject scriptObject)
		{
			if (scriptObject == null)
				throw new ArgumentNullException ("scriptObject");

			this.scriptObject = scriptObject;
			this.events = new List<ScriptEventDescriptor>();
			this.methods = new List<ScriptMethodDescriptor>();
			this.props = new List<ScriptPropertyDescriptor>();
		}

		public IScriptObject ScriptObject {
			get {
				return scriptObject;
			}
		}

		public void AddEvent (ScriptEventDescriptor eventDescriptor)
		{
			if (closed)
				throw new InvalidOperationException ("Items cannot be added to a type descriptor that has been closed.");
			events.Add (eventDescriptor);
		}

		public void AddMethod (ScriptMethodDescriptor methodDescriptor)
		{
			if (closed)
				throw new InvalidOperationException ("Items cannot be added to a type descriptor that has been closed.");
			methods.Add (methodDescriptor);
		}

		public void AddProperty (ScriptPropertyDescriptor propertyDescriptor)
		{
			if (closed)
				throw new InvalidOperationException ("Items cannot be added to a type descriptor that has been closed.");
			props.Add (propertyDescriptor);
		}

		public void Close ()
		{
			closed = true;
		}

		public IEnumerable<ScriptEventDescriptor> GetEvents ()
		{
			return events;
		}

		public IEnumerable<ScriptMethodDescriptor> GetMethods()
		{
			return methods;
		}

		public IEnumerable<ScriptPropertyDescriptor> GetProperties ()
		{
			return props;
		}
	}
}

#endif
