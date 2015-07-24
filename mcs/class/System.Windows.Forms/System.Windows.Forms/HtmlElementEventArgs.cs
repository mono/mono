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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	avidigal@novell.com
//

using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms
{
	public sealed class HtmlElementEventArgs : EventArgs
	{
		#region Fields
		private bool alt_key_pressed;
		private bool bubble_event;
		private Point client_mouse_position;
		private bool ctrl_key_pressed;
		private string event_type;
		private HtmlElement from_element;
		private int key_pressed_code;
		private MouseButtons mouse_buttons_pressed;
		private Point mouse_position;
		private Point offset_mouse_position;
		private bool return_value;
		private bool shift_key_pressed;
		private HtmlElement to_element;
		#endregion

		#region Constructor
		internal HtmlElementEventArgs ()
		{
			alt_key_pressed = false;
			bubble_event = false;
			client_mouse_position = Point.Empty;
			ctrl_key_pressed = false;;
			event_type = null;
			from_element = null;
			key_pressed_code = 0;
			mouse_buttons_pressed = MouseButtons.None;
			mouse_position = Point.Empty;
			offset_mouse_position = Point.Empty;
			return_value = false;
			shift_key_pressed = false;
			to_element = null;
		}
		#endregion

		#region Public Properties
		public bool AltKeyPressed {
			get { return alt_key_pressed; }
		}
		
		public bool BubbleEvent {
			get { return bubble_event; }
			set { bubble_event = value; }
		}
		
		public Point ClientMousePosition {
			get { return client_mouse_position; }
		}
		
		public bool CtrlKeyPressed {
			get { return ctrl_key_pressed; }
		}
		
		public string EventType {
			get { return event_type; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public HtmlElement FromElement {
			get { return from_element; }
		}
		
		public int KeyPressedCode {
			get { return key_pressed_code; }
		}
		
		public MouseButtons MouseButtonsPressed {
			get { return mouse_buttons_pressed; }
		}
		
		public Point MousePosition {
			get { return mouse_position; }
		}
		
		public Point OffsetMousePosition {
			get { return offset_mouse_position; }
		}
		
		public bool ReturnValue {
			get { return return_value; }
			set { return_value = value; }
		}
		
		public bool ShiftKeyPressed {
			get { return shift_key_pressed; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public HtmlElement ToElement {
			get { return to_element; }
		}
		#endregion
	}
}
