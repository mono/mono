// 
// Copied from System.Web.HttpUtility and marked internal
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Net {

	static class HttpUtility
	{
		sealed class HttpQSCollection : NameValueCollection
		{
			public override string ToString ()
			{
				int count = Count;
				if (count == 0)
					return "";
				StringBuilder sb = new StringBuilder ();
				string [] keys = AllKeys;
				for (int i = 0; i < count; i++) {
					sb.AppendFormat ("{0}={1}&", keys [i], this [keys [i]]);
				}
				if (sb.Length > 0)
					sb.Length--;
				return sb.ToString ();
			}
		}

		// Must be sorted
		static readonly long[] entities = new long[] {
			(long)'A' << 56 | (long)'E' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24, 
			(long)'A' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'A' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'A' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'A' << 56 | (long)'l' << 48 | (long)'p' << 40 | (long)'h' << 32 | (long)'a' << 24, 
			(long)'A' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'g' << 24, 
			(long)'A' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'A' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'B' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'C' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'d' << 32 | (long)'i' << 24 | (long)'l' << 16, 
			(long)'C' << 56 | (long)'h' << 48 | (long)'i' << 40, 
			(long)'D' << 56 | (long)'a' << 48 | (long)'g' << 40 | (long)'g' << 32 | (long)'e' << 24 | (long)'r' << 16, 
			(long)'D' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'t' << 32 | (long)'a' << 24, 
			(long)'E' << 56 | (long)'T' << 48 | (long)'H' << 40, 
			(long)'E' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'E' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'E' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'E' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'E' << 56 | (long)'t' << 48 | (long)'a' << 40, 
			(long)'E' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'G' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'m' << 32 | (long)'a' << 24, 
			(long)'I' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'I' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'I' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'I' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'I' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'K' << 56 | (long)'a' << 48 | (long)'p' << 40 | (long)'p' << 32 | (long)'a' << 24, 
			(long)'L' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'b' << 32 | (long)'d' << 24 | (long)'a' << 16, 
			(long)'M' << 56 | (long)'u' << 48, 
			(long)'N' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'N' << 56 | (long)'u' << 48, 
			(long)'O' << 56 | (long)'E' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24, 
			(long)'O' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'O' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'O' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'O' << 56 | (long)'m' << 48 | (long)'e' << 40 | (long)'g' << 32 | (long)'a' << 24, 
			(long)'O' << 56 | (long)'m' << 48 | (long)'i' << 40 | (long)'c' << 32 | (long)'r' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'O' << 56 | (long)'s' << 48 | (long)'l' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'h' << 16, 
			(long)'O' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'O' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'P' << 56 | (long)'h' << 48 | (long)'i' << 40, 
			(long)'P' << 56 | (long)'i' << 48, 
			(long)'P' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24, 
			(long)'P' << 56 | (long)'s' << 48 | (long)'i' << 40, 
			(long)'R' << 56 | (long)'h' << 48 | (long)'o' << 40, 
			(long)'S' << 56 | (long)'c' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'o' << 24 | (long)'n' << 16, 
			(long)'S' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24, 
			(long)'T' << 56 | (long)'H' << 48 | (long)'O' << 40 | (long)'R' << 32 | (long)'N' << 24, 
			(long)'T' << 56 | (long)'a' << 48 | (long)'u' << 40, 
			(long)'T' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24, 
			(long)'U' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'U' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'U' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'U' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'U' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'X' << 56 | (long)'i' << 48, 
			(long)'Y' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'Y' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'Z' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'a' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'a' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'a' << 56 | (long)'c' << 48 | (long)'u' << 40 | (long)'t' << 32 | (long)'e' << 24, 
			(long)'a' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24, 
			(long)'a' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'a' << 56 | (long)'l' << 48 | (long)'e' << 40 | (long)'f' << 32 | (long)'s' << 24 | (long)'y' << 16 | (long)'m' << 8, 
			(long)'a' << 56 | (long)'l' << 48 | (long)'p' << 40 | (long)'h' << 32 | (long)'a' << 24, 
			(long)'a' << 56 | (long)'m' << 48 | (long)'p' << 40, 
			(long)'a' << 56 | (long)'n' << 48 | (long)'d' << 40, 
			(long)'a' << 56 | (long)'n' << 48 | (long)'g' << 40, 
			(long)'a' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'g' << 24, 
			(long)'a' << 56 | (long)'s' << 48 | (long)'y' << 40 | (long)'m' << 32 | (long)'p' << 24, 
			(long)'a' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'a' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'b' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'b' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'b' << 56 | (long)'r' << 48 | (long)'v' << 40 | (long)'b' << 32 | (long)'a' << 24 | (long)'r' << 16, 
			(long)'b' << 56 | (long)'u' << 48 | (long)'l' << 40 | (long)'l' << 32, 
			(long)'c' << 56 | (long)'a' << 48 | (long)'p' << 40, 
			(long)'c' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'d' << 32 | (long)'i' << 24 | (long)'l' << 16, 
			(long)'c' << 56 | (long)'e' << 48 | (long)'d' << 40 | (long)'i' << 32 | (long)'l' << 24, 
			(long)'c' << 56 | (long)'e' << 48 | (long)'n' << 40 | (long)'t' << 32, 
			(long)'c' << 56 | (long)'h' << 48 | (long)'i' << 40, 
			(long)'c' << 56 | (long)'i' << 48 | (long)'r' << 40 | (long)'c' << 32, 
			(long)'c' << 56 | (long)'l' << 48 | (long)'u' << 40 | (long)'b' << 32 | (long)'s' << 24, 
			(long)'c' << 56 | (long)'o' << 48 | (long)'n' << 40 | (long)'g' << 32, 
			(long)'c' << 56 | (long)'o' << 48 | (long)'p' << 40 | (long)'y' << 32, 
			(long)'c' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'r' << 24, 
			(long)'c' << 56 | (long)'u' << 48 | (long)'p' << 40, 
			(long)'c' << 56 | (long)'u' << 48 | (long)'r' << 40 | (long)'r' << 32 | (long)'e' << 24 | (long)'n' << 16, 
			(long)'d' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'d' << 56 | (long)'a' << 48 | (long)'g' << 40 | (long)'g' << 32 | (long)'e' << 24 | (long)'r' << 16, 
			(long)'d' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'d' << 56 | (long)'e' << 48 | (long)'g' << 40, 
			(long)'d' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'t' << 32 | (long)'a' << 24, 
			(long)'d' << 56 | (long)'i' << 48 | (long)'a' << 40 | (long)'m' << 32 | (long)'s' << 24, 
			(long)'d' << 56 | (long)'i' << 48 | (long)'v' << 40 | (long)'i' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'e' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'e' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'e' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'e' << 56 | (long)'m' << 48 | (long)'p' << 40 | (long)'t' << 32 | (long)'y' << 24, 
			(long)'e' << 56 | (long)'m' << 48 | (long)'s' << 40 | (long)'p' << 32, 
			(long)'e' << 56 | (long)'n' << 48 | (long)'s' << 40 | (long)'p' << 32, 
			(long)'e' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'e' << 56 | (long)'q' << 48 | (long)'u' << 40 | (long)'i' << 32 | (long)'v' << 24, 
			(long)'e' << 56 | (long)'t' << 48 | (long)'a' << 40, 
			(long)'e' << 56 | (long)'t' << 48 | (long)'h' << 40, 
			(long)'e' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'e' << 56 | (long)'u' << 48 | (long)'r' << 40 | (long)'o' << 32, 
			(long)'e' << 56 | (long)'x' << 48 | (long)'i' << 40 | (long)'s' << 32 | (long)'t' << 24, 
			(long)'f' << 56 | (long)'n' << 48 | (long)'o' << 40 | (long)'f' << 32, 
			(long)'f' << 56 | (long)'o' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'l' << 24 | (long)'l' << 16, 
			(long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'1' << 24 | (long)'2' << 16, 
			(long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'1' << 24 | (long)'4' << 16, 
			(long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'3' << 24 | (long)'4' << 16, 
			(long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'l' << 24, 
			(long)'g' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'m' << 32 | (long)'a' << 24, 
			(long)'g' << 56 | (long)'e' << 48, 
			(long)'g' << 56 | (long)'t' << 48, 
			(long)'h' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'h' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'h' << 56 | (long)'e' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'t' << 24 | (long)'s' << 16, 
			(long)'h' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'l' << 32 | (long)'i' << 24 | (long)'p' << 16, 
			(long)'i' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'i' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'i' << 56 | (long)'e' << 48 | (long)'x' << 40 | (long)'c' << 32 | (long)'l' << 24, 
			(long)'i' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'i' << 56 | (long)'m' << 48 | (long)'a' << 40 | (long)'g' << 32 | (long)'e' << 24, 
			(long)'i' << 56 | (long)'n' << 48 | (long)'f' << 40 | (long)'i' << 32 | (long)'n' << 24, 
			(long)'i' << 56 | (long)'n' << 48 | (long)'t' << 40, 
			(long)'i' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'i' << 56 | (long)'q' << 48 | (long)'u' << 40 | (long)'e' << 32 | (long)'s' << 24 | (long)'t' << 16, 
			(long)'i' << 56 | (long)'s' << 48 | (long)'i' << 40 | (long)'n' << 32, 
			(long)'i' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'k' << 56 | (long)'a' << 48 | (long)'p' << 40 | (long)'p' << 32 | (long)'a' << 24, 
			(long)'l' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'l' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'b' << 32 | (long)'d' << 24 | (long)'a' << 16, 
			(long)'l' << 56 | (long)'a' << 48 | (long)'n' << 40 | (long)'g' << 32, 
			(long)'l' << 56 | (long)'a' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'l' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'l' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'i' << 32 | (long)'l' << 24, 
			(long)'l' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'l' << 56 | (long)'e' << 48, 
			(long)'l' << 56 | (long)'f' << 48 | (long)'l' << 40 | (long)'o' << 32 | (long)'o' << 24 | (long)'r' << 16, 
			(long)'l' << 56 | (long)'o' << 48 | (long)'w' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'t' << 16, 
			(long)'l' << 56 | (long)'o' << 48 | (long)'z' << 40, 
			(long)'l' << 56 | (long)'r' << 48 | (long)'m' << 40, 
			(long)'l' << 56 | (long)'s' << 48 | (long)'a' << 40 | (long)'q' << 32 | (long)'u' << 24 | (long)'o' << 16, 
			(long)'l' << 56 | (long)'s' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'l' << 56 | (long)'t' << 48, 
			(long)'m' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'r' << 32, 
			(long)'m' << 56 | (long)'d' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'h' << 24, 
			(long)'m' << 56 | (long)'i' << 48 | (long)'c' << 40 | (long)'r' << 32 | (long)'o' << 24, 
			(long)'m' << 56 | (long)'i' << 48 | (long)'d' << 40 | (long)'d' << 32 | (long)'o' << 24 | (long)'t' << 16, 
			(long)'m' << 56 | (long)'i' << 48 | (long)'n' << 40 | (long)'u' << 32 | (long)'s' << 24, 
			(long)'m' << 56 | (long)'u' << 48, 
			(long)'n' << 56 | (long)'a' << 48 | (long)'b' << 40 | (long)'l' << 32 | (long)'a' << 24, 
			(long)'n' << 56 | (long)'b' << 48 | (long)'s' << 40 | (long)'p' << 32, 
			(long)'n' << 56 | (long)'d' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'h' << 24, 
			(long)'n' << 56 | (long)'e' << 48, 
			(long)'n' << 56 | (long)'i' << 48, 
			(long)'n' << 56 | (long)'o' << 48 | (long)'t' << 40, 
			(long)'n' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'i' << 32 | (long)'n' << 24, 
			(long)'n' << 56 | (long)'s' << 48 | (long)'u' << 40 | (long)'b' << 32, 
			(long)'n' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'n' << 56 | (long)'u' << 48, 
			(long)'o' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'o' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'o' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24, 
			(long)'o' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'o' << 56 | (long)'l' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'e' << 24, 
			(long)'o' << 56 | (long)'m' << 48 | (long)'e' << 40 | (long)'g' << 32 | (long)'a' << 24, 
			(long)'o' << 56 | (long)'m' << 48 | (long)'i' << 40 | (long)'c' << 32 | (long)'r' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'o' << 56 | (long)'p' << 48 | (long)'l' << 40 | (long)'u' << 32 | (long)'s' << 24, 
			(long)'o' << 56 | (long)'r' << 48, 
			(long)'o' << 56 | (long)'r' << 48 | (long)'d' << 40 | (long)'f' << 32, 
			(long)'o' << 56 | (long)'r' << 48 | (long)'d' << 40 | (long)'m' << 32, 
			(long)'o' << 56 | (long)'s' << 48 | (long)'l' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'h' << 16, 
			(long)'o' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16, 
			(long)'o' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24 | (long)'s' << 16, 
			(long)'o' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'p' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'a' << 32, 
			(long)'p' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'t' << 32, 
			(long)'p' << 56 | (long)'e' << 48 | (long)'r' << 40 | (long)'m' << 32 | (long)'i' << 24 | (long)'l' << 16, 
			(long)'p' << 56 | (long)'e' << 48 | (long)'r' << 40 | (long)'p' << 32, 
			(long)'p' << 56 | (long)'h' << 48 | (long)'i' << 40, 
			(long)'p' << 56 | (long)'i' << 48, 
			(long)'p' << 56 | (long)'i' << 48 | (long)'v' << 40, 
			(long)'p' << 56 | (long)'l' << 48 | (long)'u' << 40 | (long)'s' << 32 | (long)'m' << 24 | (long)'n' << 16, 
			(long)'p' << 56 | (long)'o' << 48 | (long)'u' << 40 | (long)'n' << 32 | (long)'d' << 24, 
			(long)'p' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24, 
			(long)'p' << 56 | (long)'r' << 48 | (long)'o' << 40 | (long)'d' << 32, 
			(long)'p' << 56 | (long)'r' << 48 | (long)'o' << 40 | (long)'p' << 32, 
			(long)'p' << 56 | (long)'s' << 48 | (long)'i' << 40, 
			(long)'q' << 56 | (long)'u' << 48 | (long)'o' << 40 | (long)'t' << 32, 
			(long)'r' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'r' << 56 | (long)'a' << 48 | (long)'d' << 40 | (long)'i' << 32 | (long)'c' << 24, 
			(long)'r' << 56 | (long)'a' << 48 | (long)'n' << 40 | (long)'g' << 32, 
			(long)'r' << 56 | (long)'a' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'r' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'r' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'i' << 32 | (long)'l' << 24, 
			(long)'r' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'r' << 56 | (long)'e' << 48 | (long)'a' << 40 | (long)'l' << 32, 
			(long)'r' << 56 | (long)'e' << 48 | (long)'g' << 40, 
			(long)'r' << 56 | (long)'f' << 48 | (long)'l' << 40 | (long)'o' << 32 | (long)'o' << 24 | (long)'r' << 16, 
			(long)'r' << 56 | (long)'h' << 48 | (long)'o' << 40, 
			(long)'r' << 56 | (long)'l' << 48 | (long)'m' << 40, 
			(long)'r' << 56 | (long)'s' << 48 | (long)'a' << 40 | (long)'q' << 32 | (long)'u' << 24 | (long)'o' << 16, 
			(long)'r' << 56 | (long)'s' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'s' << 56 | (long)'b' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24, 
			(long)'s' << 56 | (long)'c' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'o' << 24 | (long)'n' << 16, 
			(long)'s' << 56 | (long)'d' << 48 | (long)'o' << 40 | (long)'t' << 32, 
			(long)'s' << 56 | (long)'e' << 48 | (long)'c' << 40 | (long)'t' << 32, 
			(long)'s' << 56 | (long)'h' << 48 | (long)'y' << 40, 
			(long)'s' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24, 
			(long)'s' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24 | (long)'f' << 16, 
			(long)'s' << 56 | (long)'i' << 48 | (long)'m' << 40, 
			(long)'s' << 56 | (long)'p' << 48 | (long)'a' << 40 | (long)'d' << 32 | (long)'e' << 24 | (long)'s' << 16, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'b' << 40, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'b' << 40 | (long)'e' << 32, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'m' << 40, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'1' << 32, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'2' << 32, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'3' << 32, 
			(long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'e' << 32, 
			(long)'s' << 56 | (long)'z' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24, 
			(long)'t' << 56 | (long)'a' << 48 | (long)'u' << 40, 
			(long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'r' << 32 | (long)'e' << 24 | (long)'4' << 16, 
			(long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24, 
			(long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24 | (long)'s' << 16 | (long)'y' << 8 | (long)'m' << 0, 
			(long)'t' << 56 | (long)'h' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'s' << 24 | (long)'p' << 16, 
			(long)'t' << 56 | (long)'h' << 48 | (long)'o' << 40 | (long)'r' << 32 | (long)'n' << 24, 
			(long)'t' << 56 | (long)'i' << 48 | (long)'l' << 40 | (long)'d' << 32 | (long)'e' << 24, 
			(long)'t' << 56 | (long)'i' << 48 | (long)'m' << 40 | (long)'e' << 32 | (long)'s' << 24, 
			(long)'t' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'d' << 32 | (long)'e' << 24, 
			(long)'u' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'u' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'u' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32, 
			(long)'u' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24, 
			(long)'u' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16, 
			(long)'u' << 56 | (long)'m' << 48 | (long)'l' << 40, 
			(long)'u' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'h' << 24, 
			(long)'u' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8, 
			(long)'u' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'w' << 56 | (long)'e' << 48 | (long)'i' << 40 | (long)'e' << 32 | (long)'r' << 24 | (long)'p' << 16, 
			(long)'x' << 56 | (long)'i' << 48, 
			(long)'y' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16, 
			(long)'y' << 56 | (long)'e' << 48 | (long)'n' << 40, 
			(long)'y' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32, 
			(long)'z' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32, 
			(long)'z' << 56 | (long)'w' << 48 | (long)'j' << 40, 
			(long)'z' << 56 | (long)'w' << 48 | (long)'n' << 40 | (long)'j' << 32
		};

		static readonly char[] entities_values = new char[] {
			'\u00C6',
			'\u00C1',
			'\u00C2',
			'\u00C0',
			'\u0391',
			'\u00C5',
			'\u00C3',
			'\u00C4',
			'\u0392',
			'\u00C7',
			'\u03A7',
			'\u2021',
			'\u0394',
			'\u00D0',
			'\u00C9',
			'\u00CA',
			'\u00C8',
			'\u0395',
			'\u0397',
			'\u00CB',
			'\u0393',
			'\u00CD',
			'\u00CE',
			'\u00CC',
			'\u0399',
			'\u00CF',
			'\u039A',
			'\u039B',
			'\u039C',
			'\u00D1',
			'\u039D',
			'\u0152',
			'\u00D3',
			'\u00D4',
			'\u00D2',
			'\u03A9',
			'\u039F',
			'\u00D8',
			'\u00D5',
			'\u00D6',
			'\u03A6',
			'\u03A0',
			'\u2033',
			'\u03A8',
			'\u03A1',
			'\u0160',
			'\u03A3',
			'\u00DE',
			'\u03A4',
			'\u0398',
			'\u00DA',
			'\u00DB',
			'\u00D9',
			'\u03A5',
			'\u00DC',
			'\u039E',
			'\u00DD',
			'\u0178',
			'\u0396',
			'\u00E1',
			'\u00E2',
			'\u00B4',
			'\u00E6',
			'\u00E0',
			'\u2135',
			'\u03B1',
			'\u0026',
			'\u2227',
			'\u2220',
			'\u00E5',
			'\u2248',
			'\u00E3',
			'\u00E4',
			'\u201E',
			'\u03B2',
			'\u00A6',
			'\u2022',
			'\u2229',
			'\u00E7',
			'\u00B8',
			'\u00A2',
			'\u03C7',
			'\u02C6',
			'\u2663',
			'\u2245',
			'\u00A9',
			'\u21B5',
			'\u222A',
			'\u00A4',
			'\u21D3',
			'\u2020',
			'\u2193',
			'\u00B0',
			'\u03B4',
			'\u2666',
			'\u00F7',
			'\u00E9',
			'\u00EA',
			'\u00E8',
			'\u2205',
			'\u2003',
			'\u2002',
			'\u03B5',
			'\u2261',
			'\u03B7',
			'\u00F0',
			'\u00EB',
			'\u20AC',
			'\u2203',
			'\u0192',
			'\u2200',
			'\u00BD',
			'\u00BC',
			'\u00BE',
			'\u2044',
			'\u03B3',
			'\u2265',
			'\u003E',
			'\u21D4',
			'\u2194',
			'\u2665',
			'\u2026',
			'\u00ED',
			'\u00EE',
			'\u00A1',
			'\u00EC',
			'\u2111',
			'\u221E',
			'\u222B',
			'\u03B9',
			'\u00BF',
			'\u2208',
			'\u00EF',
			'\u03BA',
			'\u21D0',
			'\u03BB',
			'\u2329',
			'\u00AB',
			'\u2190',
			'\u2308',
			'\u201C',
			'\u2264',
			'\u230A',
			'\u2217',
			'\u25CA',
			'\u200E',
			'\u2039',
			'\u2018',
			'\u003C',
			'\u00AF',
			'\u2014',
			'\u00B5',
			'\u00B7',
			'\u2212',
			'\u03BC',
			'\u2207',
			'\u00A0',
			'\u2013',
			'\u2260',
			'\u220B',
			'\u00AC',
			'\u2209',
			'\u2284',
			'\u00F1',
			'\u03BD',
			'\u00F3',
			'\u00F4',
			'\u0153',
			'\u00F2',
			'\u203E',
			'\u03C9',
			'\u03BF',
			'\u2295',
			'\u2228',
			'\u00AA',
			'\u00BA',
			'\u00F8',
			'\u00F5',
			'\u2297',
			'\u00F6',
			'\u00B6',
			'\u2202',
			'\u2030',
			'\u22A5',
			'\u03C6',
			'\u03C0',
			'\u03D6',
			'\u00B1',
			'\u00A3',
			'\u2032',
			'\u220F',
			'\u221D',
			'\u03C8',
			'\u0022',
			'\u21D2',
			'\u221A',
			'\u232A',
			'\u00BB',
			'\u2192',
			'\u2309',
			'\u201D',
			'\u211C',
			'\u00AE',
			'\u230B',
			'\u03C1',
			'\u200F',
			'\u203A',
			'\u2019',
			'\u201A',
			'\u0161',
			'\u22C5',
			'\u00A7',
			'\u00AD',
			'\u03C3',
			'\u03C2',
			'\u223C',
			'\u2660',
			'\u2282',
			'\u2286',
			'\u2211',
			'\u2283',
			'\u00B9',
			'\u00B2',
			'\u00B3',
			'\u2287',
			'\u00DF',
			'\u03C4',
			'\u2234',
			'\u03B8',
			'\u03D1',
			'\u2009',
			'\u00FE',
			'\u02DC',
			'\u00D7',
			'\u2122',
			'\u21D1',
			'\u00FA',
			'\u2191',
			'\u00FB',
			'\u00F9',
			'\u00A8',
			'\u03D2',
			'\u03C5',
			'\u00FC',
			'\u2118',
			'\u03BE',
			'\u00FD',
			'\u00A5',
			'\u00FF',
			'\u03B6',
			'\u200D',
			'\u200C'
		};

		#region Methods
	
		public static void HtmlAttributeEncode (string s, TextWriter output) 
		{
			output.Write(HtmlAttributeEncode(s));
		}
	
		public static string HtmlAttributeEncode (string s) 
		{
			if (null == s) 
				return null;
	
			bool needEncode = false;
			for (int i = 0; i < s.Length; i++) {
				if (s [i] == '&' || s [i] == '"' || s [i] == '<') {
					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			StringBuilder output = new StringBuilder ();
			int len = s.Length;
			for (int i = 0; i < len; i++)
				switch (s [i]) {
				case '&' : 
					output.Append ("&amp;");
					break;
				case '"' :
					output.Append ("&quot;");
					break;
				case '<':
					output.Append ("&lt;");
					break;
				default:
					output.Append (s [i]);
					break;
				}
	
			return output.ToString();
		}
	
		public static string UrlDecode (string str) 
		{
			return UrlDecode(str, Encoding.UTF8);
		}
	
		static char [] GetChars (MemoryStream b, Encoding e)
		{
			return e.GetChars (b.GetBuffer (), 0, (int) b.Length);
		}

		static void WriteCharBytes (IList buf, char ch, Encoding e)
		{
			if (ch > 255) {
				foreach (byte b in e.GetBytes (new char[] { ch }))
					buf.Add (b);
			} else
				buf.Add ((byte)ch);
		}
		
		public static string UrlDecode (string s, Encoding e)
		{
			if (null == s) 
				return null;

			if (s.IndexOf ('%') == -1 && s.IndexOf ('+') == -1)
				return s;
			
			if (e == null)
				e = Encoding.UTF8;

			long len = s.Length;
			var bytes = new List <byte> ();
			int xchar;
			char ch;
			
			for (int i = 0; i < len; i++) {
				ch = s [i];
				if (ch == '%' && i + 2 < len && s [i + 1] != '%') {
					if (s [i + 1] == 'u' && i + 5 < len) {
						// unicode hex sequence
						xchar = GetChar (s, i + 2, 4);
						if (xchar != -1) {
							WriteCharBytes (bytes, (char)xchar, e);
							i += 5;
						} else
							WriteCharBytes (bytes, '%', e);
					} else if ((xchar = GetChar (s, i + 1, 2)) != -1) {
						WriteCharBytes (bytes, (char)xchar, e);
						i += 2;
					} else {
						WriteCharBytes (bytes, '%', e);
					}
					continue;
				}

				if (ch == '+')
					WriteCharBytes (bytes, ' ', e);
				else
					WriteCharBytes (bytes, ch, e);
			}
			
			byte[] buf = bytes.ToArray ();
			bytes = null;
			return e.GetString (buf);
			
		}
	
		public static string UrlDecode (byte [] bytes, Encoding e)
		{
			if (bytes == null)
				return null;

			return UrlDecode (bytes, 0, bytes.Length, e);
		}

		static int GetInt (byte b)
		{
			char c = (char) b;
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			return -1;
		}

		static int GetChar (byte [] bytes, int offset, int length)
		{
			int value = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				int current = GetInt (bytes [i]);
				if (current == -1)
					return -1;
				value = (value << 4) + current;
			}

			return value;
		}

		static int GetChar (string str, int offset, int length)
		{
			int val = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				char c = str [i];
				if (c > 127)
					return -1;

				int current = GetInt ((byte) c);
				if (current == -1)
					return -1;
				val = (val << 4) + current;
			}

			return val;
		}
		
		public static string UrlDecode (byte [] bytes, int offset, int count, Encoding e)
		{
			if (bytes == null)
				return null;
			if (count == 0)
				return String.Empty;

			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			if (offset < 0 || offset > bytes.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > bytes.Length)
				throw new ArgumentOutOfRangeException ("count");

			StringBuilder output = new StringBuilder ();
			MemoryStream acc = new MemoryStream ();

			int end = count + offset;
			int xchar;
			for (int i = offset; i < end; i++) {
				if (bytes [i] == '%' && i + 2 < count && bytes [i + 1] != '%') {
					if (bytes [i + 1] == (byte) 'u' && i + 5 < end) {
						if (acc.Length > 0) {
							output.Append (GetChars (acc, e));
							acc.SetLength (0);
						}
						xchar = GetChar (bytes, i + 2, 4);
						if (xchar != -1) {
							output.Append ((char) xchar);
							i += 5;
							continue;
						}
					} else if ((xchar = GetChar (bytes, i + 1, 2)) != -1) {
						acc.WriteByte ((byte) xchar);
						i += 2;
						continue;
					}
				}

				if (acc.Length > 0) {
					output.Append (GetChars (acc, e));
					acc.SetLength (0);
				}

				if (bytes [i] == '+') {
					output.Append (' ');
				} else {
					output.Append ((char) bytes [i]);
				}
			}

			if (acc.Length > 0) {
				output.Append (GetChars (acc, e));
			}
			
			acc = null;
			return output.ToString ();
		}
	
		public static byte [] UrlDecodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			return UrlDecodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlDecodeToBytes (string str)
		{
			return UrlDecodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlDecodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (e == null)
				throw new ArgumentNullException ("e");

			return UrlDecodeToBytes (e.GetBytes (str));
		}

		public static byte [] UrlDecodeToBytes (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;
			if (count == 0)
				return new byte [0];

			int len = bytes.Length;
			if (offset < 0 || offset >= len)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || offset > len - count)
				throw new ArgumentOutOfRangeException("count");

			MemoryStream result = new MemoryStream ();
			int end = offset + count;
			for (int i = offset; i < end; i++){
				char c = (char) bytes [i];
				if (c == '+') {
					c = ' ';
				} else if (c == '%' && i < end - 2) {
					int xchar = GetChar (bytes, i + 1, 2);
					if (xchar != -1) {
						c = (char) xchar;
						i += 2;
					}
				}
				result.WriteByte ((byte) c);
			}

			return result.ToArray ();
		}

		public static string UrlEncode(string str) 
		{
			return UrlEncode(str, Encoding.UTF8);
		}
	
		public static string UrlEncode (string s, Encoding Enc) 
		{
			if (s == null)
				return null;

			if (s == "")
				return "";

			bool needEncode = false;
			int len = s.Length;
			for (int i = 0; i < len; i++) {
				char c = s [i];
				if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z')) {
					if (NotEncoded (c))
						continue;

					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			// avoided GetByteCount call
			byte [] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
			int realLen = Enc.GetBytes (s, 0, s.Length, bytes, 0);
			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, 0, realLen));
		}
	  
		public static string UrlEncode (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return "";

			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, 0, bytes.Length));
		}

		public static string UrlEncode (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return "";

			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, offset, count));
		}

		public static byte [] UrlEncodeToBytes (string str)
		{
			return UrlEncodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlEncodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (str == "")
				return new byte [0];

			byte [] bytes = e.GetBytes (str);
			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return new byte [0];

			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		static char [] hexChars = "0123456789abcdef".ToCharArray ();

		static bool NotEncoded (char c)
		{
			return (c == '!' || c == '\'' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_');
		}

		static void UrlEncodeChar (char c, Stream result, bool isUnicode) {
			if (c > 255) {
				//FIXME: what happens when there is an internal error?
				//if (!isUnicode)
				//	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
				int idx;
				int i = (int) c;

				result.WriteByte ((byte)'%');
				result.WriteByte ((byte)'u');
				idx = i >> 12;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 8) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 4) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = i & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				return;
			}
			
			if (c > ' ' && NotEncoded (c)) {
				result.WriteByte ((byte)c);
				return;
			}
			if (c==' ') {
				result.WriteByte ((byte)'+');
				return;
			}
			if (	(c < '0') ||
				(c < 'A' && c > '9') ||
				(c > 'Z' && c < 'a') ||
				(c > 'z')) {
				if (isUnicode && c > 127) {
					result.WriteByte ((byte)'%');
					result.WriteByte ((byte)'u');
					result.WriteByte ((byte)'0');
					result.WriteByte ((byte)'0');
				}
				else
					result.WriteByte ((byte)'%');
				
				int idx = ((int) c) >> 4;
				result.WriteByte ((byte)hexChars [idx]);
				idx = ((int) c) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
			}
			else
				result.WriteByte ((byte)c);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;

			int len = bytes.Length;
			if (len == 0)
				return new byte [0];

			if (offset < 0 || offset >= len)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || count > len - offset)
				throw new ArgumentOutOfRangeException("count");

			MemoryStream result = new MemoryStream (count);
			int end = offset + count;
			for (int i = offset; i < end; i++)
				UrlEncodeChar ((char)bytes [i], result, false);

			return result.ToArray();
		}

		public static string UrlEncodeUnicode (string str)
		{
			if (str == null)
				return null;

			return Encoding.ASCII.GetString (UrlEncodeUnicodeToBytes (str));
		}

		public static byte [] UrlEncodeUnicodeToBytes (string str)
		{
			if (str == null)
				return null;

			if (str == "")
				return new byte [0];

			MemoryStream result = new MemoryStream (str.Length);
			foreach (char c in str){
				UrlEncodeChar (c, result, true);
			}
			return result.ToArray ();
		}

		static string ConvertKeyToEntity (string key)
		{
			var token = CalculateKeyValue (key);
			if (token == 0)
				return key;

			var idx = Array.BinarySearch (entities, token);
			if (idx < 0)
				return key;

			return entities_values [idx].ToString ();
		}

		static long CalculateKeyValue (string s)
		{
			if (s.Length > 8)
				return 0;

			long key = 0;
			for (int i = 0; i < s.Length; ++i) {
				long ch = s[i];
				if (ch > 'z' || ch < '0')
					return 0;

				key |= ch << ((7 - i) * 8);
			}

			return key;
		}

		/// <summary>
		/// Decodes an HTML-encoded string and returns the decoded string.
		/// </summary>
		/// <param name="s">The HTML string to decode. </param>
		/// <returns>The decoded text.</returns>
		public static string HtmlDecode (string s) 
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			if (s.IndexOf ('&') == -1)
				return s;

			StringBuilder entity = new StringBuilder ();
			StringBuilder output = new StringBuilder ();
			int len = s.Length;
			// 0 -> nothing,
			// 1 -> right after '&'
			// 2 -> between '&' and ';' but no '#'
			// 3 -> '#' found after '&' and getting numbers
			int state = 0;
			int number = 0;
			bool have_trailing_digits = false;
	
			for (int i = 0; i < len; i++) {
				char c = s [i];
				if (state == 0) {
					if (c == '&') {
						entity.Append (c);
						state = 1;
					} else {
						output.Append (c);
					}
					continue;
				}

				if (c == '&') {
					state = 1;
					if (have_trailing_digits) {
						entity.Append (number.ToString (CultureInfo.InvariantCulture));
						have_trailing_digits = false;
					}

					output.Append (entity.ToString ());
					entity.Length = 0;
					entity.Append ('&');
					continue;
				}

				if (state == 1) {
					if (c == ';') {
						state = 0;
						output.Append (entity.ToString ());
						output.Append (c);
						entity.Length = 0;
					} else {
						number = 0;
						if (c != '#') {
							state = 2;
						} else {
							state = 3;
						}
						entity.Append (c);
					}
				} else if (state == 2) {
					entity.Append (c);
					if (c == ';') {
						string key = entity.ToString ();
						if (key.Length > 1) {
							var skey = key.Substring (1, key.Length - 2);
							key = ConvertKeyToEntity (skey);
						}

						output.Append (key);
						state = 0;
						entity.Length = 0;
					}
				} else if (state == 3) {
					if (c == ';') {
						if (number > 65535) {
							output.Append ("&#");
							output.Append (number.ToString (CultureInfo.InvariantCulture));
							output.Append (";");
						} else {
							output.Append ((char) number);
						}
						state = 0;
						entity.Length = 0;
						have_trailing_digits = false;
					} else if (Char.IsDigit (c)) {
						number = number * 10 + ((int) c - '0');
						have_trailing_digits = true;
					} else {
						state = 2;
						if (have_trailing_digits) {
							entity.Append (number.ToString (CultureInfo.InvariantCulture));
							have_trailing_digits = false;
						}
						entity.Append (c);
					}
				}
			}

			if (entity.Length > 0) {
				output.Append (entity.ToString ());
			} else if (have_trailing_digits) {
				output.Append (number.ToString (CultureInfo.InvariantCulture));
			}
			return output.ToString ();
		}
	
		/// <summary>
		/// Decodes an HTML-encoded string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The HTML string to decode</param>
		/// <param name="output">The TextWriter output stream containing the decoded string. </param>
		public static void HtmlDecode(string s, TextWriter output) 
		{
			if (s != null)
				output.Write (HtmlDecode (s));
		}
	
		/// <summary>
		/// HTML-encodes a string and returns the encoded string.
		/// </summary>
		/// <param name="s">The text string to encode. </param>
		/// <returns>The HTML-encoded text.</returns>
		public static string HtmlEncode (string s) 
		{
			if (s == null)
				return null;

			bool needEncode = false;
			for (int i = 0; i < s.Length; i++) {
				char c = s [i];
				if (c == '&' || c == '"' || c == '<' || c == '>' || c > 159) {
					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			StringBuilder output = new StringBuilder ();
			
			int len = s.Length;
			for (int i = 0; i < len; i++) 
				switch (s [i]) {
				case '&' :
					output.Append ("&amp;");
					break;
				case '>' : 
					output.Append ("&gt;");
					break;
				case '<' :
					output.Append ("&lt;");
					break;
				case '"' :
					output.Append ("&quot;");
					break;
				default:
					// MS starts encoding with &# from 160 and stops at 255.
					// We don't do that. One reason is the 65308/65310 unicode
					// characters that look like '<' and '>'.
#if TARGET_JVM
					if (s [i] > 159 && s [i] < 256) {
#else
					if (s [i] > 159) {
#endif
						output.Append ("&#");
						output.Append (((int) s [i]).ToString (CultureInfo.InvariantCulture));
						output.Append (";");
					} else {
						output.Append (s [i]);
					}
					break;
				}
			return output.ToString ();
		}
	
		/// <summary>
		/// HTML-encodes a string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The string to encode. </param>
		/// <param name="output">The TextWriter output stream containing the encoded string. </param>
		public static void HtmlEncode(string s, TextWriter output) 
		{
			if (s != null)
				output.Write (HtmlEncode (s));
		}

		public static string UrlPathEncode (string s)
		{
			if (s == null || s.Length == 0)
				return s;

			MemoryStream result = new MemoryStream ();
			int length = s.Length;
			for (int i = 0; i < length; i++) {
				UrlPathEncodeChar (s [i], result);
			}
			return Encoding.ASCII.GetString (result.ToArray ());
		}
		
		static void UrlPathEncodeChar (char c, Stream result)
		{
			if (c < 33 || c > 126) {
				byte [] bIn = Encoding.UTF8.GetBytes (c.ToString ());
				for (int i = 0; i < bIn.Length; i++) {
					result.WriteByte ((byte) '%');
					int idx = ((int) bIn [i]) >> 4;
					result.WriteByte ((byte) hexChars [idx]);
					idx = ((int) bIn [i]) & 0x0F;
					result.WriteByte ((byte) hexChars [idx]);
				}
			}
			else if (c == ' ') {
				result.WriteByte ((byte) '%');
				result.WriteByte ((byte) '2');
				result.WriteByte ((byte) '0');
			}
			else
				result.WriteByte ((byte) c);
		}

		public static NameValueCollection ParseQueryString (string query)
		{
			return ParseQueryString (query, Encoding.UTF8);
		}

		public static NameValueCollection ParseQueryString (string query, Encoding encoding)
		{
			if (query == null)
				throw new ArgumentNullException ("query");
			if (encoding == null)
				throw new ArgumentNullException ("encoding");
			if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
				return new NameValueCollection ();
			if (query[0] == '?')
				query = query.Substring (1);
				
			NameValueCollection result = new HttpQSCollection ();
			ParseQueryString (query, encoding, result);
			return result;
		}

		internal static void ParseQueryString (string query, Encoding encoding, NameValueCollection result)
		{
			if (query.Length == 0)
				return;

			string decoded = HtmlDecode (query);
			int decodedLength = decoded.Length;
			int namePos = 0;
			bool first = true;
			while (namePos <= decodedLength) {
				int valuePos = -1, valueEnd = -1;
				for (int q = namePos; q < decodedLength; q++) {
					if (valuePos == -1 && decoded [q] == '=') {
						valuePos = q + 1;
					} else if (decoded [q] == '&') {
						valueEnd = q;
						break;
					}
				}

				if (first) {
					first = false;
					if (decoded [namePos] == '?')
						namePos++;
				}
				
				string name, value;
				if (valuePos == -1) {
					name = null;
					valuePos = namePos;
				} else {
					name = UrlDecode (decoded.Substring (namePos, valuePos - namePos - 1), encoding);
				}
				if (valueEnd < 0) {
					namePos = -1;
					valueEnd = decoded.Length;
				} else {
					namePos = valueEnd + 1;
				}
				value = UrlDecode (decoded.Substring (valuePos, valueEnd - valuePos), encoding);

				result.Add (name, value);
				if (namePos == -1)
					break;
			}
		}
		#endregion // Methods
	}
}

