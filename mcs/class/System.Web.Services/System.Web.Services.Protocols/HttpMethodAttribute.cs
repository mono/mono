// 
// System.Web.Services.Protocols.HttpMethodAttribute.cs
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
	[AttributeUsage (AttributeTargets.Method, Inherited = true)]
	public sealed class HttpMethodAttribute : Attribute {

		#region Fields

		Type parameterFormatter;
		Type returnFormatter;

		#endregion

		#region Constructors

		public HttpMethodAttribute () 
		{
		}

		public HttpMethodAttribute (Type returnFormatter, Type parameterFormatter) 
			: this ()
		{
			this.parameterFormatter = parameterFormatter;
			this.returnFormatter = returnFormatter;
		}
		
		#endregion // Constructors

		#region Properties

		public Type ParameterFormatter {
			get { return parameterFormatter; }
			set { parameterFormatter = value; }
		}

		public Type ReturnFormatter {
			get { return returnFormatter; }
			set { returnFormatter = value; }
		}

		#endregion // Properties
	}
}
