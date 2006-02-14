//
// System.Drawing.Design.ToolboxItem.cs
//
// Authors:
//   Alejandro Sánchez Acosta
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jordi Mas i Hernandez, jordimash@gmail.com
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
		private bool locked = false;
		private ICollection filter = new ToolboxItemFilterAttribute[0];
		private Hashtable properties = new Hashtable ();
		
		public ToolboxItem() {
		}

		public ToolboxItem (Type toolType) {
			Initialize (toolType);
		}

		public AssemblyName AssemblyName {
			get {
				return (AssemblyName) properties["AssemblyName"];
			}

			set {
				CheckUnlocked ();
				properties["AssemblyName"] = value;
			}
		}

		public Bitmap Bitmap {
			get {
				return (Bitmap) properties["Bitmap"];
			}
			
			set {
				CheckUnlocked ();
				properties["Bitmap"] = value;
			}
		}

		public string DisplayName {
			get {
				return (string) properties["DisplayName"];
			}
			
			set {
				CheckUnlocked ();
				properties["DisplayName"] = value;
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
#if NET_2_0
		public virtual bool Locked {
#else		
		protected bool Locked {
#endif		
			get {
				return locked;
			}
		}

		public string TypeName {
			get {
				return (string) properties["TypeName"];
			}

			set {
				CheckUnlocked ();
				properties["TypeName"] = value;
			}
		}
#if NET_2_0
		public string Company {
			get { return (string) properties["Company"]; }
			set { properties["Company"] = value; }
		}

		public virtual string ComponentType {
			get { return "DotNET_ComponentType"; }
		}

		public AssemblyName[] DependentAssemblies {
			get { return (AssemblyName[]) properties["DependentAssemblies"]; }
			set { properties["DependentAssemblies"] = value; }
		}

		public string Description {
			get { return (string) properties["Description"]; }
			set { properties["Description"] = value; }
		}

		public bool IsTransient {			
			get { return (bool) properties["IsTransient"]; }
			set { properties["IsTransient"] = value; }
		}

		public IDictionary Properties {
			 get { return properties; }
		}

		public virtual string Version { 
			get { return string.Empty; }
		}				

#endif		
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
			Type type = GetType(host, AssemblyName, TypeName, true);
			if (type == null)
				components = new IComponent[] { };
			else
				components = new IComponent[] { host.CreateComponent(type) };

			OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(components));
			return components;
		}

#if NET_2_0
		[MonoTODO] 
		public IComponent[] CreateComponents (IDesignerHost host, IDictionary defaultValues)
		{
			throw new NotImplementedException ();
		} 

		[MonoTODO] 
		public Type GetType (IDesignerHost host)
		{
      			throw new NotImplementedException ();
		}

		[MonoTODO] 
		protected virtual object FilterPropertyValue(string propertyName, object value)
		{
			throw new NotImplementedException ();
		}
#endif

		protected virtual void Deserialize (SerializationInfo info, StreamingContext context)
		{			
			AssemblyName = (AssemblyName)info.GetValue ("AssemblyName", typeof (AssemblyName));
			Bitmap = (Bitmap)info.GetValue ("Bitmap", typeof (Bitmap));
			filter = (ICollection)info.GetValue ("Filter", typeof (ICollection));
			DisplayName = info.GetString ("DisplayName");
			locked = info.GetBoolean ("Locked");
			TypeName = info.GetString ("TypeName");
		}

		public override bool Equals (object obj)
		{
			// FIXME: too harsh??
			if (!(obj is ToolboxItem))
				return false;
			if (obj == this)
				return true;
			return ((ToolboxItem) obj).AssemblyName.Equals (AssemblyName) &&
				((ToolboxItem) obj).Locked.Equals (locked) &&
				((ToolboxItem) obj).TypeName.Equals (TypeName) &&
				((ToolboxItem) obj).DisplayName.Equals (DisplayName) &&
				((ToolboxItem) obj).Bitmap.Equals (Bitmap);
		}
		
		public override int GetHashCode ()
		{
			// FIXME: other algorithm?
			return string.Concat (TypeName, DisplayName).GetHashCode ();
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
			AssemblyName = type.Assembly.GetName();
			DisplayName = type.Name;
			TypeName = type.FullName;
			
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
					Bitmap = (Bitmap) image;
				else
					Bitmap = new Bitmap (image);
			}

			filter = type.GetCustomAttributes (typeof (ToolboxItemFilterAttribute), true);
		}
			
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			Serialize (info, context);
		}

#if NET_2_0
		public virtual void Lock () 
#else		
		public void Lock ()
#endif				
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
			info.AddValue ("AssemblyName", AssemblyName);
			info.AddValue ("Bitmap", Bitmap);
			info.AddValue ("Filter", filter);
			info.AddValue ("DisplayName", DisplayName);
			info.AddValue ("Locked", locked);
			info.AddValue ("TypeName", TypeName);
		}

		public override string ToString()
		{
			return DisplayName;
		}

#if NET_2_0
		protected void ValidatePropertyType (string propertyName, object value, Type expectedType, bool allowNull)
		{
			throw new NotImplementedException ();
		}

		protected virtual object ValidatePropertyValue (string propertyName, object value)
		{
			throw new NotImplementedException ();
		} 
#endif

		public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

		public event ToolboxComponentsCreatingEventHandler ComponentsCreating;
	}
}
