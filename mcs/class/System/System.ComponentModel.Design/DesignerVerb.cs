// System.ComponentModel.Design.DesignerVerb.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class DesignerVerb : MenuCommand
	{
		[MonoTODO]
		public DesignerVerb (string text, EventHandler handler) : base (handler, new CommandID (text)){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DesignerVerb (string text, EventHandler handler, CommandID startCommandID) 
			: base (handler, startCommandID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string Text 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
	}	
}
