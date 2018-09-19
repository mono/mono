//
// OpenSSLX509VerifyParam.cs
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
	public class OpenSSLX509VerifyParam : OpenSSLObject
	{
		new internal MonoOpenSSLX509VerifyParam Instance {
			get { return (MonoOpenSSLX509VerifyParam)base.Instance; }
		}

		internal OpenSSLX509VerifyParam (MonoOpenSSLX509VerifyParam param)
			: base (param)
		{
		}

		public OpenSSLX509VerifyParam Copy ()
		{
			return new OpenSSLX509VerifyParam (Instance.Copy ());
		}

		public void SetName (string name)
		{
			Instance.SetName (name);
		}

		public void SetHost (string name)
		{
			Instance.SetHost (name);
		}

		public void AddHost (string name)
		{
			Instance.AddHost (name);
		}

		public OpenSSLX509VerifyFlags GetFlags ()
		{
			return (OpenSSLX509VerifyFlags)Instance.GetFlags ();
		}

		public void SetFlags (OpenSSLX509VerifyFlags flags)
		{
			Instance.SetFlags ((ulong)flags);
		}

		public void SetPurpose (OpenSSLX509Purpose purpose)
		{
			Instance.SetPurpose ((MonoOpenSSLX509Purpose)purpose);
		}

		public int GetDepth ()
		{
			return Instance.GetDepth ();
		}

		public void SetDepth (int depth)
		{
			Instance.SetDepth (depth);
		}

		public void SetTime (DateTime time)
		{
			Instance.SetTime (time);
		}
	}
}

