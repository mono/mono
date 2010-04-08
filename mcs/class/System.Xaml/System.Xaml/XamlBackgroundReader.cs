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
using System.Collections.Generic;

namespace System.Xaml
{
	public class XamlBackgroundReader : XamlReader, IXamlLineInfo
	{
		public XamlBackgroundReader (XamlReader wrappedReader)
		{
			throw new NotImplementedException ();
		}
		
		public bool HasLineInfo {
			get { throw new NotImplementedException (); }
		}
		public override bool IsEof {
			get { throw new NotImplementedException (); }
		}
		public int LineNumber {
			get { throw new NotImplementedException (); }
		}
		public int LinePosition {
			get { throw new NotImplementedException (); }
		}
		public override XamlMember Member {
			get { throw new NotImplementedException (); }
		}
		public override NamespaceDeclaration Namespace {
			get { throw new NotImplementedException (); }
		}
		public override XamlNodeType NodeType {
			get { throw new NotImplementedException (); }
		}
		public override XamlSchemaContext SchemaContext {
			get { throw new NotImplementedException (); }
		}
		public override XamlType Type {
			get { throw new NotImplementedException (); }
		}
		public override object Value {
			get { throw new NotImplementedException (); }
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}
		
		public void StartThread ()
		{
			throw new NotImplementedException ();
		}
		
		public void StartThread (string threadName)
		{
			throw new NotImplementedException ();
		}
	}
}
