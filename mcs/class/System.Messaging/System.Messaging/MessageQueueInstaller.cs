//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace System.Messaging 
{
	public class MessageQueueInstaller: ComponentInstaller 
	{
		[MonoTODO]
		public MessageQueueInstaller()
		{
		}
		
		[MonoTODO]
		public MessageQueueInstaller(MessageQueue componentToCopy)
		{
			throw new NotImplementedException();
		}
		
		public bool Authenticate 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public short BasePriority 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public Guid Category 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public EncryptionRequired EncryptionRequired 
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
		
		public long MaximumJournalSize 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public long MaximumQueueSize 
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
		
		public AccessControlList Permissions 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public bool Transactional 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public UninstallAction UninstallAction 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		public bool UseJournalQueue 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		public override void Commit(IDictionary savedState)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override void CopyFromComponent(IComponent component)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override void Install(IDictionary stateSaver)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override void Rollback(IDictionary savedState)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override void Uninstall(IDictionary savedState)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		~MessageQueueInstaller()
		{
		}
	}
}
