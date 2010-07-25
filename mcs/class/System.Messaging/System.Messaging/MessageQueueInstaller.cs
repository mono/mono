//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
