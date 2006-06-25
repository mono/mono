using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.SystemWeb.Framework
{
	public class BaseControlCollection : NameObjectCollectionBase 
	{
		public BaseControl this [string name]
		{
			get {return base.BaseGet (name) as BaseControl;}
			set {base.BaseSet (name, value);}
		}

		public void Remove (string name)
		{
			base.BaseRemove (name);
		}

		public void Add (BaseControl control)
		{
			base.BaseAdd (control.Name, control);
		}

	}
}
