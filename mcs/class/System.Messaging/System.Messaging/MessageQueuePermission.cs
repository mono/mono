//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Security;
using System.Security.Permissions;

namespace System.Messaging 
{
	
	[Serializable]
	public class MessageQueuePermission: CodeAccessPermission, IUnrestrictedPermission 
	{
		[MonoTODO]
		public MessageQueuePermission()
		{
		}
		
		[MonoTODO]
		public MessageQueuePermission(MessageQueuePermissionEntry[] permissionAccessEntries)
		{
		}
		
		[MonoTODO]
		public MessageQueuePermission(PermissionState state)
		{
		}
		
		[MonoTODO]
		public MessageQueuePermission(MessageQueuePermissionAccess permissionAccess, string path)
		{
		}
		
		[MonoTODO]
		bool IUnrestrictedPermission.IsUnrestricted() 
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public MessageQueuePermission(MessageQueuePermissionAccess permissionAccess,
		                              string machineName,
		                              string label,
		                              string category)
		{
		}
		
		public MessageQueuePermissionEntryCollection PermissionEntries
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		public override IPermission Copy()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override void FromXml(SecurityElement securityElement)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override IPermission Intersect(IPermission target)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override bool IsSubsetOf(IPermission target)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override SecurityElement ToXml()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override IPermission Union(IPermission target)
		{
			throw new NotImplementedException();
		}
	}
}
