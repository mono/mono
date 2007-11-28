//
// System.Web.RequestNotification
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
namespace System.Web 
{
	[FlagsAttribute]
	public enum RequestNotification
	{
		BeginRequest = 0x0001,
		AuthenticateRequest = 0x0002,
		AuthorizeRequest = 0x0004,
		ResolveRequestCache = 0x0008,
		MapRequestHandler = 0x0010,
		AcquireRequestState = 0x0020,
		PreExecuteRequestHandler = 0x0040,
		ExecuteRequestHandler = 0x0080,
		ReleaseRequestState = 0x0100,
		UpdateRequestCache = 0x0200,
		LogRequest = 0x0400,
		EndRequest = 0x0800,
		SendResponse = 0x20000000
	}
}
#endif