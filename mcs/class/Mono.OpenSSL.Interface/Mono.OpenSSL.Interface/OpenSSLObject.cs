﻿//
// OpenSSLObject.cs
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
	public abstract class OpenSSLObject : IDisposable
	{
		MonoOpenSSLObject instance;

		internal MonoOpenSSLObject Instance {
			get {
				if (!IsValid)
					throw new ObjectDisposedException (GetType ().Name);
				return instance;
			}
		}

		internal OpenSSLObject (MonoOpenSSLObject instance)
		{
			this.instance = instance;
		}

		public bool IsValid {
			get { return instance != null && instance.IsValid; }
		}

		protected void Dispose (bool disposing)
		{
			if (disposing) {
				if (instance != null) {
					instance.Dispose ();
					instance = null;
				}
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~OpenSSLObject ()
		{
			Dispose (false);
		}
	}
}

