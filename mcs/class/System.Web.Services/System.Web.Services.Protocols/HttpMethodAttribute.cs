// 
// System.Web.Services.Protocols.HttpMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Method)]
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
