// 
// IPeerResolverContract.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

namespace System.ServiceModel.PeerResolvers
{
	[ServiceContract (Name = "IPeerResolverContract", Namespace = "http://schemas.microsoft.com/net/2006/05/peer/resolver", SessionMode = SessionMode.Allowed)]
	public interface IPeerResolverContract
	{
		[OperationContract (IsOneWay = false, Name = "GetServiceInfo", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/GetServiceSettings", 
		                    ReplyAction = "http://schemas.microsoft.com/net/2006/05/peer/resolver/GetServiceSettingsResponse")]
		ServiceSettingsResponseInfo GetServiceSettings ();
		[OperationContract (IsOneWay = false, Name = "Refresh", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/Refresh", 
		                    ReplyAction = "http://schemas.microsoft.com/net/2006/05/peer/resolver/RefreshResponse")]
		RefreshResponseInfo Refresh (RefreshInfo refreshInfo);
		[OperationContract (IsOneWay = false, Name = "Register", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/Register", 
		                    ReplyAction = "http://schemas.microsoft.com/net/2006/05/peer/resolver/RegisterResponse")]
		RegisterResponseInfo Register (RegisterInfo registerInfo);
		[OperationContract (IsOneWay = false, Name = "Resolve", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/Resolve", 
		                    ReplyAction = "http://schemas.microsoft.com/net/2006/05/peer/resolver/ResolveResponse")]
		ResolveResponseInfo Resolve (ResolveInfo resolveInfo);
		[OperationContract (IsOneWay = false, Name = "Unregister", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/Unregister")]
		void Unregister (UnregisterInfo unregisterInfo);
		[OperationContract (IsOneWay = false, Name = "Update", 
		                    Action = "http://schemas.microsoft.com/net/2006/05/peer/resolver/Update", 
		                    ReplyAction = "http://schemas.microsoft.com/net/2006/05/peer/resolver/UpdateResponse")]
		RegisterResponseInfo Update (UpdateInfo updateInfo);
	}
}
