// System.Drawing.Design.ToolboxItem.cs
//
// Author:
//	Alejandro Sánchez Acosta
//
// (C) Alejandro Sánchez Acosta

using System.Collections;
using System.ComponentModel;
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
		private ICollection filter;
		private string displayname;
		private bool locked;
		private string name;
		
		[MonoTODO]
		public ToolboxItem() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ToolboxItem (Type toolType) {
			throw new NotImplementedException ();
		}

		public AssemblyName AssemblyName {
			get {
				return assembly;
			}

			set {
				assembly = value;
			}
		}

		public Bitmap Bitmap {
			get {
				return bitmap;
			}
			
			set {
				bitmap = value;
			}
		}

		public string DisplayName {
			get {
				return displayname;
			}
			
			set {
				displayname = value;
			}
		}

		public ICollection Filter {
			get {
				return filter;
			}
			
			set {
				filter = value;
			}
		}
		
		protected bool Locked {
			get {
				return locked;
			}

			set {
				locked = value;
			}
		}

		public string TypeName {
			get {
				return name;
			}

			set {
				name = value;
			}
		}
		
		[MonoTODO]
		protected void CheckUnlocked ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IComponent[] CreateComponents () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IComponent[] CreateComponents (IDesignerHost host)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual Type GetType (IDesignerHost host, AssemblyName assemblyName, string typeName, bool reference)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Initialize (Type type) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]		
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Lock ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnComponentsCreated (ToolboxComponentsCreatedEventArgs args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnComponentsCreating (ToolboxComponentsCreatingEventArgs args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
	}
}
