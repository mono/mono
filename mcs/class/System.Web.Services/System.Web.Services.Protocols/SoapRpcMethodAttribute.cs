// 
// System.Web.Services.Protocols.SoapRpcMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class SoapRpcMethodAttribute : Attribute {

		#region Fields

		string action;
		string binding;
		bool oneWay;
		string requestElementName;
		string requestNamespace;
		string responseElementName;
		string responseNamespace;

		#endregion // Fields

		#region Constructors

		public SoapRpcMethodAttribute ()
		{
		}

		public SoapRpcMethodAttribute (string action)
			: this ()
		{
			this.action = action;
		}

		#endregion // Constructors

		#region Properties

		public string Action {
			get { return action != null ? action : ""; }
			set { action = value; }
		}

		public string Binding {
			get { return binding; }
			set { binding = value; }
		}

		public bool OneWay {
			get { return oneWay; }
			set { oneWay = value; }
		}

		public string RequestElementName {
			get { return requestElementName != null ? requestElementName : ""; }
			set { requestElementName = value; }
		}

		public string RequestNamespace {
			get { return requestNamespace != null ? requestNamespace : ""; }
			set { requestNamespace = value; }
		}

		public string ResponseElementName {
			get { return responseElementName != null ? responseElementName : ""; }
			set { responseElementName = value; }
		}

		public string ResponseNamespace {
			get { return responseNamespace != null ? responseNamespace : ""; }
			set { responseNamespace = value; }
		}

		#endregion // Properties
	}
}
