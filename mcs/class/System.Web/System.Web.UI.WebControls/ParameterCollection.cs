//
// System.Web.UI.WebControls/ParameterCollection.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

#if NET_1_2

using System.Web.UI;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{

	public class ParameterCollection : StateManagedCollection
	{

		private static Type[] _knownTypes = new Type[] {
		                                    typeof (ControlParameter),
						    typeof (CookieParameter),
						    typeof (FormParameter),
						    typeof (Parameter),
						    typeof (QueryStringParameter),
						    typeof (SessionParameter) };
						    
		private EventHandler _parametersChanged;
		private KeyedList _values;


		public int Add (Parameter param)
		{
			return base.Add (param);
		}

		public int Add (string name, string value)
		{
			return base.Add (new Parameter (name, 1, value));
		}

		public int Add (string name, TypeCode type, string value)
		{
			return base.Add (new Parameter (name, type, value));
		}

		protected override object CreateKnownType (int idx)
		{
			switch (idx) {
			case 0:
				return new ControlParameter ();
				break;
			case 1:
				return new CookieParameter ();
				break;
			case 2:
				return new FormParameter ();
				break;
			case 3:
				return new Parameter ();
				break;
			case 4:
				return new QueryStringParameter ();
				break;
			case 5:
				return new SessionParameter ();
				break;
			}

			throw new ArgumentOutOfRangeException ("index");
		}

		protected override Type[] GetKnownTypes ()
		{
			return _knownTypes;
		}

		public IOrderedDictionary GetValues (Control control)
		{
			if (_values == null)
			{
				_values = new KeyedList ();
				foreach (Parameter param in this)
				{
					string name = param.Name;
					for (int i = 1; _values.Contains (name); i++)
					{
						name = param.Name + i.ToString ();
					}
					_values.Add (name, param.ParameterValue);
				}
			}
			return _values;
		}
		
		public void Insert (int idx, Parameter param)
		{
			base.Insert (idx, param);
		}

		protected override void OnClearComplete ()
		{
			base.OnClearComplete ();
			OnParametersChanged (EventArgs.Empty);
		}

		protected override void OnInsert (int idx, object value)
		{
			base.OnInsert (idx, value);
			((Parameter)value).SetOwnerCollection (this);
		}

		protected override void OnInsertComplete (int idx, object value)
		{
			base.OnInsertComplete (idx, value);
			OnParametersChanged (EventArgs.Empty);
		}

		protected virtual void OnParametersChanged (EventArgs e)
		{
			if (_parametersChanged != null)
				_parametersChanged.Invoke (this, e);
			
			_values = null;
		}

		protected override void OnValidate (object o)
		{
			base.OnValidate (o);
			
			if (!o is Parameter)
				throw new ArgumentException ("o is not a Parameter");
		}

		public void Remove (Parameter param)
		{
			base.Remove (param);
		}

		public void RemoveAt (int idx)
		{
			base.RemoveAr (idx);
		}

		protected override void SetDirtyObject (object o)
		{
			((Parameter)o).SetDirty();
		}

		internal void ParametersChanged ()
		{
			OnParametersChanged (EventArgs.Empty);
		}

		private int Contains (string name)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].Name == name)
					return i;
			}
			return -1;
		}

		public Parameter this[int idx] {
			get {
				return (Parameter) base[idx];
			}
			set {
				base[idx] = value;
			}
		}

		public Parameter this[string name] {
			get {
				int idx = IndexOfString (name);
				if (idx == -1)
					return null;
				return this[idx];
			}
			set {
				int idx = IndexOfString (name);
				if (idx == -1) {
					Add (value);
					return;
				}
				this[idx] = value;
			}
		}

		public event EventHandler ParametersChanged {
			get { _parametersChanged += value; }
			set { _parametersChanged -= value; }
		}
			
	}
}

#endif
