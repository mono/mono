using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// The collection of <see cref="BaseControl"/> instances used by <see cref="FormRequest"/>.
	/// </summary>
	/// <seealso cref="BaseControl"/>
	/// <seealso cref="FormRequest"/>
	[Serializable]
	public sealed class BaseControlCollection : NameObjectCollectionBase 
	{
		/// <summary>
		/// The default constructor; does nothing.
		/// </summary>
		public BaseControlCollection ()
		{
		}

		/// <summary>
		/// The constructor is necessary because this class overrides
		/// <see cref="System.Collections.Specialized.NameObjectCollectionBase"/>,
		/// which makes a custom serialization.
		/// </summary>
		/// <param name="info">Serialization information.</param>
		/// <param name="context">Serialization context.</param>
		/// <seealso cref="System.Collections.Specialized.NameObjectCollectionBase"/>
		public BaseControlCollection (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		/// <summary>
		/// Sets or gets the control with the given name. The <c>get</c> ??? is guaranteed
		/// to return a non-null value.
		/// </summary>
		public BaseControl this [string name]
		{
			get {return base.BaseGet (name) as BaseControl;}
			set {base.BaseSet (name, value);}
		}

		/// <summary>
		/// Removes a control from the collection.
		/// </summary>
		/// <param name="name">The name of the control to remove.</param>
		public void Remove (string name)
		{
			base.BaseRemove (name);
		}

		/// <summary>
		/// Adds a new control to the collection. If there is a control with
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
		/// Adds a new control to the collection. If there is control with
		/// the same name, it will be overwritten.
		/// </summary>
		/// <param name="control">New control.</param>
		public void Add (BaseControl control)
		{
			this [control.Name] = control;
		}
	}
}
