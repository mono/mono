//
// TlsException.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
using System.Text;
using System.Runtime.Serialization;

namespace Mono.Security.Interface
{
	public sealed class TlsException : Exception
	{
		#region Fields

		private Alert alert;

		#endregion

		#region Properties

		public Alert Alert {
			get { return this.alert; }
		}

		#endregion

		#region Constructors

		public TlsException (Alert alert)
			: this (alert, alert.Description.ToString())
		{
		}

		public TlsException (Alert alert, string message)
			: base (message)
		{
			this.alert = alert;
		}

		public TlsException (AlertLevel level, AlertDescription description)
			: this (new Alert (level, description))
		{
		}

		public TlsException (AlertDescription description)
			: this (new Alert (description))
		{
		}

		public TlsException (AlertDescription description, string message)
			: this (new Alert (description), message)
		{
		}

		public TlsException (AlertDescription description, string format, params object[] args)
			: this (new Alert (description), string.Format (format, args))
		{
		}

		#endregion
	}
}
