//
// System.Windows.Forms.AccessibleObject.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002/3 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
using System.Drawing;
using Accessibility;
using System.Runtime.InteropServices;
using IEnumVariant = System.Windows.Forms.UnsafeNativeMethods.IEnumVariant;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides information that accessibility applications use to adjust an application's UI for users with impairments.
	/// </summary>
	[MonoTODO]
	public class AccessibleObject : MarshalByRefObject, IReflect, IAccessible, UnsafeNativeMethods.IEnumVariant {
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

		// --- Properties ---

		//Version 1.1
		protected void UseStdAccessibleObjects(IntPtr handle){
		}

		//Version 1.1
		protected void UseStdAccessibleObjects(IntPtr handle, int objid){
		}
	
		public virtual Rectangle Bounds {
			get { return Rectangle.Empty; } // As per spec for default. Expect override.
		}

		public virtual string DefaultAction {
			get {return null; }// As per spec for default. Expect override.
		}
    
		public virtual string Description {
			get {return null; }// As per spec for default. Expect override.
		}
	
		public virtual string Help {
			get {return null; }// As per spec for default. Expect override.
		}
	
		public virtual string KeyboardShortcut {
			get {return null; }// As per spec for default. Expect override.
		}
	
		public virtual string Name {
			get { return name; }
			set { name = value; }
		}
	
		public virtual string Value {
			get { return this.value; }
			set { this.value = value; }
		}
	
		public virtual AccessibleObject Parent {
			get { return parent; }
			set { parent = value; }
		}
		
		public virtual AccessibleRole Role {
			get { return role; }
			set { role = value; }
		}
	
		public virtual AccessibleStates State {
			get { return state; }
			set { state = value; }
		}

		// --- Methods ---
	
		public virtual void DoDefaultAction() {
			return; //default action is "" and cannot be changed, must be overridden.
		}
		
		[MonoTODO]
		public virtual AccessibleObject GetChild(int index) {
			return null;
		}
		
		[MonoTODO]
		public virtual int GetChildCount() {
			return -1; //as per spec
		}
		
		[MonoTODO]
		public virtual AccessibleObject GetFocused() {
			return null;//FIXME: not quite to spec.
		}
	
		public virtual int GetHelpTopic(out string fileName) {
			fileName = "";
			return -1;//no help
		}
		
		public virtual AccessibleObject GetSelected() {
			return null;
		}
		
		[MonoTODO]
		public virtual AccessibleObject HitTest(int x,int y) {
			return null;		}
		
		[MonoTODO]
		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) {
			//by default, navagate back to here. Does this work? 
			//not to spec, but better than execption FIXME:
			return this;
		}

		[MonoTODO]
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

		[MonoTODO]
		Type IReflect.UnderlyingSystemType {
		//private Type UnderlyingSystemType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		void IAccessible.accDoDefaultAction(object childID) {
			//FIXME:
		}

		[MonoTODO]
		int IAccessible.accChildCount{
			get{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		object IAccessible.accFocus{
			get{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		object IAccessible.accHitTest(int xLeft, int yTop) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IAccessible.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IAccessible.accNavigate(int navDir, object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IAccessible.accParent {
			get{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		void IAccessible.accSelect(int flagsSelect, object childID) {
			//FIXME:
		}

		[MonoTODO]
		object IAccessible.accSelection {
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		object IAccessible.get_accChild(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accDefaultAction(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accDescription(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accHelp(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IAccessible.get_accHelpTopic(out string pszHelpFile,object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accKeyboardShortcut(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accName(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IAccessible.get_accRole(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IAccessible.get_accState(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IAccessible.get_accValue(object childID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IAccessible.set_accName(object childID, string newName) {
			//FIXME:
		}

		[MonoTODO]
		void IAccessible.set_accValue(object childID, string newValue) {
			//FIXME:
		}

		#region Implementation of UnsafeNativeMethods.IEnumVariant

		[MonoTODO]
		void UnsafeNativeMethods.IEnumVariant.Clone (IEnumVariant[] v)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int UnsafeNativeMethods.IEnumVariant.Next (int n, IntPtr rgvar, int[] ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void UnsafeNativeMethods.IEnumVariant.Reset ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void UnsafeNativeMethods.IEnumVariant.Skip (int n)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of UnsafeNativeMethods.IEnumVariant
	}

}

