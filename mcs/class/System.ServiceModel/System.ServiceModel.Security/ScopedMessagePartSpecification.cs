//
// ScopedMessagePartSpecification.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
	public class ScopedMessagePartSpecification
	{
		public ScopedMessagePartSpecification ()
		{
			table = new Dictionary<string,MessagePartSpecification> ();
			parts = new MessagePartSpecification ();
		}

		public ScopedMessagePartSpecification (
			ScopedMessagePartSpecification other)
		{
			XmlQualifiedName [] array = new XmlQualifiedName [other.parts.HeaderTypes.Count];
			other.parts.HeaderTypes.CopyTo (array, 0);
			parts = new MessagePartSpecification (
				other.parts.IsBodyIncluded, array);
			table = new Dictionary<string,MessagePartSpecification> (other.table);
		}

		Dictionary<string,MessagePartSpecification> table;
		MessagePartSpecification parts;
		bool is_readonly;

		public ICollection<string> Actions {
			get { return table.Keys; }
		}

		public MessagePartSpecification ChannelParts {
			get { return parts; }
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public void AddParts (MessagePartSpecification parts)
		{
			if (parts == null)
				throw new ArgumentNullException ("parts");
			if (IsReadOnly)
				throw new InvalidOperationException ("This ScopedMessagePartSpecification is read-only.");
			ChannelParts.Union (parts);
		}

		public void AddParts (MessagePartSpecification parts,
			string action)
		{
			if (parts == null)
				throw new ArgumentNullException ("parts");
			if (action == null)
				throw new ArgumentNullException ("action");
			if (IsReadOnly)
				throw new InvalidOperationException ("This ScopedMessagePartSpecification is read-only.");

			MessagePartSpecification existing;
			if (table.TryGetValue (action, out existing))
				existing.Union (parts);
			else
				table.Add (action, parts);
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
		}

		public bool TryGetParts (
			string action, out MessagePartSpecification parts)
		{
			return TryGetParts (action, false, out parts);
		}

		public bool TryGetParts (
			string action, bool excludeChannelScope,
			out MessagePartSpecification parts)
		{
			return table.TryGetValue (action, out parts);
		}
	}
}
