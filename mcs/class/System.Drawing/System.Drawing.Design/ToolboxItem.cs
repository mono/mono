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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Drawing.Design 
{
	[Serializable]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class ToolboxItem : ISerializable
	{

		private AssemblyName assembly;
		private Bitmap bitmap = null;
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
			if (locked)
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

		[MonoTODO ("get error handling logic correct")] 
		protected virtual IComponent[] CreateComponentsCore (IDesignerHost host)
		{
			if (host == null)
				throw new ArgumentNullException("host");

			OnComponentsCreating(new ToolboxComponentsCreatingEventArgs(host));
			
			IComponent[] components;
			Type type = GetType(host, assembly, name, true);
			if (type == null)
				components = new IComponent[] { };
			else
				components = new IComponent[] { host.CreateComponent(type) };

			OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(components));
			return components;
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
			if (host == null)
				throw new ArgumentNullException("host");

			//get ITypeResolutionService from host, as we have no other IServiceProvider here
			ITypeResolutionService typeRes = host.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
			if (typeRes == null)
				throw new Exception("Host does not provide an ITypeResolutionService");

			//TODO: Using Assembly loader to throw errors. Silent fail and return null?
			Assembly assembly = typeRes.GetAssembly(assemblyName, true);
			if (reference)
				typeRes.ReferenceAssembly(assemblyName);
			return typeRes.GetType(typeName, true);
		}

		[MonoTODO ("Should we be returning empty bitmap, or null?")]
		public virtual void Initialize (Type type) 
		{
			assembly = type.Assembly.GetName();
			displayname = type.Name;
			name = type.FullName;
			
			// seems to be a right place to create the bitmap
			System.Drawing.Image image = null;
			foreach (object attribute in type.GetCustomAttributes(true)) {
				ToolboxBitmapAttribute tba = attribute as ToolboxBitmapAttribute;
				if (tba != null) {
					image = tba.GetImage (type);
					break;
				}
			}
			//fallback: check for image even if not attribute
			if (image == null)
				image = ToolboxBitmapAttribute.GetImageFromResource (type, null, false);
			
			if (image != null) {
				if (image is Bitmap)
					bitmap = (Bitmap) image;
				else
					bitmap = new Bitmap (image);
			}

			filter = type.GetCustomAttributes (typeof (ToolboxItemFilterAttribute), true);
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
