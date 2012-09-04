// 
// System.Web.Services.Protocols.SoapHeaderAttribute.cs
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

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class SoapHeaderAttribute : Attribute {

		#region Fields

		SoapHeaderDirection direction;
		string memberName;
		bool required;

		#endregion // Fields

		#region Constructors

		public SoapHeaderAttribute (string memberName) 
		{
			direction = SoapHeaderDirection.In;
			this.memberName = memberName;
			required = true;
		}

		#endregion // Constructors

		#region Properties

		public SoapHeaderDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public string MemberName {	
			get { return memberName; }
			set { memberName = value; }
		}

               [Obsolete ("This property will be removed from a future"
                       + " version. The presence of a particular header"
                       + " in a SOAP message is no longer enforced", false)]
		public bool Required {
			get { return required; }
			set { required = value; }
		}

		#endregion // Properties
	}
}
