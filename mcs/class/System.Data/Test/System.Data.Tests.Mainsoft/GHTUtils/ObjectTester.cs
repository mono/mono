// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

namespace GHTUtils
{
	// This class implements tests for method derived from object
	public sealed class ObjectTester : GHTUtils.Base.GHTBase
	{
		public void Object_Equals(object tested, object Expected)
		{
			BeginCase("Instatnce Equal");
			Compare(tested.Equals(Expected), true);
			EndCase(null);
			
			BeginCase("Static Equal");
			Compare(object.Equals(tested, Expected), true);
			EndCase(null);		
		}
				
		public void Object_GetType(object tested, Type Expected)
		{
			BeginCase("GetType");
			Compare(tested.GetType(), Expected);
			EndCase(null);
		}

		public void Object_ToString(object tested, string Expected)
		{
			BeginCase("ToString()");
			Compare(tested.ToString(), Expected);
			EndCase(null);
		}

	}
}
