 // 
// System.Web.Services.WebServiceAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class WebServiceAttribute : Attribute {

		#region Fields

		public const string DefaultNamespace = "http://tempuri.org/";
		string description;
		string name;
		string ns;

		#endregion // Fields

		#region Constructors

		
		public WebServiceAttribute ()
		{
			description = String.Empty;
			name = String.Empty;
			ns = DefaultNamespace;
		}
		
		#endregion // Constructors

		#region Properties

		public string Description { 	
			get { return description; }
			set { description = value; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		#endregion // Properties
	}
}
