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

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Event)]
	public class CategoryAttribute : Attribute
	{
		private string category;
		private bool IsLocalized = false;

		static CategoryAttribute action, appearance, behaviour, data,   def;
		static CategoryAttribute design, drag_drop,  focus,     format, key;
		static CategoryAttribute layout, mouse,      window_style;


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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
					if (appearance == null)
						appearance = new CategoryAttribute ("Appearance");
				}
				return appearance;
			}
		}

		public static CategoryAttribute Behavior {
			get {
				if (behaviour != null)
					return behaviour;

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
					if (def == null)
						def = new CategoryAttribute ();
				}
				return def;
			}
		}

		public static CategoryAttribute Design {
			get {
				if (design != null)
					return design;

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
					if (drag_drop == null)
						drag_drop = new CategoryAttribute ("Drag Drop");
				}
				return drag_drop;
			}
		}

		public static CategoryAttribute Focus {
			get {
				if (focus != null)
					return focus;

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
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

				lock (typeof (CategoryAttribute)) {
					if (window_style == null)
						window_style = new CategoryAttribute ("Window Style");
				}
				return window_style;
			}
		}

		[MonoTODO ("Add localization support")]
		protected virtual string GetLocalizedString (string value)
		{
			// FIXME: Check if other implementation exists
			return null;
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

