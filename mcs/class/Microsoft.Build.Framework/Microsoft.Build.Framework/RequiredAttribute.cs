//
// RequiredAttribute.cs: Defines the metadata attribute that task author use to
// identify required task parameters. 
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;

namespace Microsoft.Build.Framework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
	public sealed class RequiredAttribute : Attribute
	{
		// It should stop build process if property is of non-array type
		// and is null
		// It should pass empty table if property is of array type
		// and is null
		public RequiredAttribute ()
		{
		}
	}
}

#endif