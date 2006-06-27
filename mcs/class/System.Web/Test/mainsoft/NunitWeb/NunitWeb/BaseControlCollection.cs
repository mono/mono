using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public sealed class BaseControlCollection : NameObjectCollectionBase 
	{
		public BaseControlCollection (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public BaseControlCollection ()
		{
		}

		/// <summary>
		/// Sets or gets the control with the given name. Get is guaranteed
		/// to return not null value
		/// </summary>
		public BaseControl this [string name]
		{
			get {return base.BaseGet (name) as BaseControl;}
			set {base.BaseSet (name, value);}
		}

		public void Remove (string name)
		{
			base.BaseRemove (name);
		}

		/// <summary>
		/// Add a new control to the collection. If there is control with
		/// the same name, it will be kept intact.
		/// </summary>
		/// <param name="name">The name of a control to be added.</param>
		public void Add (string name)
		{
			BaseControl bc = this[name];
			if (bc != null)
				return;
			bc = new BaseControl ();
			bc.Name = name;
			base.BaseAdd (name, bc);
		}

		/// <summary>
		/// Add a new control to the collection. If there is control with
		/// the same name, it will be overwritten.
		/// </summary>
		/// <param name="control">New control.</param>
		public void Add (BaseControl control)
		{
			this [control.Name] = control;
		}

	}
}
