//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;
using Gtk;
using GtkSharp;

namespace System.Windows.Forms{
	
	public class MessageBox : Control {

		Gtk.MessageDialog dialog;

		private MessageBox (){
		}

		public static void Show (String text) {

			Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);			
			dialog.Run();
			dialog.Destroy();
		}

		public static void Show (Form myform, String text) {

			Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);			
			dialog.Run();
			dialog.Destroy();
		}



		public static void Show (String text, String caption) {

			Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);	
			dialog.Title = caption; 		
			dialog.Run();
			dialog.Destroy();
		}

		public static void Show (Form myform, String text, String caption) {

			Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);	
			dialog.Title = caption; 		
			dialog.Run();
			dialog.Destroy();
		}

		public static void Show (String text, String caption, MessageBoxButtons but) {
			
			if (but == MessageBoxButtons.OK){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}	
			if (but == MessageBoxButtons.OKCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.OkCancel, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}
			if (but == MessageBoxButtons.YesNo){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.YesNo, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}
			if (but == MessageBoxButtons.YesNoCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.YesNo, text);	
				dialog.Title = caption; 
				dialog.AddButton(Gtk.Stock.Cancel, 2);		
				dialog.Run();
				dialog.Destroy();
			}		
			if (but == MessageBoxButtons.RetryCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.None, text);	
				dialog.Title = caption; 
				dialog.AddButton(Gtk.Stock.Redo, 4);
				dialog.AddButton(Gtk.Stock.Cancel, 2);		
				dialog.Run();
				dialog.Destroy();
			} 
		}
			public static void Show (Form myform ,String text, String caption, MessageBoxButtons but) {
			
			if (but == MessageBoxButtons.OK){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}	
			if (but == MessageBoxButtons.OKCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.OkCancel, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}
			if (but == MessageBoxButtons.YesNo){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.YesNo, text);	
				dialog.Title = caption; 		
				dialog.Run();
				dialog.Destroy();
			}
			if (but == MessageBoxButtons.YesNoCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.YesNo, text);	
				dialog.Title = caption; 
				dialog.AddButton(Gtk.Stock.Cancel, 2);		
				dialog.Run();
				dialog.Destroy();
			}		
			if (but == MessageBoxButtons.RetryCancel){
				Gtk.MessageDialog dialog = new Gtk.MessageDialog(myform.win, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.None, text);	
				dialog.Title = caption; 
				dialog.AddButton(Gtk.Stock.Redo, 4);
				dialog.AddButton(Gtk.Stock.Cancel, 2);		
				dialog.Run();
				dialog.Destroy();
			} 

		}

	}

} 
