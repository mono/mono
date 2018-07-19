//
// SecurityTokenProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Selectors
{
	public abstract class SecurityTokenProvider
	{
		protected SecurityTokenProvider ()
		{
		}

		public virtual bool SupportsTokenCancellation {
			get { return false; }
		}

		public virtual bool SupportsTokenRenewal {
			get { return false; }
		}

		public SecurityToken GetToken (TimeSpan timeout)
		{
			return GetTokenCore (timeout);
		}

		public IAsyncResult BeginGetToken (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return BeginGetTokenCore (timeout, callback, state);
		}

		public SecurityToken EndGetToken (IAsyncResult result)
		{
			return EndGetTokenCore (result);
		}

		public void CancelToken (TimeSpan timeout, SecurityToken token)
		{
			CancelTokenCore (timeout, token);
		}

		public IAsyncResult BeginCancelToken (
			TimeSpan timeout, SecurityToken token,
			AsyncCallback callback, object state)
		{
			return BeginCancelTokenCore (timeout, token, callback, state);
		}

		public void EndCancelToken (IAsyncResult result)
		{
			EndCancelTokenCore (result);
		}

		public SecurityToken RenewToken (TimeSpan timeout, SecurityToken tokenToBeRenewed)
		{
			return RenewTokenCore (timeout, tokenToBeRenewed);
		}

		public IAsyncResult BeginRenewToken (
			TimeSpan timeout, SecurityToken tokenToBeRenewed,
			AsyncCallback callback, object state)
		{
			return BeginRenewTokenCore (timeout, tokenToBeRenewed, callback, state);
		}

		public SecurityToken EndRenewToken (IAsyncResult result)
		{
			return EndRenewTokenCore (result);
		}

		protected abstract SecurityToken GetTokenCore (TimeSpan timeout);

		protected virtual void CancelTokenCore (TimeSpan timeout, SecurityToken token)
		{
			throw new NotSupportedException (String.Format ("Token cancellation on this security token provider '{0}' is not supported.", this));
		}

		protected virtual SecurityToken RenewTokenCore (TimeSpan timeout, SecurityToken tokenToBeRenewed)
		{
			throw new NotSupportedException (String.Format ("Token renewal on this security token provider '{0}' is not supported.", this));
		}

		[MonoTODO]
		protected virtual IAsyncResult BeginGetTokenCore (
			TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected virtual IAsyncResult BeginCancelTokenCore (
			TimeSpan timeout,
			SecurityToken token,
			AsyncCallback callback, object state)
		{
			throw new NotSupportedException (String.Format ("Token cancellation on this security token provider '{0}' is not supported.", this));
		}

		protected virtual IAsyncResult BeginRenewTokenCore (
			TimeSpan timeout,
			SecurityToken tokenToBeRenewed,
			AsyncCallback callback, object state)
		{
			throw new NotSupportedException (String.Format ("Token renewal on this security token provider '{0}' is not supported.", this));
		}

		[MonoTODO]
		protected virtual SecurityToken EndGetTokenCore (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected virtual void EndCancelTokenCore (IAsyncResult result)
		{
			throw new NotSupportedException (String.Format ("Token cancellation on this security token provider '{0}' is not supported.", this));
		}

		protected virtual SecurityToken EndRenewTokenCore (IAsyncResult result)
		{
			throw new NotSupportedException (String.Format ("Token renewal on this security token provider '{0}' is not supported.", this));
		}
	}
}
