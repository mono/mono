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

using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace System.Net.Mime {
	public class ContentDisposition
	{
		// FIXME: "r" was not enough, neither was zzz
		// so this will fail if the offset is not an even hour
		const string rfc822 = "dd MMM yyyy HH':'mm':'ss zz00";
		
		#region Fields

		string dispositionType;
		StringDictionary parameters = new StringDictionary ();

		#endregion // Fields

		#region Constructors

		public ContentDisposition () : this (DispositionTypeNames.Attachment)
		{
		}

		public ContentDisposition (string disposition)
		{
			if (disposition == null)
				throw new ArgumentNullException ();
			if (disposition.Length < 1)
				throw new FormatException ();
			Size = -1;

			try {
				int index = disposition.IndexOf (';');
				if (index < 0) {
					dispositionType = disposition.Trim ();
				}
				else {
					string[] split = disposition.Split (';');
					dispositionType = split[0].Trim ();
					for (int i = 1; i < split.Length; i++)
						Parse (split[i]);
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
			if (split.Length == 2)
				parameters.Add (split[0].Trim (), split[1].Trim ());
			else
				throw new FormatException ();
		}

		#endregion // Constructors

		#region Properties

		public DateTime CreationDate {
			get {
				if (parameters.ContainsKey ("creation-date"))
					return DateTime.ParseExact (parameters["creation-date"], rfc822, null);
				else
					return DateTime.MinValue;
			}
			set {
				if (value > DateTime.MinValue)
					parameters["creation-date"] = value.ToString (rfc822);
				else
					parameters.Remove ("modification-date");
			}
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
			get { return parameters["filename"]; }
			set { parameters["filename"] = value; }
		}

		public bool Inline {
			get { return String.Compare (dispositionType, DispositionTypeNames.Inline, true, CultureInfo.InvariantCulture) == 0; }
			set {
				if (value)
					dispositionType = DispositionTypeNames.Inline;
				else
					dispositionType = DispositionTypeNames.Attachment;
			}
		}

		public DateTime ModificationDate {
			get {
				if (parameters.ContainsKey ("modification-date"))
					return DateTime.ParseExact (parameters["modification-date"], rfc822, null);
				else
					return DateTime.MinValue;
			}
			set {
				if (value > DateTime.MinValue)
					parameters["modification-date"] = value.ToString (rfc822);
				else
					parameters.Remove ("modification-date");
			}
		}

		public StringDictionary Parameters {
			get { return parameters; }
		}

		public DateTime ReadDate {
			get {
				if (parameters.ContainsKey ("read-date"))
					return DateTime.ParseExact (parameters["read-date"], rfc822, null);
				else
					return DateTime.MinValue;
			}
			set {
				if (value > DateTime.MinValue)
					parameters["read-date"] = value.ToString (rfc822);
				else
					parameters.Remove ("read-date");
			}
		}

		public long Size {
			get {
				if (parameters.ContainsKey ("size"))
					return long.Parse (parameters["size"]);
				else
					return -1;
			}
			set {
				if (value > -1)
					parameters["size"] = value.ToString ();
				else
					parameters.Remove ("size");
			}
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			return Equals (obj as ContentDisposition);
		}

		bool Equals (ContentDisposition other)
		{
			return other != null && ToString () == other.ToString ();
		}

		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		public override string ToString ()
		{
			// the content-disposition header as in RFC 2183
			// ex. attachment; filename=genome.jpeg; modification-date="Wed, 12 Feb 1997 16:29:51 -0500";
			// the dates must be quoted and in RFC 822 format
			//
			// According to RFC 2183, the filename field value follows the definition
			// given in RFC 1521, which is
			//
			//  value := token / quoted-string
			//
			StringBuilder sb = new StringBuilder ();
			sb.Append (DispositionType.ToLower ());
			if (Parameters != null && Parameters.Count > 0) {
				bool quote = false;
				string key, value;
				
				foreach (DictionaryEntry pair in Parameters)
				{
					if (pair.Value != null && pair.Value.ToString ().Length > 0) {
						sb.Append ("; ");
						sb.Append (pair.Key);
						sb.Append ("=");

						key = pair.Key.ToString ();
						value = pair.Value.ToString ();
						if ((key == "filename" && value.IndexOf (' ') != -1) || key.EndsWith ("date"))
							quote = true;
						else
							quote = false;
						
						if (quote)
							sb.Append ("\"");
						sb.Append (value);
						if (quote)
							sb.Append ("\"");
					}
				}
			}
			return sb.ToString ();
		}

		#endregion // Methods
	}
}

