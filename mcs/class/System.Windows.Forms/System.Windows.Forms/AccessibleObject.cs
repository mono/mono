//
// System.Windows.Forms.AccessibleObject.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;

namespace System.Windows.Forms
{
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
	public class AccessibleObject : MarshalByRefObject, IReflect/*, [MonoTODO]: */ //,IAccessible
	{
		string name;
		
//		// --- Properties ---
//		[MonoTODO]
//		public virtual Rectangle Bounds
//		{
//			get { throw new NotImplementedException (); }
//		}
//
//		[MonoTODO]
//		public virtual string DefaultAction
//		{
//			get { throw new NotImplementedException (); }
//		}
//    
//		[MonoTODO]
//		public virtual string Description
//		{
//			get { throw new NotImplementedException (); }
//		}
//		[MonoTODO]
//		public virtual string Help
//		{
//			get { throw new NotImplementedException (); }
//		}
//		[MonoTODO]
//		public virtual string KeyboardShortcut
//		{
//			get { throw new NotImplementedException (); }
//		}
//
//		public virtual string Name {
//			get { return name; }
//			set { name = value; }
//		}
//
//		[MonoTODO]
//		public virtual AccessibleObject Parent
//		{
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleRole Role
//		{
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleStates State
//		{
//			get { throw new NotImplementedException (); }
//		}
//
//		[MonoTODO]
//		public virtual string Value
//		{
//			get { throw new NotImplementedException (); }
//		}
//
//		
//		
//		// --- Constructor ---
//		[MonoTODO]
//		public AccessibleObject()
//		{
//			throw new NotImplementedException ();
//		}
//		
//		
//		
//		// --- Methods ---
//		[MonoTODO]
//		public virtual void DoDefaultAction() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleObject GetChild(int index) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual int GetChildCount() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleObject GetFocused() {
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public virtual int GetHelpTopic(out string fileName) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleObject GetSelected() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleObject HitTest(int x,int y) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) {
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public virtual void Select(AccessibleSelection flags) {
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		protected void UseStdAccessibleObjects(IntPtr handle,int objid) {
//			throw new NotImplementedException ();
//		}
//
//
//
//		// --- Methods: object ---
//		[MonoTODO]
//		public override bool Equals (object obj) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public override int GetHashCode()
//		{
//			throw new NotImplementedException ();
//		}
//
//
//
//
//		// --- Methods: IReflect ---
//		[MonoTODO]
//		public FieldInfo GetField( string name,BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public FieldInfo[] GetFields (BindingFlags bindingAttr)
//		{
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public MemberInfo[] GetMember( string name, BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public MemberInfo[] GetMembers( BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public MethodInfo GetMethod( string name, BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public MethodInfo GetMethod( string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public MethodInfo[] GetMethods( BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public PropertyInfo[] GetProperties( BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
//			// FIXME
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public Type UnderlyingSystemType
//		{
//			get { throw new NotImplementedException (); }
//		}
//		
//		/*
//		interface IAccessible
//		{
//			void accDoDefaultAction(object childID);
//			...
//		}
//		*/
	}
}
