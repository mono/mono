//
// Copyright (C) 2010 Novell Inc. http://novell.com
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

namespace System.Windows.Markup
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyWindowsBase)]
	public sealed class ContentWrapperAttribute : Attribute
	{
		public ContentWrapperAttribute (Type contentWrapper)
		{
			ContentWrapper = contentWrapper;
		}
		
		public Type ContentWrapper { get; private set; }

#if !__MOBILE__
		public override Object TypeId {
			get { return this; }
		}
#endif

		public override bool Equals (object other)
		{
			var cwa = other as ContentWrapperAttribute;
			if (cwa == null)
				return false;
			return ContentWrapper != null ? ContentWrapper == cwa.ContentWrapper : cwa.ContentWrapper == null;
		}

		public override int GetHashCode ()
		{
			return ContentWrapper != null ? ContentWrapper.GetHashCode () : 0;
		}
	}
}
