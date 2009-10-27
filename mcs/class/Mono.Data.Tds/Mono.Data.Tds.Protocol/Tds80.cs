//
// Mono.Data.Tds.Protocol.Tds80.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
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

using Mono.Data.Tds;
using System;

namespace Mono.Data.Tds.Protocol {
        public sealed class Tds80 : Tds
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds80;

		#endregion // Fields

		#region Constructors

		public Tds80 (string server, int port)
			: this (server, port, 512, 15)
		{
		}

		public Tds80 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
		}

		#endregion // Constructors

		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			throw new NotImplementedException ();
		}

		protected override void ProcessColumnInfo ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
