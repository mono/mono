 // 
// System.Web.Services.WebServiceBindingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services {
	[AttributeUsage(AttributeTargets.Class)]
	public class WebServiceBindingAttribute : Attribute {

		#region Fields

		string location;
		string name;
		string ns;

		#endregion // Fields

		#region Constructors

		
		public WebServiceBindingAttribute ()
		{
			location = String.Empty; // FIXME
			name = String.Empty; // FIXME
			ns = "http://tempuri.org/"; // .NET reference
		}

		public WebServiceBindingAttribute (string name)
			: this ()
		{
			this.name = name;
		}

		public WebServiceBindingAttribute (string name, string ns)
			: this ()
		{
			this.name = name;
			this.ns = ns;
		}

		public WebServiceBindingAttribute (string name, string ns, string location)
			: this ()
		{
			this.name = name;
			this.ns = ns;
			this.location = location;
		}
		
		#endregion // Constructors

		#region Properties

		public string Location { 	
			get { return location; }
			set { location = value; }
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
