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
	
	[AttributeUsage(
		AttributeTargets.Assembly | 
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Constructor |
 		AttributeTargets.Method | 
		AttributeTargets.Event)]
	[Serializable]
	public class MessageQueuePermissionAttribute: CodeAccessSecurityAttribute 
	{
		[MonoTODO]
		public MessageQueuePermissionAttribute(SecurityAction action):base(action)
		{
		}
		
		public string Category 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public string Label 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public string MachineName 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public string Path 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public MessageQueuePermissionAccess PermissionAccess 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		public override IPermission CreatePermission()
		{
			throw new NotImplementedException();
		}
	}
}
