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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Dennis Bartok	pbartok@novell.com
//


// NOT COMPLETE

using Accessibility;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ComVisible(true)]
	public class AccessibleObject : StandardOleMarshalObject, IReflect, IAccessible {
		#region Private Variables
		internal string		name;
		internal string		value;
		internal Control owner;
		internal AccessibleRole	role;
		internal AccessibleStates	state;
		internal string		default_action;
		internal string		description;
		internal string		help;
		internal string		keyboard_shortcut;
		#endregion	// Private Variables

		#region Public Constructors
		public AccessibleObject() {
			this.owner = null;
			this.value = null;
			this.name = null;
			this.role = AccessibleRole.Default;
			this.default_action = null;
			this.description = null;
			this.help = null;
			this.keyboard_shortcut = null;
			this.state = AccessibleStates.None;
		}
		#endregion	// Public Constructors

		#region Private Constructors
		internal AccessibleObject(Control owner) : this () {
			this.owner=owner;
		}
		#endregion	// Private Constructors

		#region Public Instance Properties
		public virtual Rectangle Bounds {
			get {
				return owner.Bounds;
			}
		}

		public virtual string DefaultAction {
			get {
				return default_action;
			}
		}

		public virtual string Description {
			get {
				return description;
			}
		}

		public virtual string Help {
			get {
				return help;
			}
		}

		public virtual string KeyboardShortcut {
			get {
				return keyboard_shortcut;
			}
		}

		public virtual string Name {
			get {
				return name;
			}

			set {
				name=value;
			}
		}

		public virtual AccessibleObject Parent {
			get {
				if ((owner!=null) && (owner.Parent!=null)) {
					return owner.Parent.AccessibilityObject;
				}
				return null;
			}
		}

		public virtual AccessibleRole Role {
			get {
				return role;
			}
		}

		public virtual AccessibleStates State {
			get {
#if not
				if (owner!=null) {
					if (owner.Focused) {
						state |= AccessibleStates.Focused;
					}

					if (!owner.Visible) {
						state |= AccessibleStates.Invisible;
					}
				}
#endif
				return state;
			}
		}

		public virtual string Value {
			get {
				return this.value;
			}

			set {
				this.value=value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public virtual void DoDefaultAction() {
			if (owner!=null) {
				owner.DoDefaultAction();
			}
		}

		public virtual AccessibleObject GetChild(int index) {
			if (owner!=null) {
				if (index<owner.Controls.Count) {
					return owner.Controls[index].AccessibilityObject;
				}
			}
			return null;
		}

		public virtual int GetChildCount() {
			if (owner!=null) {
				return owner.Controls.Count;
			}
			return -1;
		}

		public virtual AccessibleObject GetFocused() {
			if (owner.has_focus) {
				return owner.AccessibilityObject;
			}

			return FindFocusControl(owner);
		}

		public virtual int GetHelpTopic (out string fileName)
		{
			fileName = null;
			return -1;
		}

		public virtual AccessibleObject GetSelected() {
			if ((state & AccessibleStates.Selected) != 0) {
				return this;
			}

			return FindSelectedControl(owner);
		}

		public virtual AccessibleObject HitTest(int x, int y) {
			Control result;

			result = FindHittestControl(owner, x, y);

			if (result != null) {
				return result.AccessibilityObject;
			}

			return null;
		}

		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) {
			int	index;

			// I'm not throwing exceptions if an object doesn't exist in the specified direction
			// Might not be too helpful to a blind dude trying to navigate. Instead we return
			// our own object

			if (owner.Parent != null) {
				index = owner.Parent.Controls.IndexOf(owner);
			} else {
				index = -1;
			}

			switch (navdir) {
				// Spatial navigation; limited to siblings
				case AccessibleNavigation.Up: {
					if (owner.Parent != null) {
						for (int i=0; i<owner.Parent.Controls.Count; i++) {
							if ((owner != owner.Parent.Controls[i]) && (owner.Parent.Controls[i].Top<owner.Top)) {
								return owner.Parent.Controls[i].AccessibilityObject;
							}
						}
						
					}
					return owner.AccessibilityObject;
				}

				case AccessibleNavigation.Down: {
					if (owner.Parent != null) {
						for (int i=0; i<owner.Parent.Controls.Count; i++) {
							if ((owner != owner.Parent.Controls[i]) && (owner.Parent.Controls[i].Top>owner.Bottom)) {
								return owner.Parent.Controls[i].AccessibilityObject;
							}
						}
						
					}
					return owner.AccessibilityObject;
				}

				case AccessibleNavigation.Left: {
					if (owner.Parent != null) {
						for (int i=0; i<owner.Parent.Controls.Count; i++) {
							if ((owner != owner.Parent.Controls[i]) && (owner.Parent.Controls[i].Left<owner.Left)) {
								return owner.Parent.Controls[i].AccessibilityObject;
							}
						}
						
					}
					return owner.AccessibilityObject;
				}

				case AccessibleNavigation.Right: {
					if (owner.Parent != null) {
						for (int i=0; i<owner.Parent.Controls.Count; i++) {
							if ((owner != owner.Parent.Controls[i]) && (owner.Parent.Controls[i].Left>owner.Right)) {
								return owner.Parent.Controls[i].AccessibilityObject;
							}
						}
						
					}
					return owner.AccessibilityObject;
				}

				// Logical navigation
				case AccessibleNavigation.Next: {
					if (owner.Parent != null) {
						if ((index+1)<owner.Parent.Controls.Count) {
							return owner.Parent.Controls[index+1].AccessibilityObject;
						} else {
							return owner.Parent.Controls[0].AccessibilityObject;
						}
					} else {
						return owner.AccessibilityObject;
					}
				}

				case AccessibleNavigation.Previous: {
					if (owner.Parent != null) {
						if (index>0) {
							return owner.Parent.Controls[index-1].AccessibilityObject;
						} else {
							return owner.Parent.Controls[owner.Parent.Controls.Count-1].AccessibilityObject;
						}
					} else {
						return owner.AccessibilityObject;
					}
				}

				case AccessibleNavigation.FirstChild: {
					if (owner.Controls.Count>0) {
						return owner.Controls[0].AccessibilityObject;
					} else {
						return owner.AccessibilityObject;
					}
				}

				case AccessibleNavigation.LastChild: {
					if (owner.Controls.Count>0) {
						return owner.Controls[owner.Controls.Count-1].AccessibilityObject;
					} else {
						return owner.AccessibilityObject;
					}
				}
			}

			return owner.AccessibilityObject;
		}

		public virtual void Select(AccessibleSelection flags) {
			if ((flags & AccessibleSelection.TakeFocus) != 0){
				owner.Focus();
			}
			return;
		}
		#endregion	// Public Instance Methods

		#region	Protected Instance Methods
		protected void UseStdAccessibleObjects(IntPtr handle) {
		}

		protected void UseStdAccessibleObjects(IntPtr handle, int objid) {
			UseStdAccessibleObjects(handle, 0);
		}
		#endregion	// Protected Instance Methods


		#region Internal Methods
		internal static AccessibleObject FindFocusControl(Control parent) {
			Control	child;

			if (parent != null) {
				for (int i=0; i < parent.Controls.Count; i++) {
					child = parent.Controls[i];
					if ((child.AccessibilityObject.state & AccessibleStates.Focused) != 0) {
						return child.AccessibilityObject;
					}

					if (child.Controls.Count>0) {
						AccessibleObject result;

						result = FindFocusControl(child);
						if (result != null) {
							return result;
						}
					}
				}
			}
			return null;
		}

		internal static AccessibleObject FindSelectedControl(Control parent) {
			Control	child;

			if (parent != null) {
				for (int i=0; i < parent.Controls.Count; i++) {
					child = parent.Controls[i];
					if ((child.AccessibilityObject.state & AccessibleStates.Selected) != 0) {
						return child.AccessibilityObject;
					}
					if (child.Controls.Count>0) {
						AccessibleObject result;

						result = FindSelectedControl(child);
						if (result != null) {
							return result;
						}
					}
				}
			}
			return null;
		}

		internal static Control FindHittestControl(Control parent, int x, int y) {
			Control	child;
			Point	child_point;
			Point	hittest_point;

			hittest_point = new Point(x, y);

			child_point = parent.PointToClient(hittest_point);
			if (parent.ClientRectangle.Contains(child_point)) {
				return parent;
			}

			for (int i=0; i < parent.Controls.Count; i++) {
				child=parent.Controls[i];
				child_point = child.PointToClient(hittest_point);
				if (child.ClientRectangle.Contains(child_point)) {
					return child;
				}
				if (child.Controls.Count>0) {
					Control result;

					result = FindHittestControl(child, x, y);
					if (result != null) {
						return result;
					}
				}
			}
			return null;
		}
		#endregion	// Internal Methods

		#region	IReflection Methods and Properties
		FieldInfo IReflect.GetField(String name, BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}       

		FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}    

		MemberInfo[] IReflect.GetMember(String name, BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		MethodInfo IReflect.GetMethod(String name, BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		MethodInfo IReflect.GetMethod(String name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException();
		}

		MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		PropertyInfo IReflect.GetProperty(String name, BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		PropertyInfo IReflect.GetProperty(String name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException();
		}

		PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr) {
			throw new NotImplementedException();
		}

		Object IReflect.InvokeMember(String name, BindingFlags invokeAttr, Binder binder, Object target, Object[] args, ParameterModifier[] modifiers, CultureInfo culture, String[] namedParameters) {
			throw new NotImplementedException();
		}

		Type IReflect.UnderlyingSystemType {
			get {
				throw new NotImplementedException();
			}
		}
		#endregion	// IReflection Methods and Properties

		#region IAccessible Methods and Properties
		void IAccessible.accDoDefaultAction(object childID) {
			throw new NotImplementedException();
		}

		int IAccessible.accChildCount {
			get {
				throw new NotImplementedException();
			}
		}

		object IAccessible.accFocus {
			get {
				throw new NotImplementedException();
			}
		}

		object IAccessible.accHitTest(int xLeft, int yTop) {
			throw new NotImplementedException();
		}

		void IAccessible.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID) {
			throw new NotImplementedException();
		}

		object IAccessible.accNavigate(int navDir, object childID) {
			throw new NotImplementedException();
		}

		object IAccessible.accParent {
			get {
				throw new NotImplementedException();
			}
		}

                void IAccessible.accSelect(int flagsSelect, object childID) {
			throw new NotImplementedException();
                }

                object IAccessible.accSelection {
                        get {
				throw new NotImplementedException();
			}
		}

		object IAccessible.get_accChild(object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accDefaultAction(object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accDescription(object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accHelp(object childID) {
			throw new NotImplementedException();
		}

		int IAccessible.get_accHelpTopic(out string pszHelpFile,object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accKeyboardShortcut(object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accName(object childID) {
			throw new NotImplementedException();
		}

		object IAccessible.get_accRole(object childID) {
			throw new NotImplementedException();
		}

		object IAccessible.get_accState(object childID) {
			throw new NotImplementedException();
		}

		string IAccessible.get_accValue(object childID) {
			throw new NotImplementedException();
		}

		void IAccessible.set_accName(object childID, string newName) {
			throw new NotImplementedException();
		}

		void IAccessible.set_accValue(object childID, string newValue) {
			throw new NotImplementedException();
                }
		#endregion	// IAccessible Methods and Properties
	}
}
