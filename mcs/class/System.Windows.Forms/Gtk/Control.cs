//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using Gtk;
using GtkSharp;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class Control : Component {
		internal Widget widget;
		Control parent;
		string text;
		int left, top, width, height;

		static int init_me;
		
		static Control ()
		{
			init_me = 1;
			Console.WriteLine ("MEEEEEEEEEEEEEEEEEEEEEE");
		}
		
		public Control () : this ("")
		{
		}

		public Control (string text) : this (null, text)
		{
		}

		public Control (Control parent, string text)
		{
			this.parent = parent;
			this.text = text;
		}

		public Control (string text, int left, int top, int width, int height)
		{
		}

		public Control (Control parent, string text, int left, int top, int width, int height)
		{
		}

		internal Widget Widget {
			get {
				if (widget == null)
					widget = CreateWidget ();
				return widget;
			}
		}
		
		internal virtual Widget CreateWidget ()
		{
			throw new Exception ();
		}

		public virtual string Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}
		
		public void Show ()
		{
			Widget.EmitShow ();
		}

		public void Hide ()
		{
			Widget.EmitHide ();
		}

		public bool Visible {
			get {
				return Widget.Visible;
			}

			set {
				Widget.Visible = value;
			}
		}
	}
}
