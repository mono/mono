using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;

namespace Test.Elements
{
    public class FactoryMade: Element
    {
        public readonly string Label;
        public string Title { get; set; }
        public int IntProp { get; set; }
        public Element Element { get; set; }

        public FactoryMade(string label) 
        {
            Label = label;
        }

        public FactoryMade(Element element)
        {
            Element = element;
        }

        public FactoryMade(string label1, string label2)
        {
            Label = label1;
        }

        public static FactoryMade Create()
        {
            return new FactoryMade("default");
        }

        public static FactoryMade Create(string label)
        {
            return new FactoryMade(label);
        }

        public static FactoryMade Create(Element element)
        {
            return new FactoryMade(element);
        }

        public static FactoryMade Create(string label1, string label2)
        {
            return new FactoryMade(label1, label2);
        }

        public static FactoryMade CreateE()
        {
            throw new Exception("test");
        }

        public static Object CreateI(Object o)
        {
            return o;
        }
    }

    public class FactoryMarkupExtension : MarkupExtension
    {
        private string _label;

        public FactoryMarkupExtension(string label) 
        {
            _label = label;
        }

        public static FactoryMarkupExtension Create()
        {
            return new FactoryMarkupExtension("default");
        }

        public static FactoryMarkupExtension Create(string label)
        {
            return new FactoryMarkupExtension(label);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new FactoryMade(_label);
        }
    }

    public class FactoryGenericMarkupExtension<T> : MarkupExtension
    {
        private T _thing;

        public FactoryGenericMarkupExtension(T thing) 
        {
            _thing = thing;
        }

        public static FactoryGenericMarkupExtension<T> Create()
        {
            return new FactoryGenericMarkupExtension<T>(default(T));
        }

        public static FactoryGenericMarkupExtension<T> Create(T thing)
        {
            return new FactoryGenericMarkupExtension<T>(thing);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new FactoryMade((_thing==null)?"default":_thing.ToString());
        }
    }

    internal class FactoryInternal : Element, IHaveNoPrivacy
    {
        public readonly string Label;
        public string CreationOverload { get; private set; }
        public Element Element { get; set; }

        internal FactoryInternal(string label)
        {
            Label = label;
            CreationOverload="ctor(string)";
        }

        internal FactoryInternal(Element element)
        {
            Element = element;
            CreationOverload="ctor(Element)";
        }

        public static FactoryInternal Create()
        {
            FactoryInternal result = new FactoryInternal("default");
            result.CreationOverload="Create()";
            return result;
        }

        public static FactoryInternal Create(string label)
        {
            FactoryInternal result = new FactoryInternal(label);
            result.CreationOverload="Create(string)";
            return result;
        }
    
        object IHaveNoPrivacy.GetValue(string memberName)
        {
 	        switch (memberName)
            {
                case "Label":
                    return Label;
                case "CreationOverload":
                    return CreationOverload;
                case "Element":
                    return Element;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    public class FactoryNullable
    {
        public readonly int? Value;

        public FactoryNullable(int? value)
        {
            Value = value;
        }
    }

    public class FactoryProvider
    {
        public static FactoryMade Create() 
        {
            return new FactoryMade("Foo");
        }

        public static FactoryMade Create(string label)
        {
            return new FactoryMade(label);
        }
    }
}

namespace Test.Elements2
{
    public class OtherFactoryProvider
    {
        public static Test.Elements.FactoryMade Create()
        {
            return new Test.Elements.FactoryMade("Foo");
        }
    }

    public class Test1:Test.Elements.Element
    {
        public Test1() { }
    }
}
