using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// This is a container for a control used by <see cref="FormRequest"/>.
	/// </summary>
	/// <seealso cref="FormRequest"/>
	[Serializable]
	public class BaseControl
	{
		/// <summary>
		/// Default constructor; does nothing.
		/// </summary>
		public BaseControl ()
		{
		}

		/// <summary>
		/// Creates a <see cref="BaseControl"/> instance, initializing the
		/// <see cref="Name"/> and <see cref="Value"/> properties with
		/// the given values.
		/// </summary>
		/// <param name="name">The name of the control.</param>
		/// <param name="value">The value of the control.</param>
		/// <seealso cref="Name"/>
		/// <seealso cref="Value"/>
		public BaseControl (string name, string value)
		{
			_name = name;
			_value = value;
		}

		string _name;
		/// <summary>
		/// The name of the control.
		/// </summary>
		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		string _value;
		/// <summary>
		/// The string value of the control.
		/// </summary>
		public virtual string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		/// Returns <c>true</c>, if the control is valid for submission. Override this method 
		/// to implement validation of different controls. See
		/// <see href="http://www.w3.org/TR/REC-html40/interact/forms.html#successful-controls"/>
		/// </summary>
		public virtual bool IsSuccessful () 
		{
			return true;
		}
	}
}
