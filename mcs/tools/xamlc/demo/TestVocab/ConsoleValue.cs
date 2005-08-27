using System;
using System.Globalization;
using System.ComponentModel;


namespace Xaml.TestVocab.Console {
	
	[TypeConverter(typeof(ConsoleValueConverter))]
	public abstract class ConsoleValue {
		public abstract string Value { get; }


		public override bool Equals(Object o)
		{
			return (((ConsoleValue)o).Value == Value);
		}
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	public class ConsoleValueConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			return (t == typeof(ConsoleValue));
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
		{
			return (t == typeof(string)) || (t == typeof(int));
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
			if (destinationType != typeof(string) &&
					destinationType != typeof(int))
				throw new NotSupportedException();

			if (o is ConsoleValue) {
				if (destinationType == typeof(string))
					return ((ConsoleValue)o).Value;
				else
					return Int32.Parse(((ConsoleValue)o).Value);
			} else {
				throw new NotSupportedException();
			}
		}
	}

	

	public class ConsoleValueString : ConsoleValue {
		string val;

		public ConsoleValueString()
		{
			this.val = "";
		}
		public ConsoleValueString(string val)
		{
			this.val = val;
		}

		public override string Value {
			get { return val; }
		}

		public string Text {
			set { val = value; }
		}

	}

	public class ConsoleValueAppend : ConsoleValue {
		ConsoleValue a, b;
		public ConsoleValueAppend(ConsoleValue a, ConsoleValue b)
		{
			this.a = a;
			this.b = b;
		}

		public override string Value {
			get { return a.Value + b.Value; }
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

		public override bool Equals(object o)
		{
			return (((ConsoleValueVar)o).var == var);
		}
		public override int GetHashCode()
		{
			return var.GetHashCode();
		}
	}

}
