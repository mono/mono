// 
// System.Web.Services.Description.OperationBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
	public sealed class OperationBinding : DocumentableItem {

		#region Fields

		Binding binding;
		ServiceDescriptionFormatExtensionCollection extensions;
		FaultBindingCollection faults;
		InputBinding input;
		string name;
		OutputBinding output;

		#endregion // Fields

		#region Constructors
		
		public OperationBinding ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			faults = new FaultBindingCollection (this);
			input = null;
			name = String.Empty;
			output = null;
		}
		
		#endregion // Constructors

		#region Properties
	
		[XmlIgnore]
		public Binding Binding {
			get { return binding; }
		}

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		[XmlElement ("fault")]
		public FaultBindingCollection Faults {
			get { return faults; }
		}

		[XmlElement ("input")]
		public InputBinding Input {
			get { return input; }
			set { input = value; }
		}

		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlElement ("output")]
		public OutputBinding Output {
			get { return output; }
			set { output= value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Binding binding) 
		{
			this.binding = binding; 
		} 

		#endregion
	}
}
