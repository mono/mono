using System;
using System.Globalization;
using System.ComponentModel;


namespace Xaml.TestVocab.Console {
	
	[TypeConverter(typeof(ConsoleValueConverter))]
	public abstract class ConsoleValue {
		public abstract string Value { get; }
	}

	public class ConsoleValueConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			return (t == typeof(ConsoleValue));
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
		{
			return (t == typeof(string));
		}
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object o)
		{
			if (o is string)
				return new ConsoleValueString((string)o);
			else
				throw new NotSupportedException();
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object o, Type destinationType)
		{
			if (destinationType != typeof(string))
				throw new NotSupportedException();

			if (o is ConsoleValue)
				return ((ConsoleValue)o).Value;
			else
				throw new NotSupportedException();
		}
	}

	

	public class ConsoleValueString : ConsoleValue {
		string val;
		public ConsoleValueString(string val)
		{
			this.val = val;
		}

		public override string Value {
			get { return val; }
		}
	}

	public class ConsoleValueVar : ConsoleValue {
		string var;
		public ConsoleValueVar()
		{
		}

		public ConsoleValueVar(string var)
		{
			this.var = var;
		}

		public override string Value {
			get { return ConsoleVars.Get(var); }
		}

		public string Variable {
			get { return var; }
			set { var = value; }
		}
	}

}
