//
// System.ComponentModel.CategoryAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
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

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.All)]
	public class CategoryAttribute : Attribute
	{
		private string category;
		private bool IsLocalized;

		static volatile CategoryAttribute action, appearance, behaviour, data,   def;
		static volatile CategoryAttribute design, drag_drop,  focus,     format, key;
		static volatile CategoryAttribute layout, mouse,      window_style;
		static volatile CategoryAttribute async;

		static object lockobj = new object ();

		public CategoryAttribute ()
		{
			this.category = "Misc";
		}

		public CategoryAttribute (string category)
		{
			this.category = category;
		}


		public static CategoryAttribute Action {
			get {
				if (action != null)
					return action;

				lock (lockobj) {
					if (action == null)
						action = new CategoryAttribute ("Action");
				}
				return action;
			}
		}

		public static CategoryAttribute Appearance {
			get {
				if (appearance != null)
					return appearance;

				lock (lockobj) {
					if (appearance == null)
						appearance = new CategoryAttribute ("Appearance");
				}
				return appearance;
			}
		}

		public static CategoryAttribute Asynchronous {
			get {
				if (behaviour != null)
					return behaviour;

				lock (lockobj) {
					if (async == null)
						async = new CategoryAttribute ("Asynchronous");
				}
				return async;
			}
		}

		public static CategoryAttribute Behavior {
			get {
				if (behaviour != null)
					return behaviour;

				lock (lockobj) {
					if (behaviour == null)
						behaviour = new CategoryAttribute ("Behavior");
				}
				return behaviour;
			}
		}

		public static CategoryAttribute Data {
			get {
				if (data != null)
					return data;

				lock (lockobj) {
					if (data == null)
						data = new CategoryAttribute ("Data");
				}
				return data;
			}
		}

		public static CategoryAttribute Default {
			get {
				if (def != null)
					return def;

				lock (lockobj) {
					if (def == null)
#if NET_2_1
						def = new CategoryAttribute ("Default");
#else
						def = new CategoryAttribute ();
#endif
				}
				return def;
			}
		}

		public static CategoryAttribute Design {
			get {
				if (design != null)
					return design;

				lock (lockobj) {
					if (design == null)
						design = new CategoryAttribute ("Design");
				}
				return design;
			}
		}

		public static CategoryAttribute DragDrop {
			get {
				if (drag_drop != null)
					return drag_drop;

				lock (lockobj) {
					if (drag_drop == null)
#if NET_2_1
						drag_drop = new CategoryAttribute ("DragDrop");
#else
						drag_drop = new CategoryAttribute ("Drag Drop");
#endif
				}
				return drag_drop;
			}
		}

		public static CategoryAttribute Focus {
			get {
				if (focus != null)
					return focus;

				lock (lockobj) {
					if (focus == null)
						focus = new CategoryAttribute ("Focus");
				}
				return focus;
			}
		}

		public static CategoryAttribute Format {
			get {
				if (format != null)
					return format;

				lock (lockobj) {
					if (format == null)
						format = new CategoryAttribute ("Format");
				}
				return format;
			}
		}

		public static CategoryAttribute Key {
			get {
				if (key != null)
					return key;

				lock (lockobj) {
					if (key == null)
						key = new CategoryAttribute ("Key");
				}
				return key;
			}
		}

		public static CategoryAttribute Layout {
			get {
				if (layout != null)
					return layout;

				lock (lockobj) {
					if (layout == null)
						layout = new CategoryAttribute ("Layout");
				}
				return layout;
			}
		}

		public static CategoryAttribute Mouse {
			get {
				if (mouse != null)
					return mouse;

				lock (lockobj) {
					if (mouse == null)
						mouse = new CategoryAttribute ("Mouse");
				}
				return mouse;
			}
		}

		public static CategoryAttribute WindowStyle {
			get {
				if (window_style != null)
					return window_style;

				lock (lockobj) {
					if (window_style == null)
#if NET_2_1
						window_style = new CategoryAttribute ("WindowStyle");
#else
						window_style = new CategoryAttribute ("Window Style");
#endif
				}
				return window_style;
			}
		}

		protected virtual string GetLocalizedString (string value)
		{
			return Locale.GetText (value);
		}

		public string Category {
			get {
				if (IsLocalized == false) {
					IsLocalized = true;
					string LocalizedString = GetLocalizedString (category);
					if (LocalizedString != null)
						category = LocalizedString;
				}

				return category;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is CategoryAttribute))
				return false;
			if (obj == this)
				return true;
			return ((CategoryAttribute) obj).Category == category;
		}

		public override int GetHashCode ()
		{
			return category.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return category == CategoryAttribute.Default.Category;
		}
	}
}

