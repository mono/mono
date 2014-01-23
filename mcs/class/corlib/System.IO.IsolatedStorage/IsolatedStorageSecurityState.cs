//
// System.IO.IsolatedStorage.IsolatedStorageSecurityState
//
// Author: 
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using System.Security;

#if NET_4_0

namespace System.IO.IsolatedStorage
{
	public class IsolatedStorageSecurityState : SecurityState {

		internal IsolatedStorageSecurityState ()
		{
		}

		public IsolatedStorageSecurityOptions Options {
			get {
				return IsolatedStorageSecurityOptions.IncreaseQuotaForApplication;
			}
		}

		public long Quota {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public long UsedSize {
			get {
				throw new NotImplementedException ();
			}
		}

		public override void EnsureState ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

