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
using System.Windows.Markup;

namespace System.Xaml
{
	public class XamlObjectWriter : XamlWriter, IXamlLineInfoConsumer
	{
		public XamlObjectWriter (XamlSchemaContext schemaContext)
		{
		}

		public XamlObjectWriter (XamlSchemaContext schemaContext, XamlObjectWriterSettings settings)
		{
		}

		public virtual object Result {
			get { throw new NotImplementedException (); }
		}
		public INameScope RootNameScope {
			get { throw new NotImplementedException (); }
		}
		public override XamlSchemaContext SchemaContext {
			get { throw new NotImplementedException (); }
		}
		public bool ShouldProvideLineInfo {
			get { throw new NotImplementedException (); }
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAfterBeginInit (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAfterEndInit (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAfterProperties (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnBeforeProperties (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool OnSetValue (object eventSender, XamlMember member, object value)
		{
			throw new NotImplementedException ();
		}

		public void SetLineInfo (int lineNumber, int linePosition)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndMember ()
		{
			throw new NotImplementedException ();
		}
		public override void WriteEndObject ()
		{
			throw new NotImplementedException ();
		}
		public override void WriteGetObject ()
		{
			throw new NotImplementedException ();
		}
		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			throw new NotImplementedException ();
		}
		public override void WriteStartMember (XamlMember property)
		{
			throw new NotImplementedException ();
		}
		public override void WriteStartObject (XamlType xamlType)
		{
			throw new NotImplementedException ();
		}
		public override void WriteValue (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
