//
// AcceptEncodingConfig.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace Mono.Http.Configuration
{
	public class AcceptEncodingConfig
	{
		Hashtable tbl = CollectionsUtil.CreateCaseInsensitiveHashtable ();

		public AcceptEncodingConfig () : this (null)
		{
		}
		
		public AcceptEncodingConfig (AcceptEncodingConfig parent)
		{
			if (parent == null)
				return;

			// FIXME: copy parent's config
		}

		public void Add (string encoding, string type)
		{
			tbl [encoding] = Type.GetType (type);
		}

		public bool SetFilter (HttpResponse response, string acceptEncoding)
		{
			if (acceptEncoding == null)
				return false;

			acceptEncoding = acceptEncoding.Trim ();
			if (acceptEncoding == "")
				return false;
				
			string [] parts = null;
			if (acceptEncoding.IndexOf (';') != -1)
				parts = acceptEncoding.Split (';');
			else
				parts = acceptEncoding.Split (',');

			string encoding;
			float weight = 0.0f;
			float current = 0.0f;
			Type type = null;
			string name = null;
			int i = 0;
			foreach (string s in parts) {
				encoding = null;
				ParseValue (s, ref encoding, ref weight);
				if (encoding != null && weight > current && tbl.Contains (encoding)) {
					type = tbl [encoding] as Type;
					current = weight;
					name = encoding;
				}
				i++;
			}

			if (type == null)
				return false;

			Stream filter = response.Filter;
			response.Filter = (Stream) Activator.CreateInstance (type, new object [] {filter});
			response.AppendHeader ("Content-Encoding", name);
			return true;
		}

		public void Clear ()
		{
			tbl.Clear ();
		}
		
		static void ParseValue (string s, ref string encoding, ref float weight)
		{
			//FIXME: make it more spec compliant
			string [] parts = s.Trim ().Split (',');
			if (parts.Length == 1) {
				encoding = parts [0].Trim ();
				weight = 1.0f;
			} else if (parts.Length == 2) {
				encoding = parts [0].Trim ();
				try {
					int i = parts [1].IndexOf ('=');
					if (i != -1)
						weight = Convert.ToSingle (parts [1].Substring (i + 1));
				} catch {
					weight = 0.0f;
				}
			} else {
				//ignore
			}
		}
	}
}

