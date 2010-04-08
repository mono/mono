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
	public abstract class XamlWriter : IDisposable
	{
		protected bool IsDisposed { get; private set; }
		public abstract XamlSchemaContext SchemaContext { get; }

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

		public abstract void WriteEndMember ();
		public abstract void WriteEndObject ();
		public abstract void WriteGetObject ();
		public abstract void WriteNamespace (NamespaceDeclaration namespaceDeclaration);
		public abstract void WriteStartMember (XamlMember xamlMember);
		public abstract void WriteStartObject (XamlType type);
		public abstract void WriteValue (object value);

		public void WriteNode (XamlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			switch (reader.NodeType) {
			case XamlNodeType.StartObject:
				WriteStartObject (reader.Type);
				break;
			case XamlNodeType.GetObject:
				WriteGetObject ();
				break;
			case XamlNodeType.EndObject:
				WriteEndObject ();
				break;
			case XamlNodeType.StartMember:
				WriteStartMember (reader.Member);
				break;
			case XamlNodeType.EndMember:
				WriteEndMember ();
				break;
			case XamlNodeType.Value:
				WriteValue (reader.Value);
				break;
			case XamlNodeType.NamespaceDeclaration:
				WriteNamespace (reader.Namespace);
				break;
			default:
				throw NotImplemented (); // documented behavior
			}
		}

		Exception NotImplemented ()
		{
			return new NotImplementedException ();
		}
	}
}
