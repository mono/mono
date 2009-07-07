using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Description;

namespace System.ServiceModel.PeerResolvers
{
	[DataContract (Namespace = "http://mono-project.com/ns/2008/07/peer-resolver")]
	class PeerServiceSettingsInfo
	{
		[DataMember]
		public TimeSpan RefreshInterval { get; set; }
		[DataMember]
		public TimeSpan CleanupInterval { get; set; }
		[DataMember]
		public bool ControlMeshShape { get; set; }
	}

	[ServiceContract]
	interface ICustomPeerResolverContract : IPeerResolverContract
	{
		[OperationContract]
		PeerServiceSettingsInfo GetCustomServiceSettings ();
		[OperationContract]
		void SetCustomServiceSettings (PeerServiceSettingsInfo info);
	}

	interface ICustomPeerResolverClient : ICustomPeerResolverContract, IClientChannel
	{
	}
}
