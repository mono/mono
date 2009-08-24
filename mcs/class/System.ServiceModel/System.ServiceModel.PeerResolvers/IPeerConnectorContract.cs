using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Description;

namespace System.ServiceModel.PeerResolvers
{
	[ServiceContract (Namespace = Constants.NetPeer, SessionMode = SessionMode.Allowed, CallbackContract = typeof (IPeerConnectorContract))]
	internal interface IPeerConnectorContract
	{
		[OperationContract (Action = Constants.NetPeer + "/Connect", IsOneWay = true)]
		void Connect (ConnectInfo connect);

		[OperationContract (Action = Constants.NetPeer + "/Welcome", IsOneWay = true)]
		void Welcome (WelcomeInfo welcome);

		[OperationContract (Action = Constants.NetPeer + "/Refuse", IsOneWay = true)]
		void Refuse (RefuseInfo refuse);

		[OperationContract (Action = Constants.NetPeer + "/Disconnect", IsOneWay = true)]
		void Disconnect (DisconnectInfo disconnect);

		[OperationContract (Action = Constants.NetPeer + "/LinkUtility", IsOneWay = true)]
		void LinkUtility (LinkUtilityInfo linkUtility);

		[OperationContract (Action = Constants.NetPeer + "/Ping", IsOneWay = true)]
		void Ping ();

		[OperationContract (Action = "*", IsOneWay = true)]
		void SendMessage (Message msg);
	}

	// Common datatype

	[DataContract (Name = "Referral", Namespace = Constants.NetPeer)]
	internal class Referral
	{
		[DataMember]
		public ulong Id { get; set; }

		[DataMember]
		public PeerNodeAddress PeerNodeAddress { get; set; }
	}

	internal enum RefuseReason
	{
		DuplicateNeighbor,
		DuplicateNodeId,
		NodeBusy,
	}

	internal enum DisconnectReason
	{
		LeavingMesh,
		NotUsefulNeighbor,
		DuplicateNeighbor,
		DuplicateNodeId,
	}

	// Connect

	[DataContract (Name = "Connect", Namespace = Constants.NetPeer)]
	internal class ConnectInfoDC
	{
		[DataMember]
		public PeerNodeAddress Address { get; set; }
		[DataMember]
		public ulong NodeId { get; set; }
	}

	[MessageContract (IsWrapped = false)]
	internal class ConnectInfo
	{
		public ConnectInfo ()
		{
			dc = new ConnectInfoDC ();
		}

		[MessageBodyMember (Name = "Connect", Namespace = Constants.NetPeer)]
		ConnectInfoDC dc;

		public PeerNodeAddress Address {
			get { return dc.Address; }
			set { dc.Address = value; }
		}

		public ulong NodeId {
			get { return dc.NodeId; }
			set { dc.NodeId = value; }
		}
	}

	// Welcome

	[DataContract (Name = "Welcome", Namespace = Constants.NetPeer)]
	internal class WelcomeInfoDC
	{
		[DataMember]
		public ulong NodeId { get; set; }
		[DataMember]
		public Referral [] Referrals { get; set; }
	}

	[MessageContract (IsWrapped = false)]
	internal class WelcomeInfo
	{
		public WelcomeInfo ()
		{
			dc = new WelcomeInfoDC ();
		}

		[MessageBodyMember (Name = "Welcome", Namespace = Constants.NetPeer)]
		WelcomeInfoDC dc;

		public ulong NodeId {
			get { return dc.NodeId; }
			set { dc.NodeId = value; }
		}

		public Referral [] Referrals {
			get { return dc.Referrals; }
			set { dc.Referrals = value; }
		}
	}

	// Refuse

	[DataContract (Name = "Refuse", Namespace = Constants.NetPeer)]
	internal class RefuseInfoDC
	{
		[DataMember]
		public Referral [] Referrals { get; set; }
		[DataMember]
		public RefuseReason Reason { get; set; }
	}

	[MessageContract (IsWrapped = false)]
	internal class RefuseInfo
	{
		public RefuseInfo ()
		{
			dc = new RefuseInfoDC ();
		}

		[MessageBodyMember (Name = "Refuse", Namespace = Constants.NetPeer)]
		RefuseInfoDC dc;

		public Referral [] Referrals {
			get { return dc.Referrals; }
			set { dc.Referrals = value; }
		}

		public RefuseReason Reason {
			get { return dc.Reason; }
			set { dc.Reason = value; }
		}
	}

	// Disconnect

	[DataContract (Name = "Disconnect", Namespace = Constants.NetPeer)]
	internal class DisconnectInfoDC
	{
		[DataMember]
		public Referral [] Referrals { get; set; }
		[DataMember]
		public DisconnectReason Reason { get; set; }
	}

	[MessageContract (IsWrapped = false)]
	internal class DisconnectInfo
	{
		public DisconnectInfo ()
		{
			dc = new DisconnectInfoDC ();
		}

		[MessageBodyMember (Name = "Disconnect", Namespace = Constants.NetPeer)]
		DisconnectInfoDC dc;

		public Referral [] Referrals {
			get { return dc.Referrals; }
			set { dc.Referrals = value; }
		}

		public DisconnectReason Reason {
			get { return dc.Reason; }
			set { dc.Reason = value; }
		}
	}

	// LinkUtilityInfo

	[DataContract (Name = "LinkUtilityInfo", Namespace = Constants.NetPeer)]
	internal class LinkUtilityInfoDC
	{
		[DataMember]
		public uint Total { get; set; }
		[DataMember]
		public uint Useful { get; set; }
	}

	[MessageContract (IsWrapped = false)]
	internal class LinkUtilityInfo
	{
		public LinkUtilityInfo ()
		{
			dc = new LinkUtilityInfoDC ();
		}

		[MessageBodyMember (Name = "LinkUtilityInfo", Namespace = Constants.NetPeer)]
		LinkUtilityInfoDC dc;

		public uint Total {
			get { return dc.Total; }
			set { dc.Total = value; }
		}

		public uint Useful {
			get { return dc.Useful; }
			set { dc.Useful = value; }
		}
	}
}
