//
// System.Configuration.ConfigurationLocation.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System.Collections;

namespace System.Configuration {

	public class ConfigurationLocationCollection : ReadOnlyCollectionBase
	{
		internal ConfigurationLocationCollection ()
		{
		}
		
		public ConfigurationLocation this [int index] {
			get { return InnerList [index] as ConfigurationLocation; }
		}
		
		internal void Add (ConfigurationLocation loc)
		{
			InnerList.Add (loc);
		}
		
		internal ConfigurationLocation Find (string location)
		{
			foreach (ConfigurationLocation loc in InnerList)
				if (String.Compare (loc.Path, location, StringComparison.OrdinalIgnoreCase) == 0)
					return loc;
			return null;
		}

		internal ConfigurationLocation FindBest (string location)
		{
			if(String.IsNullOrEmpty (location))
				return null;
			
			ConfigurationLocation bestMatch = null;
			int locationlen = location.Length;
			int bestmatchlen = 0;
			
			foreach (ConfigurationLocation loc in InnerList) {
				string lpath = loc.Path;
				if (String.IsNullOrEmpty (lpath))
					continue;
				
				int lpathlen = lpath.Length;
				if (location.StartsWith (lpath, StringComparison.OrdinalIgnoreCase)) {
					// Exact match always takes precedence
					if (locationlen == lpathlen)
						return loc;
					
					// ensure path based comparisons consider full directory names (i.e. so 'admin' does not match an 'administration' path)
					if(locationlen > lpathlen && location [lpathlen] != '/')
						continue;

					if(bestMatch == null)
						bestMatch = loc;
					else if (bestmatchlen < lpathlen) {
						bestMatch = loc;
						bestmatchlen = lpathlen;
					}
				}
			}

			return bestMatch;
		}
	}
}
#endif
