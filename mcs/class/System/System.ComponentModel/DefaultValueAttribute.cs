//
// System.ComponentModel.DefaultValueAttribute.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class DefaultValueAttribute : Attribute
	{

		private object DefaultValue;

		public DefaultValueAttribute (bool value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (byte value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (char value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (double value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (short value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (int value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (long value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (object value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (float value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (string value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (Type type, string value)
		{
			// TODO check if this implementation is correct
			try {
				DefaultValue = Convert.ChangeType (value, type);
			}
			catch {
				DefaultValue = null;
			}
		}

		public object Value {
			get { return DefaultValue; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DefaultValueAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DefaultValueAttribute) obj).Value == DefaultValue;
		}

		public override int GetHashCode()
		{
			return DefaultValue.GetHashCode();
		}
	}
}
