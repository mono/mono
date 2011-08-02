//
// Copyright (C) 2011 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Threading;

namespace System.Xaml
{
	public class XamlBackgroundReader : XamlReader, IXamlLineInfo
	{
		public XamlBackgroundReader (XamlReader wrappedReader)
		{
			if (wrappedReader == null)
				throw new ArgumentNullException ("wrappedReader");
			r = wrappedReader;
			q = new XamlNodeQueue (r.SchemaContext) { LineInfoProvider = r as IXamlLineInfo };
		}
		
		Thread thread;
		XamlReader r;
		XamlNodeQueue q;
		bool read_all_done, do_work = true;
		ManualResetEvent wait = new ManualResetEvent (true);

		public bool HasLineInfo {
			get { return ((IXamlLineInfo) q.Reader).HasLineInfo; }
		}
		
		public override bool IsEof {
			get { return read_all_done && q.IsEmpty; }
		}
		
		public int LineNumber {
			get { return ((IXamlLineInfo) q.Reader).LineNumber; }
		}
		
		[MonoTODO ("always returns 0")]
		public int LinePosition {
			get { return ((IXamlLineInfo) q.Reader).LinePosition; }
		}
		
		public override XamlMember Member {
			get { return q.Reader.Member; }
		}
		
		public override NamespaceDeclaration Namespace {
			get { return q.Reader.Namespace; }
		}
		
		public override XamlNodeType NodeType {
			get { return q.Reader.NodeType; }
		}
		
		public override XamlSchemaContext SchemaContext {
			get { return q.Reader.SchemaContext; }
		}
		
		public override XamlType Type {
			get { return q.Reader.Type; }
		}
		
		public override object Value {
			get { return q.Reader.Value; }
		}

		protected override void Dispose (bool disposing)
		{
			do_work = false;
		}
		
		public override bool Read ()
		{
			if (q.IsEmpty)
				wait.WaitOne ();
			return q.Reader.Read ();
		}
		
		public void StartThread ()
		{
			StartThread ("XAML reader thread"); // documented name
		}
		
		public void StartThread (string threadName)
		{
			if (thread != null)
				throw new InvalidOperationException ("Thread has already started");
			thread = new Thread (new ParameterizedThreadStart (delegate {
				while (do_work && r.Read ()) {
					q.Writer.WriteNode (r);
					wait.Set ();
				}
				read_all_done = true;
			})) { Name = threadName };
			thread.Start ();
		}
	}
}
