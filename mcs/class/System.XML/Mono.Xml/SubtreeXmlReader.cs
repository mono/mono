//
// SubtreeXmlReader.cs - reads descendant nodes in XmlReader
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (c) 2004 Novell Inc. All rights reserved
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
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
	internal class SubtreeXmlReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
	{
		int startDepth;
		bool eof;
		bool initial;
		bool read;
		XmlReader Reader;
		IXmlLineInfo li;
		IXmlNamespaceResolver nsResolver;

		public SubtreeXmlReader (XmlReader reader)
		{
			this.Reader = reader;
			li = reader as IXmlLineInfo;
			nsResolver = reader as IXmlNamespaceResolver;
			initial = true;
			startDepth = reader.Depth;
			if (reader.ReadState == ReadState.Initial)
				startDepth = -1; // end == the reader's end
		}

		public override int AttributeCount {
			get { return initial ? 0 : Reader.AttributeCount; }
		}

#if NET_2_0
		public override bool CanReadBinaryContent {
			get { return Reader.CanReadBinaryContent; }
		}

		public override bool CanReadValueChunk {
			get { return Reader.CanReadValueChunk; }
		}
#endif

		public override int Depth {
			get { return Reader.Depth - startDepth; }
		}

		public override string BaseURI {
			get { return Reader.BaseURI; }
		}

		public override bool EOF {
			get { return eof || Reader.EOF; }
		}

		public int LineNumber {
			get { return initial ? 0 : li != null ? li.LineNumber : 0; }
		}

		public int LinePosition {
			get { return initial ? 0 : li != null ? li.LinePosition : 0; }
		}

		public override bool HasValue {
			get { return initial ? false : Reader.HasValue; }
		}

		public override string LocalName {
			get { return initial ? String.Empty : Reader.LocalName; }
		}

		public override string Name {
			get { return initial ? String.Empty : Reader.Name; }
		}

		public override XmlNameTable NameTable {
			get { return Reader.NameTable; }
		}

		public override string NamespaceURI {
			get { return initial ? String.Empty : Reader.NamespaceURI; }
		}

		public override XmlNodeType NodeType {
			get { return initial ? XmlNodeType.None : Reader.NodeType; }
		}

		public override string Prefix {
			get { return initial ? String.Empty : Reader.Prefix; }
		}

		public override ReadState ReadState {
			get { return initial ? ReadState.Initial : eof ? ReadState.EndOfFile : Reader.ReadState ; }
		}

		public override IXmlSchemaInfo SchemaInfo {
			get { return Reader.SchemaInfo; }
		}

		public override XmlReaderSettings Settings {
			get { return Reader.Settings; }
		}

		public override string Value {
			get { return initial ? String.Empty : Reader.Value; }
		}

		public override void Close ()
		{
			// do nothing
		}

		public override string GetAttribute (int i)
		{
			return initial ? null : Reader.GetAttribute (i);
		}

		public override string GetAttribute (string name)
		{
			return initial ? null : Reader.GetAttribute (name);
		}

		public override string GetAttribute (string local, string ns)
		{
			return initial ? null : Reader.GetAttribute (local, ns);
		}

		IDictionary IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return nsResolver != null ? nsResolver.GetNamespacesInScope (scope) : new Hashtable ();
		}

		public bool HasLineInfo ()
		{
			return li != null ? li.HasLineInfo () : false;
		}

		public override string LookupNamespace (string prefix)
		{
			return Reader.LookupNamespace (prefix);
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return nsResolver != null ? nsResolver.LookupPrefix (ns) : String.Empty;
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns, bool atomizedNames)
		{
			return nsResolver != null ? nsResolver.LookupPrefix (ns, atomizedNames) : String.Empty;
		}

		public override bool MoveToFirstAttribute ()
		{
			return initial ? false : Reader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			return initial ? false : Reader.MoveToNextAttribute ();
		}

		public override void MoveToAttribute (int i)
		{
			if (!initial)
				Reader.MoveToAttribute (i);
		}

		public override bool MoveToAttribute (string name)
		{
			return initial ? false : Reader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string local, string ns)
		{
			return initial ? false : Reader.MoveToAttribute (local, ns);
		}

		public override bool MoveToElement ()
		{
			return initial ? false : Reader.MoveToElement ();
		}

		public override bool Read ()
		{
			if (initial) {
				initial = false;
				return true;
			}
			if (!read) {
				read = true;
				return !Reader.IsEmptyElement && Reader.Read ();
			}
			if (Reader.Depth > startDepth)
				if (Reader.Read ())
					return true;
			eof = true;
			return false;
		}

		public override bool ReadAttributeValue ()
		{
			if (initial || eof)
				return false;
			return Reader.ReadAttributeValue ();
		}

		public override void ResolveEntity ()
		{
			if (initial || eof)
				return;
			Reader.ResolveEntity ();
		}
	}
}

#endif
