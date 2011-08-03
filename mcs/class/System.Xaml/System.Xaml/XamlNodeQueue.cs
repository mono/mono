//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Markup;

namespace System.Xaml
{
	public class XamlNodeQueue
	{
		Queue<XamlNodeLineInfo> queue = new Queue<XamlNodeLineInfo> ();
		XamlSchemaContext ctx;
		XamlReader reader;
		XamlWriter writer;

		public XamlNodeQueue (XamlSchemaContext schemaContext)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			this.ctx = schemaContext;
			reader = new XamlNodeQueueReader (this);
			writer = new XamlNodeQueueWriter (this);
		}
		
		internal IXamlLineInfo LineInfoProvider { get; set; }

		internal XamlSchemaContext SchemaContext {
			get { return ctx; }
		}

		public int Count {
			get { return queue.Count; }
		}

		public bool IsEmpty {
			get { return queue.Count == 0; }
		}

		public XamlReader Reader {
			get { return reader; }
		}

		public XamlWriter Writer {
			get { return writer; }
		}

		internal XamlNodeLineInfo Dequeue ()
		{
			return queue.Dequeue ();
		}

		internal void Enqueue (XamlNodeInfo info)
		{
			var nli = (LineInfoProvider != null && LineInfoProvider.HasLineInfo) ? new XamlNodeLineInfo (info, LineInfoProvider.LineNumber, LineInfoProvider.LinePosition) : new XamlNodeLineInfo (info, 0, 0);
			queue.Enqueue (nli);
		}
	}
}
