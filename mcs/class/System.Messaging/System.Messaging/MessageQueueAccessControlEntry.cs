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
	public class MessageQueueAccessControlEntry: AccessControlEntry 
	{
		[MonoTODO]
		public MessageQueueAccessControlEntry(Trustee trustee, MessageQueueAccessRights rights)
		{
		}
		
		[MonoTODO]
		public MessageQueueAccessControlEntry(Trustee trustee, MessageQueueAccessRights rights, AccessControlEntryType entryType)
		{
		}
		
		public MessageQueueAccessRights MessageQueueAccessRights {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		~MessageQueueAccessControlEntry()
		{
		}
	}
}
