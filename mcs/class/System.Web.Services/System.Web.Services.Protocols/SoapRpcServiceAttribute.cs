// 
// System.Web.Services.Protocols.SoapRpcServiceAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Class, Inherited = true)]
	public sealed class SoapRpcServiceAttribute : Attribute {

		#region Fields

		SoapServiceRoutingStyle routingStyle;

		#endregion // Fields

		#region Constructors

		public SoapRpcServiceAttribute ()
		{
			routingStyle = SoapServiceRoutingStyle.SoapAction;
		}

		#endregion // Constructors

		#region Properties

		public SoapServiceRoutingStyle RoutingStyle {
			get { return routingStyle; }
			set { routingStyle = value; }
		}

		#endregion // Properties
	}
}
