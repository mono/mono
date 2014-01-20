//
// System.Net.ListenerPrefix
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Oleg Mihailik (mihailik gmail co_m)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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

#if SECURITY_DEP

namespace System.Net {
	sealed class ListenerPrefix
	{
		string original;
		string host;
		ushort port;
		string path;
		bool secure;
		IPAddress address;
		public HttpListener Listener;

		public ListenerPrefix (string prefix)
		{
			this.original = prefix;
			Parse (prefix);
		}

		public override string ToString ()
		{
			return original;
		}

		public IPAddress Address {
			get { return address; }
		}

		public bool Secure {
			get { return secure; }
		}

		public string Host {
			get { return host; }
		}

		public int Port {
			get { return (int) port; }
		}

		public string Path {
			get { return path; }
		}

		// Equals and GetHashCode are required to detect duplicates in HttpListenerPrefixCollection.
		public override bool Equals (object o)
		{
			ListenerPrefix other = o as ListenerPrefix;
			if (other == null)
				return false;

			return (original == other.original);
		}

		public override int GetHashCode ()
		{
			return original.GetHashCode ();
		}

		void Parse (string uri)
		{
			int default_port = (uri.StartsWith ("http://")) ? 80 : -1;
			if (default_port == -1) {
				default_port = (uri.StartsWith ("https://")) ? 443 : -1;
				secure = true;
			}

			int length = uri.Length;
			int start_host = uri.IndexOf (':') + 3;
			if (start_host >= length)
				throw new ArgumentException ("No host specified.");

			int root = uri.IndexOf('/', start_host, length - start_host);
			int colon = uri.LastIndexOf (':', root - 1, root - start_host - 1);
			if (colon > 0 && ushort.TryParse(uri.Substring(colon + 1, root - colon - 1), out port)) {
				host = uri.Substring (start_host, colon - start_host);
			} else {
				host = uri.Substring (start_host, root - start_host);
			}

			if (IPAddress.TryParse(host, out address))
				host = "*"; // not sure if * or + is more appropriate to match with MS.NET, but both are better than host header filtering to IP

			path = uri.Substring(root);
			if (path.Length != 1)
				path = path.Substring (0, path.Length - 1);
		}

		public static void CheckUri (string uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uriPrefix");

			int default_port = (uri.StartsWith ("http://")) ? 80 : -1;
			if (default_port == -1)
				default_port = (uri.StartsWith ("https://")) ? 443 : -1;
			if (default_port == -1)
				throw new ArgumentException ("Only 'http' and 'https' schemes are supported.");

			int length = uri.Length;
			int start_host = uri.IndexOf (':') + 3;
			if (start_host >= length)
				throw new ArgumentException ("No host specified.");

			int colon = uri.IndexOf (':', start_host, length - start_host);
			if (start_host == colon)
				throw new ArgumentException ("No host specified.");

			if (uri [uri.Length - 1] != '/')
				throw new ArgumentException ("The prefix must end with '/'");
		}
	}
}
#endif

