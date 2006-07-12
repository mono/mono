using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// This is a container for a control used by <seealso cref="FormRequest"/>.
	/// </summary>
	[Serializable]
	public class BaseControl
	{
		/// <summary>
		/// Default constructor. Does nothing.
		/// </summary>
		public BaseControl ()
		{
		}

		/// <summary>
		/// Creates a <see cref="BaseControl"/> instance, initializing the
		/// <seealso cref="Name"/> and <seealso cref="Value"/> properties with
		/// the given values.
		/// </summary>
		/// <param name="name">The name of the control.</param>
		/// <param name="value">The value of the control.</param>
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
		/// Returns true, if the control is valid for submission. Override
		/// to implement different controls validation. See
		/// <a href="http://www.w3.org/TR/REC-html40/interact/forms.html#successful-controls">http://www.w3.org/TR/REC-html40/interact/forms.html#successful-controls</a>
		/// </summary>
		public virtual bool IsSuccessful () 
		{
			return true;
		}
	}
}
