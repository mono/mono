//
// System.Drawing.Design.ToolboxItem.cs
//
// Authors:
//   Alejandro Sánchez Acosta
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jordi Mas i Hernandez, jordimash@gmail.com
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
	[MonoTODO ("Implementation is incomplete.")]
	public class ToolboxItem : ISerializable {

		private bool locked = false;
		private Hashtable properties = new Hashtable ();
		
		public ToolboxItem ()
		{
		}

		public ToolboxItem (Type toolType)
		{
			Initialize (toolType);
		}

		public AssemblyName AssemblyName {
			get { return (AssemblyName) properties["AssemblyName"]; }
			set { SetValue ("AssemblyName", value); }
		}

		public Bitmap Bitmap {
			get { return (Bitmap) properties["Bitmap"]; }
			set { SetValue ("Bitmap", value); }
		}

		public string DisplayName {
			get { return GetValue ("DisplayName"); }
			set { SetValue ("DisplayName", value); }
		}

		public ICollection Filter {
			get {
				ICollection filter = (ICollection) properties["Filter"];
				if (filter == null)
					filter = new ToolboxItemFilterAttribute[0];
				return filter;
			}
			set { SetValue ("Filter", value); }
		}
#if NET_2_0
		public virtual bool Locked {
#else		
		protected bool Locked {
#endif		
			get { return locked; }
		}

		public string TypeName {
			get { return GetValue ("TypeName"); }
			set { SetValue ("TypeName", value); }
		}
#if NET_2_0
		public string Company {
			get { return (string) properties["Company"]; }
			set { SetValue ("Company", value); }
		}

		public virtual string ComponentType {
			get { return ".NET Component"; }
		}

		public AssemblyName[] DependentAssemblies {
			get { return (AssemblyName[]) properties["DependentAssemblies"]; }
			set {
				AssemblyName[] names = new AssemblyName [value.Length];
				for (int i=0; i < names.Length; i++)
					names [i] = value [i];
				SetValue ("DependentAssemblies", names);
			}
		}

		public string Description {
			get { return (string) properties["Description"]; }
			set { SetValue ("Description", value); }
		}

		public bool IsTransient {			
			get {
				object o = properties ["IsTransient"];
				return (o == null) ? false : (bool) o;
			}
			set { SetValue ("IsTransient", value); }
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
			OnComponentsCreated (new ToolboxComponentsCreatedEventArgs (Comp));
			return Comp;
		}

		// FIXME - get error handling logic correct
		protected virtual IComponent[] CreateComponentsCore (IDesignerHost host)
		{
			if (host == null)
				throw new ArgumentNullException("host");
			
			IComponent[] components;
			Type type = GetType(host, AssemblyName, TypeName, true);
			if (type == null)
				components = new IComponent[] { };
			else
				components = new IComponent[] { host.CreateComponent(type) };

			return components;
		}

#if NET_2_0
		protected virtual IComponent[] CreateComponentsCore (IDesignerHost host, IDictionary defaultValues)
		{
			IComponent[] components = CreateComponentsCore (host);
			foreach (Component c in components) {
				IComponentInitializer initializer = host.GetDesigner (c) as IComponentInitializer;
				initializer.InitializeNewComponent (defaultValues);
			}
			return components;
		} 

		public IComponent[] CreateComponents (IDesignerHost host, IDictionary defaultValues)
		{
			OnComponentsCreating (new ToolboxComponentsCreatingEventArgs (host));
			IComponent[] components = CreateComponentsCore (host,  defaultValues);
			OnComponentsCreated (new ToolboxComponentsCreatedEventArgs (components));

			return components;
		} 

		protected virtual object FilterPropertyValue (string propertyName, object value)
		{
			switch (propertyName) {
			case "AssemblyName":
				return (value == null) ? null : (value as ICloneable).Clone ();
			case "DisplayName":
			case "TypeName":
				return (value == null) ? String.Empty : value;
			case "Filter":
				return (value == null) ? new ToolboxItemFilterAttribute [0] : value;
			default:
				return value;
			}
		}
#endif

		protected virtual void Deserialize (SerializationInfo info, StreamingContext context)
		{			
			AssemblyName = (AssemblyName)info.GetValue ("AssemblyName", typeof (AssemblyName));
			Bitmap = (Bitmap)info.GetValue ("Bitmap", typeof (Bitmap));
			Filter = (ICollection)info.GetValue ("Filter", typeof (ICollection));
			DisplayName = info.GetString ("DisplayName");
			locked = info.GetBoolean ("Locked");
			TypeName = info.GetString ("TypeName");
		}

		// FIXME: too harsh??
		public override bool Equals (object obj)
		{
			ToolboxItem ti = (obj as ToolboxItem);
			if (ti == null)
				return false;
			if (obj == this)
				return true;
			return (ti.AssemblyName.Equals (AssemblyName) &&
				ti.Locked.Equals (locked) &&
				ti.TypeName.Equals (TypeName) &&
				ti.DisplayName.Equals (DisplayName) &&
				ti.Bitmap.Equals (Bitmap));
		}
		
		public override int GetHashCode ()
		{
			// FIXME: other algorithm?
			return string.Concat (TypeName, DisplayName).GetHashCode ();
		}

#if NET_2_0
		public Type GetType (IDesignerHost host)
		{
			return GetType (host, this.AssemblyName,  this.TypeName,  false);
		}
#endif

		protected virtual Type GetType (IDesignerHost host, AssemblyName assemblyName, string typeName, bool reference)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			if (host == null)
				return null;

			//get ITypeResolutionService from host, as we have no other IServiceProvider here
			ITypeResolutionService typeRes = host.GetService (typeof (ITypeResolutionService)) as ITypeResolutionService;
			Type type = null;
			if (typeRes != null) {
				//TODO: Using Assembly loader to throw errors. Silent fail and return null?
				typeRes.GetAssembly (assemblyName, true);
				if (reference)
					typeRes.ReferenceAssembly (assemblyName);
				type = typeRes.GetType (typeName, true);
			} else {
				Assembly assembly = Assembly.Load (assemblyName);
				if (assembly != null)
					type = assembly.GetType (typeName);
			}
			return type;
		}

		// FIXME - Should we be returning empty bitmap, or null?
		public virtual void Initialize (Type type) 
		{
			CheckUnlocked ();
			if (type == null)
				return;

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
				Bitmap = (image as Bitmap);
				if (Bitmap == null)
					Bitmap = new Bitmap (image);
			}

			Filter = type.GetCustomAttributes (typeof (ToolboxItemFilterAttribute), true);
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
			if (ComponentsCreating != null)
				this.ComponentsCreating (this, args);
		}

		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("AssemblyName", AssemblyName);
			info.AddValue ("Bitmap", Bitmap);
			info.AddValue ("Filter", Filter);
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
			if (!allowNull && (value == null))
				throw new ArgumentNullException ("value");

			if ((value != null) && !expectedType.Equals (value.GetType ())) {
				string msg = Locale.GetText ("Type mismatch between value ({0}) and expected type ({1}).",
					value.GetType (), expectedType);
				throw new ArgumentException (msg, "value");
			}
		}

		protected virtual object ValidatePropertyValue (string propertyName, object value)
		{
			switch (propertyName) {
			case "AssemblyName":
				ValidatePropertyType (propertyName, value, typeof (AssemblyName), true);
				break;
			case "Bitmap":
				ValidatePropertyType (propertyName, value, typeof (Bitmap), true);
				break;
			case "Company":
			case "Description":
			case "DisplayName":
			case "TypeName":
				ValidatePropertyType (propertyName, value, typeof (string), true);
				if (value == null)
					value = String.Empty;
				break;
			case "IsTransient":
				ValidatePropertyType (propertyName, value, typeof (bool), false);
				break;
			case "Filter":
				ValidatePropertyType (propertyName, value, typeof (ToolboxItemFilterAttribute[]), true);
				if (value == null)
					value = new ToolboxItemFilterAttribute [0];
				break;
			case "DependentAssemblies":
				ValidatePropertyType (propertyName, value, typeof (AssemblyName[]), true);
				break;
			default:
				break;
			}
			return value;
		}

		private void SetValue (string propertyName, object value)
		{
			CheckUnlocked ();
			properties [propertyName] = ValidatePropertyValue (propertyName, value);
		}
#else
		private void SetValue (string propertyName, object value)
		{
			CheckUnlocked ();
			properties [propertyName] = value;
		}
#endif
		private string GetValue (string propertyName)
		{
			string s = (string) properties [propertyName];
			return (s == null) ? String.Empty : s;
		}

		public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

		public event ToolboxComponentsCreatingEventHandler ComponentsCreating;
	}
}
