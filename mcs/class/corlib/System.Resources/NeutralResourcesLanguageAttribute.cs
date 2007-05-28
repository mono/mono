//
// System.Resources.NeutralResourcesLanguageAttribute.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Resources
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class NeutralResourcesLanguageAttribute : Attribute
	{
		
		string culture;
#if NET_2_0
		UltimateResourceFallbackLocation loc;
#endif
		// Constructors
		public NeutralResourcesLanguageAttribute (string cultureName) 
		{
			if(cultureName==null) {
				throw new ArgumentNullException("culture is null");
			}
			
			culture = cultureName;
		}

		public string CultureName
		{
			get {
				return culture;
			}
		}

#if NET_2_0
		public NeutralResourcesLanguageAttribute (string cultureName, UltimateResourceFallbackLocation location)
		{
			if(cultureName==null) {
				throw new ArgumentNullException("culture is null");
			}
			
			culture = cultureName;
			loc = location;
		}
		
		public UltimateResourceFallbackLocation Location {
			get {
				return loc;
			}  
		}
#endif
	}
}
