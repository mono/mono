//
// UniqueId.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2007 Novell, Inc.  http://www.novell.com
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

namespace System.Xml
{
	public class UniqueId
	{
		Guid guid;
		string id;

		public UniqueId ()
			: this (Guid.NewGuid ())
		{
		}

		public UniqueId (byte [] id)
			: this (id, 0)
		{
		}

		public UniqueId (Guid id)
		{
			this.guid = id;
		}

		public UniqueId (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value cannot be null", "value");

			if (value.Length == 0)
				throw new FormatException ("UniqueId cannot be zero length");

			this.id = value;
		}

		[MonoTODO]
		public UniqueId (byte [] id, int offset)
		{
			if (id == null)
				throw new ArgumentNullException ();
			if (offset < 0 || offset >= id.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (id.Length - offset != 16)
				throw new NotImplementedException ();

			guid = new Guid (new ArraySegment<byte> (id, offset, id.Length - offset).Array);
		}

		[MonoTODO]
		public UniqueId (char [] id, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int CharArrayLength {
			get { throw new NotImplementedException (); }
		}

		public bool IsGuid {
			get { return guid != default (Guid); }
		}

		[MonoTODO]
		public override bool Equals (Object obj)
		{
			UniqueId other = obj as UniqueId;

			if (other == null)
				return false;

			if (IsGuid && other.IsGuid) {
				Guid g1, g2;
				TryGetGuid (out g1);
				other.TryGetGuid (out g2);
				return g1.Equals (g2);
			}

			return ToString () == other.ToString ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public static bool operator == (UniqueId id1, UniqueId id2)
		{
			if ((object) id1 == null)
				return (object) id2 == null;
			if ((object) id2 == null)
				return false;
			return id1.Equals (id2);
		}

		public static bool operator != (UniqueId id1, UniqueId id2)
		{
			return ! (id1 == id2);
		}

		[MonoTODO]
		public int ToCharArray (char [] array, int offset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			if (IsGuid)
				return "urn:uuid:" + guid;

			return id;
		}

		public bool TryGetGuid (out Guid guid)
		{
			if (IsGuid) {
				guid = this.guid;
				return true;
			} else {
				guid = default (Guid);
				return false;
			}
		}

		public bool TryGetGuid (byte [] buffer, int offset)
		{
			Guid ret;
			if (!TryGetGuid (out ret))
				return false;
			byte [] bytes = ret.ToByteArray ();
			bytes.CopyTo (buffer, offset);
			return true;
		}
	}
}
#endif
