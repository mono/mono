//
// System.Net.Mime.ContentType.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	John Luke (john.luke@gmail.com)
//	Atsushi Eno (atsushieno@veritas-vos-liberabit.com)
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) John Luke, 2005
// Copyright (C) Atsushi Eno, 2011
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

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal class ContentType
	{
		string mediaType;
		Dictionary<string,string> parameters = new Dictionary<string,string> ();

		public ContentType (string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException ("contentType");
			if (contentType.Length == 0)
				throw new ArgumentException ("contentType");

			string[] split = contentType.Split (';');
			this.MediaType = split[0].Trim ();
			for (int i = 1; i < split.Length; i++)
				Parse (split[i].Trim ());
		}

		static char [] eq = new char [] { '=' };
		void Parse (string pair)
		{
			if (String.IsNullOrEmpty (pair))
				return;

			string [] split = pair.Split (eq, 2);
			string key = split [0].Trim ();
			string val =  (split.Length > 1) ? split [1].Trim () : "";
			int l = val.Length;
			if (l >= 2 && val [0] == '"' && val [l - 1] == '"')
				val = val.Substring (1, l - 2);
			parameters.Add (key, val);
		}

		public string MediaType {
			get { return mediaType; }
			set {
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Length < 1)
					throw new ArgumentException ();
				if (value.IndexOf ('/') < 1)
					throw new FormatException ();
				if (value.IndexOf (';') != -1)
					throw new FormatException ();
				mediaType = value;
			}
		}

		public Dictionary<string,string> Parameters {
			get { return parameters; }
		}
	}
}

