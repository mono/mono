//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//				Joel Basson			(jstrike@mweb.co.za)
//				Alberto Fernandez	(infjaf00@yahoo.es)
//
//
using System;
using System.Drawing;
using System.ComponentModel;

using Gtk;
using GtkSharp;

namespace System.Windows.Forms{
	
	[MonoTODO]
	public class MessageBox{

		static MessageBox (){
			Gtk.Application.Init();
		}
		private MessageBox (){
		}

		public static DialogResult Show (String text) {
			return Show ((IWin32Window) null, text, "");
		}
		public static DialogResult Show (IWin32Window owner, String text){
			return Show (owner, text, "");
		}

		public static DialogResult Show (String text, String caption) {
			return Show ((IWin32Window) null, text, caption);			
		}
		public static DialogResult Show (IWin32Window owner, String text, String caption){
		
			Gtk.Window w = (owner == null) ? null : (owner as Form).win;
			
			Gtk.MessageDialog dialog = new Gtk.MessageDialog(
				w, 
				Gtk.DialogFlags.DestroyWithParent, 
				Gtk.MessageType.Info, 
				Gtk.ButtonsType.Ok, 
				text);	
			dialog.Title = caption; 		
			dialog.Run();
			dialog.Destroy();
			
			return DialogResult.OK;
		}
		
		
		
		public static DialogResult Show (
			String text,
			String caption, 
			MessageBoxButtons buttons){
			
			return Show (
				(IWin32Window) null, 
				text, 
				caption, 
				buttons);
		}
		public static DialogResult Show (
			IWin32Window owner, 
			string text, 
			string caption, 
			MessageBoxButtons buttons){			
			
			return Show (
				owner,
				text,
				caption,
				buttons,
				MessageBoxIcon.None); 
		}
		public static DialogResult Show (
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon){
			
			return Show (
				(IWin32Window) null,
				text,
				caption,
				buttons,
				icon);
		}
		
		public static DialogResult Show (
			IWin32Window owner, 
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon){
			
			return Show (
				owner,
				text,
				caption,
				buttons,
				icon,
				MessageBoxDefaultButton.Button1);
		}
		
		public static DialogResult Show (
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton){
			
			return Show (
				(IWin32Window) null,
				text,
				caption,
				buttons,
				icon,
				defaultButton);		
		}
		
		public static DialogResult Show (
			IWin32Window owner, 
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton){
			
			return Show (
				owner,
				text,
				caption,
				buttons,
				icon,
				defaultButton,
				MessageBoxOptions.DefaultDesktopOnly);
		}
		
		public static DialogResult Show (
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton, 
			MessageBoxOptions options){
			
			return Show (
				(IWin32Window) null,
				text,
				caption,
				buttons,
				icon,
				defaultButton,
				options);			
		}
		
		public static DialogResult Show (
			IWin32Window owner, 
			string text, 
			string caption, 
			MessageBoxButtons buttons, 
			MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton, 
			MessageBoxOptions options){
			
			if (! Enum.IsDefined (typeof(MessageBoxButtons), buttons)){
				throw new InvalidEnumArgumentException("buttons");
			}
			if (! Enum.IsDefined (typeof (MessageBoxIcon), icon)){
				throw new InvalidEnumArgumentException ("icon");
			}
			if (! Enum.IsDefined (typeof (MessageBoxDefaultButton), defaultButton)){
				throw new InvalidEnumArgumentException ("defaultButton");
			}
			
			//ArgumentException - options contiene tanto DefaultDesktopOnly como ServiceNotification o

			
			Gtk.MessageType mType = Gtk.MessageType.Info;
			
			switch (icon){
				case MessageBoxIcon.Information:
							mType = Gtk.MessageType.Info;
							break;
				
				case MessageBoxIcon.Question:
							mType = Gtk.MessageType.Question;
							break;
				
				case MessageBoxIcon.Warning:
							mType = Gtk.MessageType.Warning;
							break;
				
				case MessageBoxIcon.Error:
							mType = Gtk.MessageType.Error;
							break;
				default:
							mType = Gtk.MessageType.Info;
							break;
			}
			
			Gtk.MessageDialog dialog = null;
			Gtk.Window w = (owner == null) ? null : (owner as Form).win;
			
			switch (buttons){
				case MessageBoxButtons.OK:
					dialog = new Gtk.MessageDialog (
						w,
						Gtk.DialogFlags.DestroyWithParent,
						mType,
						Gtk.ButtonsType.Ok, 
						text);
					break;
				case MessageBoxButtons.OKCancel:
					dialog = new Gtk.MessageDialog(
						w, 
						Gtk.DialogFlags.DestroyWithParent, 
						mType,
						Gtk.ButtonsType.OkCancel, 
						text);
					break;
				case MessageBoxButtons.YesNo:
					dialog = new Gtk.MessageDialog(
						w, 
						Gtk.DialogFlags.DestroyWithParent, 
						mType, 
						Gtk.ButtonsType.YesNo, 
						text);
					break;
				case MessageBoxButtons.YesNoCancel:
					dialog = new Gtk.MessageDialog(
						w, 
						Gtk.DialogFlags.DestroyWithParent, 
						mType,
						Gtk.ButtonsType.YesNo, 
						text);
					dialog.AddButton(Gtk.Stock.Cancel, (int) Gtk.ResponseType.Cancel);
					break;
				case MessageBoxButtons.RetryCancel:
					dialog = new Gtk.MessageDialog(
						w, 
						Gtk.DialogFlags.DestroyWithParent, 
						mType, 
						Gtk.ButtonsType.None, 
						text);
					//dialog.AddButton(Gtk.Stock.Redo, 4);
					dialog.AddButton ("Retry", 4);
					dialog.AddButton(Gtk.Stock.Cancel, (int) Gtk.ResponseType.Cancel);	
					break;
				case MessageBoxButtons.AbortRetryIgnore:
					dialog = new Gtk.MessageDialog (
						w,
						Gtk.DialogFlags.DestroyWithParent,
						mType,
						Gtk.ButtonsType.None,
						text);
					//dialog.AddButton (Gtk.Stock.Redo, 4);
					dialog.AddButton ("Abort",  3);
					dialog.AddButton ("Retry",  4);
					dialog.AddButton ("Ignore", 5);
					break;
				default:
					break;
			}
			
			int ret = dialog.Run ();
			dialog.Destroy();
			
			switch (ret){
				case (int) Gtk.ResponseType.None:
					return DialogResult.None;
				//case (int) Gtk.ResponseType.Reject:
				//case (int) Gtk.ResponseType.Accept:
					
				case (int) Gtk.ResponseType.DeleteEvent:
					return DialogResult.Cancel;
				case (int) Gtk.ResponseType.Ok:
					return DialogResult.OK;
				case (int) Gtk.ResponseType.Cancel:
					return DialogResult.Cancel;
				case (int) Gtk.ResponseType.Close:
					return DialogResult.Cancel;					
				case (int) Gtk.ResponseType.Yes:
					return DialogResult.Yes;
				case (int) Gtk.ResponseType.No:
					return DialogResult.No;
				case (int) Gtk.ResponseType.Apply:
					return DialogResult.OK;
				case (int) Gtk.ResponseType.Help:
					return DialogResult.OK;
				default:
					return (DialogResult) ret;
	
			}
		}
	}

}
