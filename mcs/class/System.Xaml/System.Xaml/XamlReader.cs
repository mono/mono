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
	public abstract class XamlReader : IDisposable
	{
		protected bool IsDisposed { get; private set; }

		public abstract bool IsEof { get; }
		public abstract XamlMember Member { get; }
		public abstract NamespaceDeclaration Namespace { get; }
		public abstract XamlNodeType NodeType { get; }
		public abstract XamlSchemaContext SchemaContext { get; }
		public abstract XamlType Type { get; }
		public abstract object Value { get; }
		
		public void Close ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			IsDisposed = true;
		}
		
		void IDisposable.Dispose ()
		{
			Dispose (true);
		}
		
		public abstract bool Read ();
		
		[MonoTODO]
		public virtual XamlReader ReadSubtree ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual void Skip ()
		{
			switch (NodeType) {
			case XamlNodeType.StartMember:
			case XamlNodeType.StartObject:
				if (!Read ())
					return;
				while (true) {
					switch (NodeType) {
					case XamlNodeType.StartMember:
					case XamlNodeType.StartObject:
						Skip ();
						continue;
					case XamlNodeType.EndMember:
					case XamlNodeType.EndObject:
						Read ();
						return;
					default:
						Read ();
						continue;
					}
				}

			default:
				Read ();
				return;
			}
		}
	}
}
