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
using System.Drawing;

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

		[DefaultValue (false)]
		public bool Authenticate {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (0)]
		public short BasePriority {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[TypeConverter (typeof(GuidConverter))]
		public Guid Category {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (EncryptionRequired.None)]
		public EncryptionRequired EncryptionRequired {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue ("")]
		public string Label {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[TypeConverter (typeof(SizeConverter))]
		public long MaximumJournalSize {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[TypeConverter (typeof(SizeConverter))]
		public long MaximumQueueSize {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[Editor ("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Path {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AccessControlList Permissions {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		public bool Transactional {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (UninstallAction.Remove)]
		public UninstallAction UninstallAction {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		public bool UseJournalQueue {
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
	}
}
