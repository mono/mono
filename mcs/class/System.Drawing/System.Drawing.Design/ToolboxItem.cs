//
// System.Drawing.Design.ToolboxItem.cs
//
// Authors:
//   Alejandro Sánchez Acosta
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
		private string displayname = string.Empty;
		private bool locked = false;
		private string name = string.Empty;
		
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

		protected virtual void Deserialize (SerializationInfo info, StreamingContext context)
		{
			assembly = (AssemblyName)info.GetValue ("AssemblyName", typeof (AssemblyName));
			bitmap = (Bitmap)info.GetValue ("Bitmap", typeof (Bitmap));
			filter = (ICollection)info.GetValue ("Filter", typeof (ICollection));
			displayname = info.GetString ("DisplayName");
			locked = info.GetBoolean ("Locked");
			name = info.GetString ("TypeName");
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
			
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			Serialize (info, context);
		}

		public void Lock ()
		{
			locked = true;
		}

		protected virtual void OnComponentsCreated (ToolboxComponentsCreatedEventArgs args)
		{
			if (ComponentsCreated != null)
				this.ComponentsCreated (this, args);
		}

		protected virtual void OnComponentsCreating (ToolboxComponentsCreatingEventArgs args)
		{
			if (ComponentsCreated != null)
				this.ComponentsCreating (this, args);
		}

		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("AssemblyName", assembly);
			info.AddValue ("Bitmap", bitmap);
			info.AddValue ("Filter", filter);
			info.AddValue ("DisplayName", displayname);
			info.AddValue ("Locked", locked);
			info.AddValue ("TypeName", name);
		}

		public override string ToString()
		{
			return displayname;
		}

		public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

		public event ToolboxComponentsCreatingEventHandler ComponentsCreating;
	}
}
