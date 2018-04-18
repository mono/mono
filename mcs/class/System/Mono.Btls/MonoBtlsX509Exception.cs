//
// MonoBtlsX509Exception.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP && MONO_FEATURE_BTLS
using System;

namespace Mono.Btls
{
	class MonoBtlsX509Exception : Exception
	{
		public MonoBtlsX509Error ErrorCode {
			get;
			private set;
		}

		public string ErrorMessage {
			get;
			private set;
		}

		public MonoBtlsX509Exception (MonoBtlsX509Error code, string message)
			: base (message)
		{
			ErrorCode = code;
			ErrorMessage = message;
		}

		public override string ToString ()
		{
			return string.Format ("[MonoBtlsX509Exception: ErrorCode={0}, ErrorMessage={1}]", ErrorCode, ErrorMessage);
		}
	}
}
#endif
