using System;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class BaseControl
	{
		public BaseControl ()
		{
		}

		public BaseControl (string name, string value)
		{
			_name = name;
			_value = value;
		}

		string _name;
		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		string _value;
		public virtual string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		/// Returns true, if the control is valid for submission. Override
		/// to implement different controls validation. See
		/// http://www.w3.org/TR/REC-html40/interact/forms.html#successful-controls
		/// </summary>
		public virtual bool IsSuccessful () 
		{
			return true;
		}
	}
}
