//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	[Serializable]
	public class MessageQueuePermissionEntry 
	{		
		[MonoTODO]
		public MessageQueuePermissionEntry(MessageQueuePermissionAccess permissionAccess, string path)
		{
		}
		
		[MonoTODO]
		public MessageQueuePermissionEntry(MessageQueuePermissionAccess permissionAccess, string machineName, string label, string category)
		{
		}
		
		public string Category 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		public string Label 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		public string MachineName 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		public string Path 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		
		public MessageQueuePermissionAccess PermissionAccess 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		~MessageQueuePermissionEntry()
		{
		}
	}
}
