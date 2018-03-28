//
// MessagePartSpecification.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Xml;

namespace System.ServiceModel.Security
{
	// Represents WS-SecurityPolicy SignedParts or EncryptedParts.
	public class MessagePartSpecification
	{
		static XmlQualifiedName [] empty = new XmlQualifiedName [0];
		static MessagePartSpecification no_parts =
			new MessagePartSpecification ();

		public static MessagePartSpecification NoParts {
			get { return no_parts; }
		}

		public MessagePartSpecification ()
			: this (empty)
		{
		}

		public MessagePartSpecification (
			bool isBodyIncluded)
			: this (isBodyIncluded, empty)
		{
		}

		public MessagePartSpecification (params XmlQualifiedName[] headerTypes)
			: this (false, headerTypes)
		{
		}

		public MessagePartSpecification (
			bool isBodyIncluded,
			params XmlQualifiedName[] headerTypes)
		{
			body = isBodyIncluded;
			header_types = new List<XmlQualifiedName> (headerTypes);
		}

		bool body;
		IList<XmlQualifiedName> header_types;

		public ICollection<XmlQualifiedName> HeaderTypes {
			get { return header_types; }
		}

		public bool IsBodyIncluded {
			get { return body; }
			set { body = value; }
		}

		public bool IsReadOnly {
			get { return header_types.IsReadOnly; }
		}

		public void Clear ()
		{
			header_types.Clear ();
		}

		public void MakeReadOnly ()
		{
			if (!header_types.IsReadOnly)
				header_types = new ReadOnlyCollection<XmlQualifiedName> (header_types);
		}

		public void Union (MessagePartSpecification specification)
		{
			if (specification == null)
				throw new ArgumentNullException ("specification");
			if (header_types.IsReadOnly)
				throw new InvalidOperationException ("This MessagePartSpecification is read-only.");
			body |= specification.body;
			foreach (XmlQualifiedName q in specification.header_types)
				// Sigh. It could be much better here.
				//if (!header_types.Contains (q))
					header_types.Add (q);
		}
	}
}
