//
// System.Net.Mime.ContentType.cs
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

using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Net.Mime {
	public class ContentType
	{
		#region Fields

		string mediaType;
		StringDictionary parameters = new StringDictionary ();

		#endregion // Fields

		#region Constructors

		public ContentType ()
		{
			mediaType = "application/octet-stream";
		}
	
		public ContentType (string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException ("contentType");
			if (contentType.Length < 1)
				throw new ArgumentException ("contentType");

			try {
				int index = contentType.IndexOf (";");
				if (index > 0) {
					string[] split = contentType.Split (';');
					this.mediaType = split[0].Trim ();
					for (int i = 1; i < split.Length; i++)
					{
						Parse (split[i]);
					}
				}
				else {
					this.mediaType = contentType.Trim ();
				}
			} catch {
				throw new FormatException ();
			}
		}

		// parse key=value pairs like:
		// "charset=us-ascii"
		void Parse (string pair)
		{
			if (pair == null || pair.Length < 1)
				return;

			string[] split = pair.Split ('=');
			if (split.Length == 2) {
				switch (split[0].Trim ()) {
					case "boundary":
					case "charset":
					case "name":
						parameters.Add (split[0].Trim (), split[1].Trim ());
						break;
					default:
						// apparently parameters must go through Parameters.Add
						throw new FormatException ("invalid content-type format");
				}
			}
		}

		#endregion // Constructors

		#region Properties

		public string Boundary {
			get { return parameters["boundary"]; }
			set { parameters["boundary"] = value; }
		}

		public string CharSet {
			get { return parameters["charset"]; }
			set { parameters["charset"] = value; }
		}

		public string MediaType {
			get { return mediaType; }
			set {
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Length < 1)
					throw new ArgumentException ();
				if (value.IndexOf (';') != -1)
					throw new FormatException ();
				mediaType = value;
			}
		}

		public string Name {
			get { return parameters["name"]; }
			set { parameters["name"] = value; }
		}

		public StringDictionary Parameters {
			get { return parameters; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			return Equals (obj as ContentType);
		}

		bool Equals (ContentType other)
		{
			return other != null && ToString () == other.ToString ();
		}
		
		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (MediaType);
			if (Parameters != null && Parameters.Count > 0) {
				foreach (DictionaryEntry pair in parameters)
				{
					if (pair.Value != null && pair.Value.ToString ().Length > 0) {	
						sb.Append ("; ");
						sb.Append (pair.Key);
						sb.Append ("=");
						sb.Append (pair.Value);
					}
				}
			}
			return sb.ToString ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
