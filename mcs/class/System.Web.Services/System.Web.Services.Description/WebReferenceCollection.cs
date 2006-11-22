// 
// System.Web.Services.Description.WebReferenceCollection.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

namespace System.Web.Services.Description
{
	public sealed class WebReferenceCollection: CollectionBase
	{
		public WebReferenceCollection ()
		{
		}
		
		public int Add (WebReference webReference) {
			return List.Add (webReference);
		}
		
		public WebReference this [int index] {
			get { return (WebReference) List [index]; }
			set { List [index] = value; }
		}
		
		public bool Contains (WebReference webReference)
		{
			return List.Contains (webReference);
		}
		
		public void CopyTo (WebReference[] array, int index)
		{
			List.CopyTo (array, index);
		}
		
		public int IndexOf (WebReference webReference)
		{
			return List.IndexOf (webReference);
		}

		public void Insert (int index, WebReference webReference)
		{
			List.Insert (index, webReference);
		}
		
		public void Remove (WebReference webReference)
		{
			List.Remove (webReference);
		}
	}
}

#endif
