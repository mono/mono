
namespace Proxy.MonoTests.Features.Client
{
	using System.Runtime.Serialization;
	using System;


	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.Runtime.Serialization", "3.0.0.0")]
	[System.Runtime.Serialization.DataContractAttribute (Name = "Point2D", Namespace = "http://MonoTests.Features.Contracts")]
	[System.SerializableAttribute ()]
	[System.Runtime.Serialization.KnownTypeAttribute (typeof (AdvPoint2D))]
	public partial class Point2D : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
	{

		[System.NonSerializedAttribute ()]
		private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

		[System.Runtime.Serialization.OptionalFieldAttribute ()]
		private int XField;

		[System.Runtime.Serialization.OptionalFieldAttribute ()]
		private int YField;

		[global::System.ComponentModel.BrowsableAttribute (false)]
		public System.Runtime.Serialization.ExtensionDataObject ExtensionData
		{
			get
			{
				return this.extensionDataField;
			}
			set
			{
				this.extensionDataField = value;
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute ()]
		public int X
		{
			get
			{
				return this.XField;
			}
			set
			{
				if ((this.XField.Equals (value) != true)) {
					this.XField = value;
					this.RaisePropertyChanged ("X");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute ()]
		public int Y
		{
			get
			{
				return this.YField;
			}
			set
			{
				if ((this.YField.Equals (value) != true)) {
					this.YField = value;
					this.RaisePropertyChanged ("Y");
				}
			}
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged (string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if ((propertyChanged != null)) {
				propertyChanged (this, new System.ComponentModel.PropertyChangedEventArgs (propertyName));
			}
		}
	}

	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.Runtime.Serialization", "3.0.0.0")]
	[System.Runtime.Serialization.DataContractAttribute (Name = "AdvPoint2D", Namespace = "http://MonoTests.Features.Contracts")]
	[System.SerializableAttribute ()]
	public partial class AdvPoint2D : Point2D
	{

		[System.Runtime.Serialization.OptionalFieldAttribute ()]
		private double ZeroDistanceField;

		[System.Runtime.Serialization.DataMemberAttribute ()]
		public double ZeroDistance
		{
			get
			{
				return this.ZeroDistanceField;
			}
			set
			{
				if ((this.ZeroDistanceField.Equals (value) != true)) {
					this.ZeroDistanceField = value;
					this.RaisePropertyChanged ("ZeroDistance");
				}
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	[System.ServiceModel.ServiceContractAttribute (Namespace = "http://MonoTests.Features.Contracts", ConfigurationName = "IKnownTypeTesterContract")]
	public interface IKnownTypeTesterContract
	{

		[System.ServiceModel.OperationContractAttribute (Action = "http://MonoTests.Features.Contracts/IKnownTypeTesterContract/Move", ReplyAction = "http://MonoTests.Features.Contracts/IKnownTypeTesterContract/MoveResponse")]
		Point2D Move (Point2D point, Point2D delta);

		[System.ServiceModel.OperationContractAttribute (Action = "http://MonoTests.Features.Contracts/IKnownTypeTesterContract/Distance", ReplyAction = "http://MonoTests.Features.Contracts/IKnownTypeTesterContract/DistanceResponse")]
		double Distance (Point2D point1, Point2D point2);
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public interface IKnownTypeTesterContractChannel : IKnownTypeTesterContract, System.ServiceModel.IClientChannel
	{
	}

	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public partial class KnownTypeTesterContractClient : System.ServiceModel.ClientBase<IKnownTypeTesterContract>, IKnownTypeTesterContract
	{

		public KnownTypeTesterContractClient ()
		{
		}

		public KnownTypeTesterContractClient (string endpointConfigurationName) :
			base (endpointConfigurationName)
		{
		}

		public KnownTypeTesterContractClient (string endpointConfigurationName, string remoteAddress) :
			base (endpointConfigurationName, remoteAddress)
		{
		}

		public KnownTypeTesterContractClient (string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
			base (endpointConfigurationName, remoteAddress)
		{
		}

		public KnownTypeTesterContractClient (System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
			base (binding, remoteAddress)
		{
		}

		public Point2D Move (Point2D point, Point2D delta)
		{
			return base.Channel.Move (point, delta);
		}

		public double Distance (Point2D point1, Point2D point2)
		{
			return base.Channel.Distance (point1, point2);
		}
	}
}
