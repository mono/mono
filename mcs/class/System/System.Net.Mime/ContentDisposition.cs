//
// System.Net.Mime.ContentDisposition.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	John Luke (john.luke@gmail.com)
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) John Luke, 2005
//

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

using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace System.Net.Mime {
	public class ContentDisposition
	{
		// FIXME: "r" was not enough, neither was zzz
		// so this will fail if the offset is not an even hour
		const string rfc822 = "dd MMM yyyy HH':'mm':'ss zz00";
		
		#region Fields

		DateTime creationDate;
		string dispositionType;
		string filename;
		DateTime modificationDate;
		DateTime readDate;
		long size = -1; // -1 means the size is unknown
		StringDictionary parameters = new StringDictionary ();

		#endregion // Fields

		#region Constructors

		public ContentDisposition ()
		{
			dispositionType = DispositionTypeNames.Attachment;
		}

		[MonoTODO]
		public ContentDisposition (string disposition)
		{
			if (disposition == null)
				throw new ArgumentNullException ();
			if (disposition.Length < 1)
				throw new FormatException ();

			try {
				int index = disposition.IndexOf (';');
				if (index < 0) {
					dispositionType = disposition.Trim ();
				}
				else {
					string[] split = disposition.Split (';');
					dispositionType = split[0].Trim ();
					for (int i = 1; i < split.Length; i++)
					{
						Parse (split[i]);
					}
				}
			} catch {
				throw new FormatException ();
			}
		}

		// the individual pieces
		void Parse (string pair)
		{
			if (pair == null || pair.Length < 0)
				return;

			string[] split = pair.Split ('=');
			switch (split[0]) {
				case "creation-date":
					creationDate = DateTime.ParseExact (split[1], rfc822, null);
					break;
				case "modification-date":
					modificationDate = DateTime.ParseExact (split[1], rfc822, null);
					break;
				case "read-date":
					readDate = DateTime.ParseExact (split[1], rfc822, null);
					break;
				case "filename":
					filename = split[1].Trim ();
					break;
				case "size":
					size = long.Parse (split[1]);
					break;
				// FIXME: this is a guess, not yet tested
				default:
					parameters.Add (split[0].Trim (), split[1].Trim ());
					break;
			}
		}

		#endregion // Constructors

		#region Properties

		public DateTime CreationDate {
			get { return creationDate; }
			set { creationDate = value; }
		}

		public string DispositionType {
			get { return dispositionType; }
			set {
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Length < 1)
					throw new ArgumentException ();
				dispositionType = value;
			}
		}

		public string FileName {
			get { return filename; }
			set { filename = value; }
		}

		public bool Inline {
			get { return dispositionType.ToLower () == DispositionTypeNames.Inline.ToLower (); }
			set {
				if (value)
					dispositionType = DispositionTypeNames.Inline;
				else
					dispositionType = DispositionTypeNames.Attachment;
			}
		}

		public DateTime ModificationDate {
			get { return modificationDate; }
			set { modificationDate = value; } 
		}

		public StringDictionary Parameters {
			get { return parameters; }
		}

		public DateTime ReadDate {
			get { return readDate; } 
			set { readDate = value; }
		}

		public long Size {
			get { return size; }
			set { size = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return Equals (obj as ContentDisposition);
		}

		bool Equals (ContentDisposition other)
		{
			return other != null && ToString () == other.ToString ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			// the content-disposition header as in RFC 2183
			// ex. attachment; filename=genome.jpeg; modification-date="Wed, 12 Feb 1997 16:29:51 -0500";
			// the dates must be quoted and in RFC 822 format
			StringBuilder sb = new StringBuilder ();
			sb.Append (DispositionType.ToLower ());
			if (CreationDate > DateTime.MinValue) {
				sb.Append ("; creation-date=\"");
				sb.Append (CreationDate.ToString (rfc822));
				sb.Append ("\"");
			}
			if (ModificationDate > DateTime.MinValue) {
				sb.Append ("; modification-date=\"");
				sb.Append (ModificationDate.ToString (rfc822));
				sb.Append ("\"");
			}
			if (ReadDate > DateTime.MinValue) {
				sb.Append ("; read-date=\"");
				sb.Append (ReadDate.ToString (rfc822));
				sb.Append ("\"");
			}
			if (FileName != null && FileName.Length > 0) {
				sb.Append ("; filename=");
				sb.Append (FileName);
			}
			if (Size > -1) {
				sb.Append ("; size=");
				sb.Append (Size.ToString ());
			}
			// this is a guess, not tested yet
			if (Parameters != null && Parameters.Count > 0) {
				foreach (DictionaryEntry pair in Parameters)
				{
					sb.Append ("; ");
					sb.Append (pair.Key);
					sb.Append ("=");
					sb.Append (pair.Value);
				}
			}
			return sb.ToString ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
