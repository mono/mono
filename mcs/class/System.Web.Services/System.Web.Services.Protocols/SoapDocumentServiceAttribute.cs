// 
// System.Web.Services.Protocols.SoapDocumentServiceAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Description;

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Class)]
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
