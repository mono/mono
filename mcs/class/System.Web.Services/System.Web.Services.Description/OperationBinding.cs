// 
// System.Web.Services.Description.OperationBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
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
	
		public Binding Binding {
			get { return binding; }
		}

		public ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		public FaultBindingCollection Faults {
			get { return faults; }
		}

		public InputBinding Input {
			get { return input; }
			set { input = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

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
