// 
// System.Web.Services.Description.Operation.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Web.Services.Description {
	public sealed class Operation : DocumentableItem {

		#region Fields

		OperationFaultCollection faults;
		OperationMessageCollection messages;
		string name;
		public string[] parameterOrder;
		public string parameterOrderString;
		public PortType portType;

		#endregion // Fields

		#region Constructors
		
		public Operation ()
		{
			faults = new OperationFaultCollection (this);
			messages = new OperationMessageCollection (this);
			name = String.Empty;
			parameterOrder = null;
			parameterOrderString = String.Empty;
			portType = null;
		}
		
		#endregion // Constructors

		#region Properties

		public OperationFaultCollection Faults {
			get { return faults; }
		}

		public OperationMessageCollection Messages {
			get { return messages; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string[] ParameterOrder {
			get { return parameterOrder; }
			set { parameterOrder = value; }
		}

		public string ParameterOrderString {
			get { return String.Join (" ", parameterOrder); }
			set { ParameterOrder = value.Split (' '); }
		}

		public PortType PortType {
			get { return portType; }
		}

		#endregion // Properties

		#region Methods

		public bool IsBoundBy (OperationBinding operationBinding)
		{
			return (operationBinding.Name == Name);
		}

		internal void SetParent (PortType portType)
		{
			this.portType = portType;
		}

		#endregion
	}
}
