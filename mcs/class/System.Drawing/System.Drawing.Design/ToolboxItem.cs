//
// System.Drawing.Design.ToolboxItem.cs
//
// Authors:
//	Alejandro Sánchez Acosta
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Drawing.Design 
{
	[Serializable]
	public class ToolboxItem : ISerializable
	{

		private AssemblyName assembly;
		private Bitmap bitmap;
		private ICollection filter = new ToolboxItemFilterAttribute[0];
		private string displayname = "";
		private bool locked = false;
		private string name = "";
		
		public ToolboxItem() {
		}

		public ToolboxItem (Type toolType) {
			Initialize (toolType);
		}

		public AssemblyName AssemblyName {
			get {
				return assembly;
			}

			set {
				CheckUnlocked ();
				assembly = value;
			}
		}

		public Bitmap Bitmap {
			get {
				return bitmap;
			}
			
			set {
				CheckUnlocked ();
				bitmap = value;
			}
		}

		public string DisplayName {
			get {
				return displayname;
			}
			
			set {
				CheckUnlocked ();
				displayname = value;
			}
		}

		public ICollection Filter {
			get {
				return filter;
			}
			
			set {
				CheckUnlocked ();
				filter = value;
			}
		}
		
		protected bool Locked {
			get {
				return locked;
			}
		}

		public string TypeName {
			get {
				return name;
			}

			set {
				CheckUnlocked ();
				name = value;
			}
		}
		
		protected void CheckUnlocked ()
		{
			throw new InvalidOperationException ("The ToolboxItem is locked");
		}

		public IComponent[] CreateComponents () 
		{
			return CreateComponents (null);
		}

		public IComponent[] CreateComponents (IDesignerHost host)
		{
			OnComponentsCreating (new ToolboxComponentsCreatingEventArgs (host));
			IComponent[] Comp = CreateComponentsCore (host);
			OnComponentsCreated ( new ToolboxComponentsCreatedEventArgs (Comp));
			return Comp;
		}

		[MonoTODO]
		protected virtual IComponent[] CreateComponentsCore (IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Deserialize (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object obj)
		{
			// FIXME: too harsh??
			if (!(obj is ToolboxItem))
				return false;
			if (obj == this)
				return true;
			return ((ToolboxItem) obj).AssemblyName.Equals (assembly) &&
				((ToolboxItem) obj).Locked.Equals (locked) &&
				((ToolboxItem) obj).TypeName.Equals (name) &&
				((ToolboxItem) obj).DisplayName.Equals (displayname) &&
				((ToolboxItem) obj).Bitmap.Equals (bitmap);
		}
		
		public override int GetHashCode ()
		{
			// FIXME: other algorithm?
			return string.Concat (name, displayname).GetHashCode ();
		}

		[MonoTODO]
		protected virtual Type GetType (IDesignerHost host, AssemblyName assemblyName, string typeName, bool reference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Initialize (Type type) 
		{
			// assembly = // FIXME we need to get the AssemblyName data from somewhere or create a new one
			displayname = type.Name;
			name = type.FullName;
			// seems to be a right place to create the bitmap
			bitmap = new Bitmap (16, 16); // FIXME set some default bitmap !?

			filter = type.GetCustomAttributes (typeof (ToolboxItemFilterAttribute), true);

			throw new NotImplementedException ();
		}
		
		[MonoTODO]		
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public void Lock ()
		{
			locked = true;
		}

		protected virtual void OnComponentsCreated (ToolboxComponentsCreatedEventArgs args)
		{
			this.ComponentsCreated (this, args);
		}

		protected virtual void OnComponentsCreating (ToolboxComponentsCreatingEventArgs args)
		{
			this.ComponentsCreating (this, args);
		}

		[MonoTODO]
		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ToString()
		{
			return displayname;
		}

		public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

		public event ToolboxComponentsCreatingEventHandler ComponentsCreating;
	}
}
