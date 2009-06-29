//
// System.Web.UI.WebControls.Parameter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//      Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004 Novell, Inc. (http://www.novell.com)
//

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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[DefaultPropertyAttribute ("DefaultValue")]
	public class Parameter : ICloneable, IStateManager 
	{
		public Parameter () : base ()
		{
		}

		protected Parameter (Parameter original)
		{
			this.DefaultValue = original.DefaultValue;
			this.Direction = original.Direction;
			this.ConvertEmptyStringToNull = original.ConvertEmptyStringToNull;
			this.Type = original.Type;
			this.Name = original.Name;
		}
		
		public Parameter (string name)
		{
			this.Name = name;
		}
		
		public Parameter(string name, TypeCode type) : this (name)
		{
			this.Type = type;
		}
		
		public Parameter (string name, TypeCode type, string defaultValue) : this (name, type)
		{
			this.DefaultValue = defaultValue;
		}
		
		protected virtual Parameter Clone ()
		{
			return new Parameter (this);
		}
		
		protected void OnParameterChanged ()
		{
			if (_owner != null)
				_owner.CallOnParameterChanged ();
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			ViewState.LoadViewState (savedState);
		}
		
		protected virtual object SaveViewState ()
		{
			return ViewState.SaveViewState ();
		}
		
		protected virtual void TrackViewState ()
		{
			isTrackingViewState = true;
			if (viewState != null)
				viewState.TrackViewState ();
		}
		
		object ICloneable.Clone ()
		{
			return this.Clone ();
		}
		
		void IStateManager.LoadViewState (object savedState)
		{
			this.LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return this.SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			this.TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return this.IsTrackingViewState; }
		}
		
		// MSDN: The ToString method returns the Name property of the Parameter object. If the Parameter object has no name, ToString returns String.Empty.
		public override string ToString ()
		{
			return Name;
		}
		
		[WebCategoryAttribute ("Parameter")]
		[DefaultValueAttribute (null)]
		[WebSysDescriptionAttribute ("Default value to be used in case value is null.")]
		public string DefaultValue {
			get { return ViewState.GetString ("DefaultValue", null); }
			set {
				
				if (DefaultValue != value) {
					ViewState ["DefaultValue"] = value;
					OnParameterChanged ();
				}
			}
		}

		[WebCategoryAttribute ("Parameter")]
		[DefaultValueAttribute ("Input")]
		[WebSysDescriptionAttribute ("Parameter's direction.")]
		public ParameterDirection Direction
		{
			get { return (ParameterDirection) ViewState.GetInt ("Direction", (int)ParameterDirection.Input); }
			set {				
				if (Direction != value) {
					ViewState ["Direction"] = value;
					OnParameterChanged ();
				}
			}
		}


		[WebCategoryAttribute ("Parameter")]
		[DefaultValueAttribute ("")]
		[WebSysDescriptionAttribute ("Parameter's name.")]
		public string Name
		{
			get {
				string s = ViewState ["Name"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				
				if (Name != value) {
					ViewState ["Name"] = value;
					OnParameterChanged ();
				}
			}
		}

		[WebCategoryAttribute ("Parameter")]
		[DefaultValueAttribute (true)]
	        [WebSysDescriptionAttribute ("Checks whether an empty string is treated as a null value.")]
		public bool ConvertEmptyStringToNull
		{
			get { return ViewState.GetBool ("ConvertEmptyStringToNull", true); }
			set {
				if (ConvertEmptyStringToNull != value) {
					ViewState["ConvertEmptyStringToNull"] = value;
					OnParameterChanged ();
				}
			}
		}

		[WebCategoryAttribute ("Parameter")]
		[DefaultValueAttribute (DbType.Object)]
		[WebSysDescriptionAttribute ("Parameter's DbType.")]
		public DbType DbType
		{
			get {
				object o = ViewState ["DbType"];
				if (o == null)
					return DbType.Object;
				return (DbType) o;
			}
			set {
				if (DbType != value) {
					ViewState ["DbType"] = value;
					OnParameterChanged ();
				}
			}
		}

		[DefaultValue (0)]
		public int Size {
			get { return ViewState.GetInt ("Size", 0); }
			set {
				if (Size != value) {
					ViewState["Size"] = value;
					OnParameterChanged ();
				}
			}
		}

		[DefaultValueAttribute (TypeCode.Empty)]
		[WebCategoryAttribute ("Parameter"), 
		WebSysDescriptionAttribute("Represents type of the parameter.")]
		public TypeCode Type
		{
			get { return (TypeCode) ViewState.GetInt ("Type", (int)TypeCode.Empty); }
			set {
				
				if (Type != value) {
					ViewState ["Type"] = value;
					OnParameterChanged ();
				}
			}
		}
		
		StateBag viewState;
		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		protected StateBag ViewState {
			get {
				if (viewState == null) {
					viewState = new StateBag ();
					if (IsTrackingViewState)
						viewState.TrackViewState ();
				}
				return viewState;
			}
		}
		
		bool isTrackingViewState = false;
		protected bool IsTrackingViewState {
			get { return isTrackingViewState; }
		}

		// MSDN: The default implementation of the Evaluate method is to return 
		// a null reference (Nothing in Visual Basic) in all cases. 
		// Classes that derive from the Parameter class override the Evaluate method 
		// to return an updated parameter value. For example, the ControlParameter object 
		// returns the value of the control that it is bound to, while 
		// the QueryStringParameter object retrieves the current name/value pair from 
		// the HttpRequest object.
		protected virtual object Evaluate (HttpContext context, Control control)
		{
			return null;
		}

		internal void UpdateValue (HttpContext context, Control control)
		{
			object oldValue = ViewState ["ParameterValue"];

			object newValue = Evaluate (context, control);

			if (!object.Equals (oldValue, newValue)) {
				ViewState ["ParameterValue"] = newValue;
				OnParameterChanged ();
			}
		}

		internal object GetValue (HttpContext context, Control control)
		{
			UpdateValue (context, control);

			object value = ConvertValue (ViewState ["ParameterValue"]);
			if (value == null)
				value = ConvertValue (DefaultValue);

			return value;
		}
		
		internal object ConvertValue (object val)
		{
			if (val == null) return null;
			if (ConvertEmptyStringToNull && val.Equals (string.Empty))
				return null;
			return Type != TypeCode.Empty ? Convert.ChangeType (val, Type) : val;
		}
		
		protected internal virtual void SetDirty()
		{
			ViewState.SetDirty (true);
		}

		ParameterCollection _owner;
		internal void SetOwnerCollection (ParameterCollection own)
		{
			_owner = own;
		}
	}
}
#endif

