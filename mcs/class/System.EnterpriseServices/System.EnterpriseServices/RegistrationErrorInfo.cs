// 
// System.EnterpriseServices.RegistrationErrorInfo.cs
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

using System;

namespace System.EnterpriseServices {
	[Serializable]
	public sealed class RegistrationErrorInfo {

		#region Fields
#pragma warning disable 649
		int errorCode;
		string errorString;
		string majorRef;
		string minorRef;
		string name;
#pragma warning restore 649

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		internal RegistrationErrorInfo (string name, string majorRef, string minorRef, int errorCode) 
		{
			this.name = name;
			this.majorRef = majorRef;
			this.minorRef = minorRef;
			this.errorCode = errorCode;
		}

		#endregion // Constructors

		#region Properties

		public int ErrorCode {
			get { return errorCode; }
		}

		public string ErrorString {
			get { return errorString; }
		}

		public string MajorRef {
			get { return majorRef; }
		}

		public string MinorRef {
			get { return minorRef; }
		}

		public string Name {
			get { return name; }
		}

		#endregion // Properties
	}
}
