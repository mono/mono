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
	public class AccessibleObject : MarshalByRefObject{//   /*, [MonoTODO]: */ // IReflect/*, [MonoTODO]: */ //,IAccessible {

		string name;
		
		// --- Constructor ---
		[MonoTODO]
		//[ComVisible(true)]
		public AccessibleObject() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		//[Serializable]
		//[ClassInterface(ClassInterfaceType.AutoDual)]
		~AccessibleObject(){
		}
		
		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this AccessibleObject and another object.
		/// </remarks>
		
		public override bool Equals (object o) 
		{
			if (!(o is AccessibleObject))
				return false;

			return (this == (AccessibleObject) o);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode () 
		{
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
		
		public override string ToString () 
		{
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

		[MonoTODO]
		[ComVisible(true)]
		public virtual string Value {

			get { throw new NotImplementedException (); }
		}
		
		// --- Methods ---
		[MonoTODO]
		[ComVisible(true)]
		public virtual void DoDefaultAction() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetChild(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual int GetChildCount() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetFocused() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual int GetHelpTopic(out string fileName) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject GetSelected() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject HitTest(int x,int y) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public virtual AccessibleObject Navigate(AccessibleNavigation navdir) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(true)]
		public virtual void Select(AccessibleSelection flags) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(true)]
		protected void UseStdAccessibleObjects(IntPtr handle,int objid) 
		{
			throw new NotImplementedException ();
		}


		// --- Methods: IReflect ---
		[MonoTODO]
		public FieldInfo GetField( string name,BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MemberInfo[] GetMember( string name, BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MemberInfo[] GetMembers( BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MethodInfo GetMethod( string name, BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MethodInfo GetMethod( string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MethodInfo[] GetMethods( BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PropertyInfo[] GetProperties( BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) 
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) 
		{
			// FIXME
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Type UnderlyingSystemType {

			get { throw new NotImplementedException (); }
		}
		
		interface IAccessible {

			void accDoDefaultAction(object childID);
		/*
			...
		*/
		}
	}
}
