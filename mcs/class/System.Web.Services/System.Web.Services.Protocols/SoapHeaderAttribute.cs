// 
// System.Web.Services.Protocols.SoapHeaderAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Method)]
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

		public bool Required {
			get { return required; }
			set { required = value; }
		}

		#endregion // Properties
	}
}
