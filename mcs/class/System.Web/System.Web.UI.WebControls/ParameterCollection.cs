//
// System.Web.UI.WebControls/ParameterCollection.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

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

#if NET_2_0

using System.Web.UI;
using System.Collections;
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
			return ((IList)this).Add (param);
		}

		public int Add (string name, string value)
		{
			return ((IList)this).Add (new Parameter (name, TypeCode.Object, value));
		}

		public int Add (string name, TypeCode type, string value)
		{
			return ((IList)this).Add (new Parameter (name, type, value));
		}

		protected override object CreateKnownType (int idx)
		{
			switch (idx) {
			case 0:
				return new ControlParameter ();
			case 1:
				return new CookieParameter ();			
			case 2:
				return new FormParameter ();			
			case 3:
				return new Parameter ();		
			case 4:
				return new QueryStringParameter ();		
			case 5:
				return new SessionParameter ();			
			}

			throw new ArgumentOutOfRangeException ("index");
		}

		protected override Type[] GetKnownTypes ()
		{
			return _knownTypes;
		}

		[MonoTODO ("Take care of HTTPContext parameter")]
		public IOrderedDictionary GetValues (HttpContext context, Control control)
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
			((IList)this).Insert (idx, param);
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
				_parametersChanged(this, e);
			
			_values = null;
		}

		protected override void OnValidate (object o)
		{
			base.OnValidate (o);
			
			if ((o is Parameter) == false)
				throw new ArgumentException ("o is not a Parameter");
		}

		public void Remove (Parameter param)
		{
			((IList)this).Remove (param);
		}

		public void RemoveAt (int idx)
		{
			((IList)this).RemoveAt (idx);
		}

		protected override void SetDirtyObject (object o)
		{
			Parameter param = (Parameter)o;
			if (Contains (param))
				param.SetDirty ();
		}

		internal void CallOnParameterChanged ()
		{
			OnParametersChanged (EventArgs.Empty);
		}

		private int IndexOfString (string name)
		{
			for (int i = 0; i < Count; i++)
			{
				if (((Parameter)((IList)this)[i]).Name == name)
					return i;
			}
			return -1;
		}

		public Parameter this[int idx] {
			get {
				return (Parameter) ((IList)this)[idx];
			}
			set {
				((IList)this)[idx] = value;
			}
		}

		public Parameter this[string name] {
			get {
				int idx = IndexOfString (name);
				if (idx == -1)
					return null;
				return ((Parameter) ((IList)this)[idx]);
			}
			set {
				int idx = IndexOfString (name);
				if (idx == -1) {
					Add (value);
					return;
				}
				((IList)this)[idx] = value;
			}
		}

		public event EventHandler ParametersChanged {
			add { _parametersChanged += value; }
			remove { _parametersChanged -= value; }
		}

		public bool Contains (Parameter param)
		{
			return ((IList)this).Contains (param);
		}

		public void CopyTo (Parameter[] paramArray, int index)
		{
			((IList)this).CopyTo (paramArray, index);
		}

		public int IndexOf (Parameter param)
		{
			return ((IList)this).IndexOf (param);
		}

		protected override void OnRemoveComplete (int index, object value)
		{
			base.OnRemoveComplete (index, value);			
			OnParametersChanged (EventArgs.Empty);
		}
		
	}
}

#endif
