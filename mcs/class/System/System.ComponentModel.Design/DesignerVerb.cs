//
// System.ComponentModel.Design.DesignerVerb.cs
//
// Author:
//   Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class DesignerVerb : MenuCommand
	{

		private string description;

		public DesignerVerb (string text, EventHandler handler) 
			: this (text, handler, StandardCommands.VerbFirst)
		{
		}

		public DesignerVerb (string text, EventHandler handler, CommandID startCommandID) 
			: base (handler, startCommandID) {
			this.description = text;
		}

		public string Text {
			get {
				return this.description;
			}
		}

		public override string ToString()
		{
			return string.Concat (description, base.ToString ());
		}
	}	
}
