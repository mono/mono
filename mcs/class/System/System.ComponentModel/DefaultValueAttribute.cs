using System;

namespace System.ComponentModel
{
	/// <summary>
	/// Specifies the default value for a property.
	/// </summary>

	[MonoTODO("Needs testing. DefaultValueAttribute(System.Type type, string value) is not implemented. Value has no description.")]
	[AttributeUsage(AttributeTargets.All)]
	public sealed class DefaultValueAttribute : Attribute
	{

		private object defaultValue;

		/// <summary>
		/// FIXME: Summary description for Value.
		/// </summary>
		public object Value
		{
			get 
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class.
		/// </summary>
		/// <param name="value">An System.Object that represents the default value.</param>
		public DefaultValueAttribute(object value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a System.Boolean value.
		/// </summary>
		/// <param name="value">An System.Boolean that represents the default value.</param>
		public DefaultValueAttribute(bool value)
		{
			defaultValue = value;
		}


		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using an 8-bit unsigned integer.
		/// </summary>
		/// <param name="value">An 8-bit unsigned integer that is the default value.</param>
		public DefaultValueAttribute(byte value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a Unicode character.
		/// </summary>
		/// <param name="value">A Unicode character that is the default value.</param>
		public DefaultValueAttribute(char value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a double-precision floating point number.
		/// </summary>
		/// <param name="value">A double-precision floating point number that is the default value.</param>
		public DefaultValueAttribute(double value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a 32-bit signed integer.
		/// </summary>
		/// <param name="value">A 32-bit signed integer that is the default value.</param>
		public DefaultValueAttribute(int value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a 64-bit signed integer.
		/// </summary>
		/// <param name="value">A 64-bit signed integer that is the default value.</param>
		public DefaultValueAttribute(long value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a 16-bit signed integer.
		/// </summary>
		/// <param name="value">A 16-bit signed integer that is the default value.</param>
		public DefaultValueAttribute(short value)
		{
			defaultValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a single-precision floating point number.
		/// </summary>
		/// <param name="value">A single-precision floating point number that is the default value.</param>
		public DefaultValueAttribute(System.Single value)
		{
			defaultValue = value;
		}

		/// <summary>	
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class using a System.String.
		/// </summary>
		/// <param name="value">A System.String that is the default value.</param>
		public DefaultValueAttribute(string value)
		{
			defaultValue = value;
		}

		/*
		/// <summary>	
		/// Initializes a new instance of the System.ComponentModel.DefaultValueAttribute class, converting the specified value to the specified type, and using an invariant culture as the translation context.
		/// </summary>
		/// <param name="type">A System.Type that represents the type to convert the value to.</param>
		/// <param name="value">A System.String that can be converted to the type using the System.ComponentModel.TypeConverter for the type and the U.S. English culture.</param>
		public DefaultValueAttribute(System.Type type, string value)
		{
			//FIXME
			throw new NotImplementedException(); 
		}
		*/
	}

}

