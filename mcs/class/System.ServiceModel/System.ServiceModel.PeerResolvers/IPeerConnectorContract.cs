using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Description;

namespace System.ServiceModel.PeerResolvers
{
	internal class Consts
	{
		public const string Namespace = "http://schemas.microsoft.com/net/2006/05/peer/resolver";
	}

	[ServiceContract (Name = "IPeerConnectorContract", Namespace = Consts.Namespace, SessionMode = SessionMode.Allowed)]
	internal interface IPeerConnectorContract
	{
		[OperationContract (IsOneWay = true)]
		void Connect (ConnectInfo connect);

		[OperationContract (IsOneWay = true)]
		void Welcome (WelcomeInfo welcome);

		[OperationContract (IsOneWay = true)]
		void Refuse (RefuseInfo refuse);

		[OperationContract (IsOneWay = true)]
		void Disconnect (DisconnectInfo disconnect);

		[OperationContract (IsOneWay = true)]
		void LinkUtility (LinkUtilityInfo linkUtility);

		[OperationContract (IsOneWay = true)]
		void Ping ();

		[OperationContract (IsOneWay = true)]
		void SendMessage (Message msg);
	}

	// Common datatype

	[DataContract (Name = "Referral", Namespace = Consts.Namespace)]
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

	[DataContract (Name = "Connect", Namespace = Consts.Namespace)]
	internal class ConnectInfoDC
	{
		[DataMember]
		public PeerNodeAddress PeerNodeAddress { get; set; }
		[DataMember]
		public ulong NodeId { get; set; }
	}

	[MessageContract]
	internal class ConnectInfo
	{
		public ConnectInfo ()
		{
			dc = new ConnectInfoDC ();
		}

		[MessageBodyMember]
		ConnectInfoDC dc;

		public PeerNodeAddress PeerNodeAddress {
			get { return dc.PeerNodeAddress; }
			set { dc.PeerNodeAddress = value; }
		}

		public ulong NodeId {
			get { return dc.NodeId; }
			set { dc.NodeId = value; }
		}
	}

	// Welcome

	[DataContract (Name = "Welcome", Namespace = Consts.Namespace)]
	internal class WelcomeInfoDC
	{
		[DataMember]
		public ulong NodeId { get; set; }
		[DataMember]
		public Referral [] Referrals { get; set; }
	}

	[MessageContract]
	internal class WelcomeInfo
	{
		public WelcomeInfo ()
		{
			dc = new WelcomeInfoDC ();
		}

		[MessageBodyMember]
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

	[DataContract (Name = "Refuse", Namespace = Consts.Namespace)]
	internal class RefuseInfoDC
	{
		[DataMember]
		public Referral [] Referrals { get; set; }
		[DataMember]
		public RefuseReason Reason { get; set; }
	}

	[MessageContract]
	internal class RefuseInfo
	{
		public RefuseInfo ()
		{
			dc = new RefuseInfoDC ();
		}

		[MessageBodyMember]
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

	[DataContract (Name = "Disconnect", Namespace = Consts.Namespace)]
	internal class DisconnectInfoDC
	{
		[DataMember]
		public Referral [] Referrals { get; set; }
		[DataMember]
		public DisconnectReason Reason { get; set; }
	}

	[MessageContract]
	internal class DisconnectInfo
	{
		public DisconnectInfo ()
		{
			dc = new DisconnectInfoDC ();
		}

		[MessageBodyMember]
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

	[DataContract (Name = "LinkUtilityInfo", Namespace = Consts.Namespace)]
	internal class LinkUtilityInfoDC
	{
		[DataMember]
		public uint Total { get; set; }
		[DataMember]
		public uint Useful { get; set; }
	}

	[MessageContract]
	internal class LinkUtilityInfo
	{
		public LinkUtilityInfo ()
		{
			dc = new LinkUtilityInfoDC ();
		}

		[MessageBodyMember (Name = "LinkUtilityInfo")]
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
