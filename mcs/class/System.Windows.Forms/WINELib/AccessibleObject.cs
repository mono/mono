//
// System.Windows.Forms.AccessibleObject.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;
using Accessibility;
using System.Runtime.InteropServices;
namespace System.Windows.Forms {

	/// <summary>
	/// Provides information that accessibility applications use to adjust an application's UI for users with impairments.
	///
	/// ToDo note:
	///  - Nothing is implemented
	///  - IAccessible members not stubbed out
	///  - MarshalByRefObject members not stubbed out
	/// MSDN gives little info on the members of IAccessible: "This member supports the .NET Framework infrastructure and is not intended to be used directly from your code."
	/// </summary>
	[MonoTODO]
	[ComVisible(true)]
	public class AccessibleObject : MarshalByRefObject, IReflect, IAccessible {
		private string defaultAction;
		private string description;
		private string help;
		private string keyboardShortcut;
		private AccessibleObject parent;
		private AccessibleRole role;
		private AccessibleStates state;
		private string name;
		private string value;
		// --- Constructor ---
		[MonoTODO]
		public AccessibleObject() {
			name = null;
			parent = null;
			role = AccessibleRole.None;
			state = AccessibleStates.None;
			value = null;

		}

		[MonoTODO]
		~AccessibleObject(){
		}
		
		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this AccessibleObject and another object.
		/// </remarks>
		
		public override bool Equals (object obj) {
			if (!(obj is AccessibleObject))
				return false;

			return (this == (AccessibleObject) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode () {
			//unchecked{//FIXME Add out proprities to the hash
				return base.GetHashCode();
			//}
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the AccessibleObject as a string.
		/// </remarks>
		
		//spec says inherited
		//public override string ToString () {
		//	return "AccessibleObject".GetType();//per spec as I read it?
		//}

		// --- Properties ---

		[ComVisible(true)]
		public virtual Rectangle Bounds {

			get { return Rectangle.Empty; } // As per spec for default. Expect override.
		}

		[ComVisible(true)]
		public virtual string DefaultAction {

			get {return null; }// As per spec for default. Expect override.
		}
    
		[ComVisible(true)]
		public virtual string Description {

			get {return null; }// As per spec for default. Expect override.
		}

		[ComVisible(true)]
		public virtual string Help {

			get {return null; }// As per spec for default. Expect override.
		}

		[ComVisible(true)]
		public virtual string KeyboardShortcut {

			get {return null; }// As per spec for default. Expect override.
		}

		[ComVisible(true)]
		public virtual string Name {
			get { return name; }
			set { name = value; }
		}

		[ComVisible(true)]
		public virtual string Value {
			get { return this.value; }
			set { this.value = value; }
		}

		[ComVisible(true)]
		public virtual AccessibleObject Parent {
			get { return parent; }
			set { parent = value; }
		}
		
		[ComVisible(true)]
		public virtual AccessibleRole Role {
			get { return role; }
			set { role = value; }
		}
		
		[ComVisible(true)]
		public virtual AccessibleStates State {
			get { return state; }
			set { state = value; }
		}

		// --- Methods ---
		[ComVisible(true)]
		public virtual void DoDefaultAction() {
			return; //default action is "" and cannot be changed, must be overridden.
		}
		
		[ComVisible(true)]
		public virtual AccessibleObject GetChild(int index) {
			return null;
		}
		
		[ComVisible(true)]
		public virtual int GetChildCount() {
			return -1; //as per spec
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetFocused() {
			return null;//FIXME: not quite to spec.
		}

		[ComVisible(true)]
		public virtual int GetHelpTopic(out string fileName) {
			fileName = "";
			return -1;//no help
		}
		
		[ComVisible(true)]
		public virtual AccessibleObject GetSelected() {
			return null;
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject HitTest(int x,int y) {
			return null;		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) {
			//by default, navagate back to here. Does this work? 
			//not to spec, but better than execption FIXME:
			return this;
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual void Select(AccessibleSelection flags) {
			return;//FIXME: Not to spec. should be over ridden anyway.
		}

		//Not part of spec?
		//[MonoTODO]
		//[ComVisible(true)]
		//protected void UseStdAccessibleObjects(IntPtr handle,int objid) 
		//{
		//	throw new NotImplementedException ();
		//}


		// --- Methods: IReflect ---
		[MonoTODO]
		FieldInfo IReflect.GetField( string name,BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		FieldInfo[] IReflect.GetFields (BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		MemberInfo[] IReflect.GetMember( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		MemberInfo[] IReflect.GetMembers( BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		MethodInfo IReflect.GetMethod( string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}
		[MonoTODO]
		MethodInfo IReflect.GetMethod( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}



		[MonoTODO]
		MethodInfo[] IReflect.GetMethods( BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyInfo[] IReflect.GetProperties( BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyInfo IReflect.GetProperty( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyInfo IReflect.GetProperty( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		//[Guid("")]
		object IReflect.InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			// FIXME
			throw new NotImplementedException ();
		}
		
		
		Type IReflect.UnderlyingSystemType {
		//private Type UnderlyingSystemType {
			get { throw new NotImplementedException (); }
		}
		
		void IAccessible.accDoDefaultAction(object childID) {
			throw new NotImplementedException ();
		}
		int IAccessible.accChildCount{
			get{
				throw new NotImplementedException ();
			}
		}

		object IAccessible.accFocus{
			get{
				throw new NotImplementedException ();
			}
		}
		object IAccessible.accHitTest(int xLeft, int yTop) {
			throw new NotImplementedException ();
		}
		void IAccessible.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID) {
			throw new NotImplementedException ();
		}
		object IAccessible.accNavigate(int navDir, object childID) {
			throw new NotImplementedException ();
		}
		object IAccessible.accParent {
			get{
				throw new NotImplementedException ();
			}
		}
		void IAccessible.accSelect(int flagsSelect, object childID) {
			throw new NotImplementedException ();
		}
		object IAccessible.accSelection {
			get{
				throw new NotImplementedException ();
			}
		}
		object IAccessible.get_accChild(object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accDefaultAction(object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accDescription(object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accHelp(object childID) {
			throw new NotImplementedException ();
		}
		int IAccessible.get_accHelpTopic(out string pszHelpFile,object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accKeyboardShortcut(object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accName(object childID) {
			throw new NotImplementedException ();
		}
		object IAccessible.get_accRole(object childID) {
			throw new NotImplementedException ();
		}
		object IAccessible.get_accState(object childID) {
			throw new NotImplementedException ();
		}
		string IAccessible.get_accValue(object childID) {
			throw new NotImplementedException ();
		}
		void IAccessible.set_accName(object childID, string newName) {
			throw new NotImplementedException ();
		}
		void IAccessible.set_accValue(object childID, string newValue) {
			throw new NotImplementedException ();
		}
	}
	
}

