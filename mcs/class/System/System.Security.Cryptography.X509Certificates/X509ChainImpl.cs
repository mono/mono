//
// X509ChainImpl.cs
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// Copyright (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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
#if SECURITY_DEP

namespace System.Security.Cryptography.X509Certificates
{
	internal abstract class X509ChainImpl : IDisposable
	{
		public abstract bool IsValid {
			get;
		}

		public abstract IntPtr Handle {
			get;
		}

		protected void ThrowIfContextInvalid ()
		{
			if (!IsValid)
				throw X509Helper2.GetInvalidChainContextException ();
		}

		public abstract X509ChainElementCollection ChainElements {
			get;
		}

		public abstract X509ChainPolicy ChainPolicy {
			get; set;
		}

		public abstract X509ChainStatus[] ChainStatus {
			get;
		}

		public abstract bool Build (X509Certificate2 certificate);

		public abstract void AddStatus (X509ChainStatusFlags errorCode);

		public abstract void Reset ();

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		~X509ChainImpl ()
		{
			Dispose (false);
		}
	}
}

#endif
