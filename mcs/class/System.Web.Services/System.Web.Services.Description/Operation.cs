// 
// System.Web.Services.Description.Operation.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class Operation : DocumentableItem {

		#region Fields

		OperationFaultCollection faults;
		OperationMessageCollection messages;
		string name;
		string[] parameterOrder;
		PortType portType;

		#endregion // Fields

		#region Constructors
		
		public Operation ()
		{
			faults = new OperationFaultCollection (this);
			messages = new OperationMessageCollection (this);
			name = String.Empty;
			parameterOrder = null;
			portType = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("fault")]
		public OperationFaultCollection Faults {
			get { return faults; }
		}

		[XmlElement ("output", typeof (OperationOutput))]
		[XmlElement ("input", typeof (OperationInput))]
		public OperationMessageCollection Messages {
			get { return messages; }
		}

		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlIgnore]
		public string[] ParameterOrder {
			get { return parameterOrder; }
			set { parameterOrder = value; }
		}

		[DefaultValue ("")]
		[XmlAttribute ("parameterOrder")]
		public string ParameterOrderString {
			get { 
				if (parameterOrder == null)
					return String.Empty;
				return String.Join (" ", parameterOrder); 
			}
			set { ParameterOrder = value.Split (' '); }
		}

		[XmlIgnore]
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
