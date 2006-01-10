//
// System.Web.Compilation.BuildProviderAppliesTo
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Compilation {

	[Flags]
	public enum BuildProviderAppliesTo
	{
		Web = 1,            /* generate code from files in directories *other* than App_Code,
				       App_LocalResources, and App_GlobalResources */
		Code = 1 << 1,      /* generate code only for App_Code/* */
		Resources = 1 << 2, /* generate code only for App_LocalResources/* and App_GlobalResources/* */
		All = 7             /* generate code for everything.  default value for the attribute */
	}
}

#endif
