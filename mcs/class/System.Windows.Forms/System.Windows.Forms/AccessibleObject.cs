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

		string name;
		string value;
		// --- Constructor ---
		[MonoTODO]
		public AccessibleObject() {
			throw new NotImplementedException ();
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
			unchecked{//FIXME Add out proprities to the hash
				return base.GetHashCode();
			}
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the AccessibleObject as a string.
		/// </remarks>
		
		public override string ToString () {
			//FIXME add our proprities to ToString
			return base.ToString();// String.Format ("[{0},{1},{2}]", bindingpath, bindingfield, bindingmember);
		}

		// --- Properties ---
		[MonoTODO]
		[ComVisible(true)]
		public virtual Rectangle Bounds {

			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual string DefaultAction {

			get { throw new NotImplementedException (); }
		}
    
		[MonoTODO]
		[ComVisible(true)]
		public virtual string Description {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		[ComVisible(true)]
		public virtual string Help {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		[ComVisible(true)]
		public virtual string KeyboardShortcut {

			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual string Name {
			get { return name; }
			set { name = value; }
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual string Value {
			get { return this.value; }
			set { this.value = value; }
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject Parent {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleRole Role {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleStates State {

			get { throw new NotImplementedException (); }
		}

		// --- Methods ---
		[MonoTODO]
		[ComVisible(true)]
		public virtual void DoDefaultAction() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetChild(int index) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual int GetChildCount() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetFocused() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual int GetHelpTopic(out string fileName) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetSelected() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject HitTest(int x,int y) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual void Select(AccessibleSelection flags) {
			throw new NotImplementedException ();
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

