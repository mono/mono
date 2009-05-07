//
// UniqueId.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2005,2007,2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Security;

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

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public UniqueId (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value cannot be null", "value");

			if (value.Length == 0)
				throw new FormatException ("UniqueId cannot be zero length");

			this.id = value;
		}

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public UniqueId (byte [] id, int offset)
		{
			if (id == null)
				throw new ArgumentNullException ();
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (offset >= id.Length)
				throw new ArgumentException ("id too small.", "offset");

			if (id.Length - offset != 16)
				throw new ArgumentException ("id and offset provide less than 16 bytes");

			if (offset == 0)
				guid = new Guid (id);
			else {
				List<byte> buf = new List<byte> (id);
				buf.RemoveRange (0, offset);
				guid = new Guid (buf.ToArray ());
			}
		}

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public UniqueId (char [] id, int offset, int count)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (offset < 0 || offset >= id.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || id.Length - offset < count)
				throw new ArgumentOutOfRangeException ("count");
			if (count == 0)
				throw new FormatException ();

			// Does it start with "urn:uuid:"?  If so, it's a Guid.
			if (count > 8 && id [offset] == 'u' && id [offset+1] == 'r' && 
					id [offset+2] == 'n' && id [offset+3] == ':' &&
					id [offset+4] == 'u' && id [offset+5] == 'u' &&
					id [offset+6] == 'i' && id [offset+7] == 'd' &&
					id [offset+8] == ':') {
				if (count != 45)
					throw new ArgumentOutOfRangeException ("Invalid Guid");
				this.guid = new Guid (new string (id, offset+9, count-9));
			} else
				this.id = new string (id, offset, count);
		}

		public int CharArrayLength {
#if !NET_2_1
			[SecurityCritical]
			[SecurityTreatAsSafe]
#endif
			get {return id != null ? id.Length : 45;}
		}

		public bool IsGuid {
			get { return guid != default (Guid); }
		}

		public override bool Equals (Object obj)
		{
			UniqueId other = obj as UniqueId;

			if (other == null)
				return false;

			if (IsGuid && other.IsGuid) {
				return guid.Equals (other.guid);
			}

			return id == other.id;
		}

		[MonoTODO ("Determine semantics when IsGuid==true")]
		public override int GetHashCode ()
		{
			return id != null ? id.GetHashCode () : guid.GetHashCode ();
		}

		public static bool operator == (UniqueId id1, UniqueId id2)
		{
			return object.Equals (id1, id2);
		}

		public static bool operator != (UniqueId id1, UniqueId id2)
		{
			return ! (id1 == id2);
		}

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public int ToCharArray (char [] array, int offset)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (offset < 0 || offset >= array.Length)
				throw new ArgumentOutOfRangeException ("offset");

			string s = ToString ();
			s.CopyTo (0, array, offset, s.Length);
			return s.Length;
		}

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public override string ToString ()
		{
			if (id == null)
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

#if !NET_2_1
		[SecurityCritical]
		[SecurityTreatAsSafe]
#endif
		public bool TryGetGuid (byte [] buffer, int offset)
		{
			if (!IsGuid)
				return false;

			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (buffer.Length - offset < 16)
				throw new ArgumentOutOfRangeException ("offset");

			guid.ToByteArray ().CopyTo (buffer, offset);
			return true;
		}
	}
}
#endif
