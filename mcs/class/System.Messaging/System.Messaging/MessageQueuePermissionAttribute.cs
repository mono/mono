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
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class|
		//                AttributeTargets.Struct | AttributeTargets.Constructor |AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		[MonoTODO]
		public MessageQueuePermissionAttribute(SecurityAction action):base(action)
		{
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class| AttributeTargets.Struct | AttributeTargets.Constructor |
		//                AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		public string Category 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
		//                | AttributeTargets.Struct | AttributeTargets.Constructor |
		//AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		public string Label 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
		//                | AttributeTargets.Struct | AttributeTargets.Constructor |
		//                AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		public string MachineName 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
		//                | AttributeTargets.Struct | AttributeTargets.Constructor |
		//                AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		public string Path 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
		//                | AttributeTargets.Struct | AttributeTargets.Constructor |
		//                AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		public MessageQueuePermissionAccess PermissionAccess 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		//[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
		//                AttributeTargets.Struct | AttributeTargets.Constructor |
		//                AttributeTargets.Method | AttributeTargets.Event)]
		//[Serializable]
		[MonoTODO]
		public override IPermission CreatePermission()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		~MessageQueuePermissionAttribute()
		{
		}
	}
}
