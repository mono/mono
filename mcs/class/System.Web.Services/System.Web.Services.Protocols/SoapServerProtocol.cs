// 
// System.Web.Services.Protocols.SoapServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what this class does.")]
	internal class SoapServerProtocol : ServerProtocol {

		#region Fields

		bool isOneWay;

		#endregion // Fields

		#region Properties

		public override bool IsOneWay {
			get { return isOneWay; }
		}

		[MonoTODO]
		public override LogicalMethodInfo MethodInfo {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override Exception OnewayInitException {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Initialize ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object[] ReadParameters ()
		{
				throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool WriteException (Exception e, Stream outputStream)
		{
				throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void WriteReturns (object[] returnValues, Stream outputStream)
		{
				throw new NotImplementedException ();
		}

		#endregion
	}
}
