//
// DynamicAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009, Novell, Inc (http://www.novell.com)
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

#if NET_4_0 || BOOTSTRAP_NET_4_0 || MOONLIGHT

using System;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public sealed class DynamicAttribute : Attribute
	{
		static readonly IList<bool> empty = Array.AsReadOnly (new [] { true });
		
		IList<bool> transformFlags;

		public DynamicAttribute ()
		{
			transformFlags = empty;
		}

		public DynamicAttribute (bool[] transformFlags)
		{
			if (transformFlags == null)
				throw new ArgumentNullException ();

			this.transformFlags = transformFlags;
		}

		public IList<bool> TransformFlags {
			get {
				return transformFlags;
			}
		}
	}
}

#endif
