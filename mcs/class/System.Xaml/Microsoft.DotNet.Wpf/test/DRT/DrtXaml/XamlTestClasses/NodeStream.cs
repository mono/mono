using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Xaml;

namespace Test.NodeStream
{
    public abstract class Node
    {
    }

    public class EM : Node
    {
        public override bool Equals(Object o)
        {
            return o is EM;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "(EM)";
        }
    }

    public class EO : Node
    {
        public override bool Equals(Object o)
        {
            return o is EO;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "(EO)";
        }
    }

    public class GO : Node
    {
        public override bool Equals(Object o)
        {
            return o is GO;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "(GO)";
        }
    }

    public class NS : Node
    {
        public NamespaceDeclaration Value { get; set; }

        public NS(NamespaceDeclaration nd)
        {
            if (nd == null)
            {
                throw new ArgumentNullException("nd");
            }
            Value = nd;
        }

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            NS ns = o as NS;
            if (ns == null)
            {
                return false;
            }
            return (Value.Prefix != null && Value.Prefix.Equals(ns.Value.Prefix)) && (Value.Namespace != null && Value.Namespace.Equals(ns.Value.Namespace));
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            string s = "(NS ";
            if (Value.Prefix != null && Value.Prefix.Length > 0)
            {
                s += "\"" + Value.Prefix + "\" ";
            }
            return s + "\"" + Value.Namespace + "\")";
        }
    }

    public class SM : Node
    {
        public XamlMember Value { get; set; }

        public SM(XamlMember xm)
        {
            if (xm == null)
            {
                throw new ArgumentNullException("xm");
            }
            Value = xm;
        }

        public override bool Equals(Object o)
        {
            if (o == null)
            {
                return false;
            }
            SM sm = o as SM;
            if (sm == null)
            {
                return false;
            }
            return Value.Equals(sm.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "(SM \"" + Value.PreferredXamlNamespace + "\" \"" + Value.Name + "\")";
        }
    }

    public class SO : Node
    {
        public XamlType Value { get; set; }

        public SO(XamlType xt)
        {
            if (xt == null)
            {
                throw new ArgumentNullException("xt");
            }
            Value = xt;
        }

        public override bool Equals(Object o)
        {
            if (o == null)
            {
                return false;
            }
            SO so = o as SO;
            if (so == null)
            {
                return false;
            }
            return Value.Equals(so.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "(SO \"" + Value.PreferredXamlNamespace + "\" \"" + Value.Name + "\")";
        }
    }

    public class V : Node
    {
        public object Value { get; set; }

        public V(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            Value = o;
        }

        public override bool Equals(Object o)
        {
            if (o == null)
            {
                return false;
            }
            V v = o as V;
            if (v == null)
            {
                return false;
            }
            return Value.Equals(v.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "(V " + (Value is string ? "\"" + Value.ToString() + "\"" : Value.ToString()) + ")";
        }
    }

    public class Arguments
    {
        public string X { get; set; }

        public string Y { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o is Arguments)
            {
                return Equals(o as Arguments);
            }
            return false;
        }

        public bool Equals(Arguments o)
        {
            if (Y != null && !Y.Equals(o.Y))
            {
                return false;
            }
            return X.Equals(o.X);
        }
    }

    [TypeConverter(typeof(ConstructorArguments1TypeConverter))]
    public class ConstructorArguments1 : Arguments
    {
        public ConstructorArguments1(string x)
            : base()
        {
            X = x;
        }

        public override string ToString()
        {
            return string.Format("(ConstructorArguments1 (X \"{0}\"))", X);
        }
    }

    [TypeConverter(typeof(ConstructorArguments2TypeConverter))]
    public class ConstructorArguments2 : Arguments
    {
        public ConstructorArguments2(string x, string y)
            : base()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("(ConstructorArguments2 (X \"{0}\") (Y \"{1}\"))", X, Y);
        }
    }

    [TypeConverter(typeof(FactoryArguments1TypeConverter))]
    public class FactoryArguments1 : Arguments
    {
        private FactoryArguments1()
            : base()
        {
        }

        public static FactoryArguments1 Factory(string x)
        {
            return new FactoryArguments1() { X = x };
        }

        public override string ToString()
        {
            return string.Format("(FactoryArguments1 (X \"{0}\"))", X);
        }
    }

    [TypeConverter(typeof(FactoryArguments2TypeConverter))]
    public class FactoryArguments2 : Arguments
    {
        private FactoryArguments2()
            : base()
        {
        }

        public static FactoryArguments2 Factory(string x, string y)
        {
            return new FactoryArguments2() { X = x, Y = y };
        }

        public override string ToString()
        {
            return string.Format("(FactoryArguments2 (X \"{0}\") (Y \"{1}\"))", X, Y);
        }
    }

    public class ConstructorArguments1TypeConverter : TypeConverter
    {
        public ConstructorArguments1TypeConverter()
        {
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is ConstructorArguments1)
            {
                ConstructorArguments1 a = (ConstructorArguments1)value;
                ConstructorInfo ci = typeof(ConstructorArguments1).GetConstructor(new Type[] { typeof(string) });
                if (ci != null)
                {
                    return new InstanceDescriptor(ci, new object[] { a.X }, true);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ConstructorArguments2TypeConverter : TypeConverter
    {
        public ConstructorArguments2TypeConverter()
        {
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is ConstructorArguments2)
            {
                ConstructorArguments2 a = (ConstructorArguments2)value;
                ConstructorInfo ci = typeof(ConstructorArguments2).GetConstructor(new Type[] { typeof(string), typeof(string) });
                if (ci != null)
                {
                    return new InstanceDescriptor(ci, new object[] { a.X, a.Y }, true);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class FactoryArguments1TypeConverter : TypeConverter
    {
        public FactoryArguments1TypeConverter()
        {
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is FactoryArguments1)
            {
                FactoryArguments1 a = (FactoryArguments1)value;
                MethodInfo mi = typeof(FactoryArguments1).GetMethod("Factory", new Type[] { typeof(string) });
                if (mi != null)
                {
                    return new InstanceDescriptor(mi, new object[] { a.X }, true);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class FactoryArguments2TypeConverter : TypeConverter
    {
        public FactoryArguments2TypeConverter()
        {
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is FactoryArguments2)
            {
                FactoryArguments2 a = (FactoryArguments2)value;
                MethodInfo mi = typeof(FactoryArguments2).GetMethod("Factory", new Type[] { typeof(string), typeof(string) });
                if (mi != null)
                {
                    return new InstanceDescriptor(mi, new object[] { a.X, a.Y }, true);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
