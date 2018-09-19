//
// OpenSSLX509StoreCtx.cs
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
using System;
namespace Mono.OpenSSL.Interface
{
	public class OpenSSLX509StoreCtx : OpenSSLObject
	{
		new internal MonoOpenSSLX509StoreCtx Instance {
			get { return (MonoOpenSSLX509StoreCtx)base.Instance; }
		}

		internal OpenSSLX509StoreCtx (MonoOpenSSLX509StoreCtx ctx)
			: base (ctx)
		{
		}

		public void Initialize (OpenSSLX509Store store, OpenSSLX509Chain chain)
		{
			Instance.Initialize (store.Instance, chain.Instance);
		}

		public void SetVerifyParam (OpenSSLX509VerifyParam param)
		{
			Instance.SetVerifyParam (param.Instance);
		}

		public int Verify ()
		{
			return Instance.Verify ();
		}

		public OpenSSLX509Error GetError ()
		{
			return (OpenSSLX509Error)Instance.GetError ();
		}

		public Exception GetException ()
		{
			return Instance.GetException ();
		}

		public OpenSSLX509Chain GetChain ()
		{
			return new OpenSSLX509Chain (Instance.GetChain ());
		}
	}
}

