// 
// System.Web.Services.Protocols.SoapDocumentServiceAttribute.cs
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

using System.Web.Services.Description;

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Class, Inherited = true)]
	public sealed class SoapDocumentServiceAttribute : Attribute {

		#region Fields

		SoapParameterStyle paramStyle;
		SoapServiceRoutingStyle routingStyle;
		SoapBindingUse use;

		#endregion

		#region Constructors

		public SoapDocumentServiceAttribute () 
		{
			paramStyle = SoapParameterStyle.Wrapped;
			routingStyle = SoapServiceRoutingStyle.SoapAction;
			use = SoapBindingUse.Literal;
		}

		public SoapDocumentServiceAttribute (SoapBindingUse use) 
			: this ()
		{
			this.use = use;
		}

		public SoapDocumentServiceAttribute (SoapBindingUse use, SoapParameterStyle paramStyle) 
			: this ()
		{
			this.use = use;
			this.paramStyle = paramStyle;
		}
		
		#endregion // Constructors

		#region Properties

		public SoapParameterStyle ParameterStyle {
			get { return paramStyle; }
			set { paramStyle = value; }
		}

		public SoapServiceRoutingStyle RoutingStyle {
			get { return routingStyle; }
			set { routingStyle = value; }
		}

		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
