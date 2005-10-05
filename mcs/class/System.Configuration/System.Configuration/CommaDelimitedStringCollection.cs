//
// System.Configuration.CommaDelimitedStringCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System;
using System.Collections.Specialized;

namespace System.Configuration {

	/* i really hate all these "new"s... maybe
	 * StringCollection marks these methods as virtual in
	 * 2.0? */
	public sealed class CommaDelimitedStringCollection : StringCollection {

		bool modified;
		bool readOnly;

		public bool IsModified {
			get {
				return modified;
			}
		}

		public new bool IsReadOnly {
			get {
				return readOnly;
			}
		}

		[MonoTODO ("exception type")]
		public new string this [int index] {
			get {
				return base [index];
			}
			set {
				if (readOnly)
					throw new NotSupportedException (); /* XXX */

				base [index] = value;
				modified = true;
			}
		}

		[MonoTODO ("exception type")]
		public new void Add (string value)
		{
			if (readOnly)
				throw new NotSupportedException (); /* XXX */

			base.Add (value);
			modified = true;
		}

		[MonoTODO ("exception type")]
		public new void AddRange (string[] range)
		{
			if (readOnly)
				throw new NotSupportedException (); /* XXX */

			base.AddRange (range);
			modified = true;
		}

		[MonoTODO ("exception type")]
		public new void Clear ()
		{
			if (readOnly)
				throw new NotSupportedException (); /* XXX */

			base.Clear ();
			modified = true;
		}

		public CommaDelimitedStringCollection Clone ()
		{
			CommaDelimitedStringCollection col = new CommaDelimitedStringCollection();
			string[] contents = new string[this.Count];
			CopyTo (contents, 0);
			
			col.AddRange (contents);

			return col;
		}

		[MonoTODO ("exception type")]
		public new void Insert (int index, string value)
		{
			if (readOnly)
				throw new NotSupportedException (); /* XXX */

			base.Insert (index, value);
			modified = true;
		}

		[MonoTODO ("exception type")]
		public new void Remove (string value)
		{
			if (readOnly)
				throw new NotSupportedException (); /* XXX */

			base.Remove (value);
			modified = true;
		}

		public void SetReadOnly ()
		{
			readOnly = true;
		}

		public new string ToString ()
		{
			string[] contents = new string[this.Count];

			CopyTo (contents, 0);

			return String.Join (",", contents);
		}
	}

}

#endif
