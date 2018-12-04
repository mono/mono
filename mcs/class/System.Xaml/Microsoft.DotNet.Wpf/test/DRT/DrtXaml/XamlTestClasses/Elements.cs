using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows;
using System.Xaml;
using System.Security;

[assembly: AllowPartiallyTrustedCallers]
[assembly: XmlnsDefinition("http://testroot", "")]

public class ClassInRootNamespace
{
}

namespace Test.Elements
{
    public class Element
    {
    }

    public class DPElement : DependencyObject
    {
    }

    [ContentProperty("Element")]
    public class HoldsOneElement : Element
    {
        public Element Element { get; set; }
    }

    public class HoldsTwoElements : Element
    {
        public Element One { get; set; }
        public Element Two { get; set; }
    }
    
    [DebuggerDisplay("{Title}")]
    public class ElementWithTitle : Element
    {
        string _title;

        public ElementWithTitle()
        {
            _title = this.GetType().ToString();
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }

    [RuntimeNameProperty("RuntimeName")]
    [XmlLangProperty("XmlLang")]
    [UidProperty("Uid")]
    public class ElementWithDirectiveAliasedProperties : Element
    {
        public string RuntimeName { get; set; }
        public string XmlLang { get; set; }
        public string Uid { get; set; }
    }

    public class ElementWithNullableDouble : Element
    {
        public double? NullableDouble { get; set; }
    }

    public class Object10 : Element
    {
        public int Mask { get; set; }
        public Object Object0 { get; set; }
        public Object Object1 { get; set; }
        public Object Object2 { get; set; }
        public Object Object3 { get; set; }
        public Object Object4 { get; set; }
        public Object Object5 { get; set; }
        public Object Object6 { get; set; }
        public Object Object7 { get; set; }
        public Object Object8 { get; set; }
        public Object Object9 { get; set; }
    }

    public class Element10 : Element
    {
        public int Mask { get; set; }
        public Element Element0 { get; set; }
        public Element Element1 { get; set; }
        public Element Element2 { get; set; }
        public Element Element3 { get; set; }
        public Element Element4 { get; set; }
        public Element Element5 { get; set; }
        public Element Element6 { get; set; }
        public Element Element7 { get; set; }
        public Element Element8 { get; set; }
        public Element Element9 { get; set; }
    }

    public class ElementList : List<Element> { }

    public class ElementListHolder : Element
    {
        ElementList _elements;

        public ElementList Elements
        {
            get
            {
                if (_elements == null)
                {
                    _elements = new ElementList();
                }
                return _elements;
            }
        }
    }

    public class ElementWithWriteOnly : Element
    {
        private string _value;

        public string ReadOnly { get { return _value; } }

        public string WriteOnly { set { _value = value; } }
    }

    [ContentProperty("Content")]
    public class GenericElement<T> : Element
    {
        public T Content { get; set; }
        public string Color { get; set; }
    }

    [TypeConverter(typeof(ColorElementConverter))]
    public class ColorElement : Element
    {
        public string ColorName { get; set; }
    }

    [TypeConverter(typeof(ColorElement2Converter))]
    public class ColorElement2 : ColorElement
    {
    }

    public class ColorHolder : Element
    {
        public ColorElement Color { get; set; }

        public void SetColor(ColorElement color)
        {
            Color = new ColorElement { ColorName = color.ColorName + "x" };
        }
    }

    public class StaticColorElements
    {
        static ColorElement _red;
        static ColorElement _green;
        static ColorElement _blue;
        static Dictionary<string, Element> _dict;

        static StaticColorElements()
        {
            _red = new ColorElement();
            _red.ColorName = "Red";

            _green = new ColorElement();
            _green.ColorName = "Green";

            _blue = new ColorElement();
            _blue.ColorName = "Blue";

            _dict = new Dictionary<string, Element>();
        }
        public static ColorElement Red { get { return _red; } }
        public static ColorElement Green { get { return _green; } }
        public static ColorElement Blue { get { return _blue; } }

        public static Dictionary<string, Element> ElementDictionary { get { return _dict; } }
    }

    public struct ColorStruct
    {
        public string Color { get; set; }
    }

    [TypeConverter(typeof(ColorElementConverter))]
    [ContentProperty("ColorNameCPA")]
    public class ColorElementDuel : ColorElement
    {
        public string ColorNameCPA { get; set; }
    }

    [TypeConverter(typeof(ColorListConverter))]
    public class ColorListTC : List<string>
    {
        public string MainColor { get; set; }
    }

    [ContentProperty("MainColor")]
    public class ColorListCPA : List<string>
    {
        public string MainColor { get; set; }
    }

    [ContentProperty("Color")]
    public class ColorElementCPA : Element
    {
        public ColorElement Color { get; set; }
    }

    [TypeConverter(typeof(System.Windows.Markup.NameReferenceConverter))]
    public class ColorNameRef : ColorElement
    {
    }

    internal class InternalObjectWithInternalDefaultCtor
    {
        public int X { get; set; }
        public int Y { get; set; }

        internal InternalObjectWithInternalDefaultCtor()
        {
        }
    }

    // Used to provide test classes with access to non-public members
    public interface IHaveNoPrivacy
    {
        object GetValue(string memberName);
    }

    internal class InternalElement : Element, IHaveNoPrivacy
    {
        private List<string> _list = new List<string>();

        public string PublicNameOfInternalType { get; set; }
        internal string InternalProperty { get; set; }

        internal ColorStruct InternalStructProperty { get; set; }

        internal List<string> InternalListProperty { get { return _list; } }

        object IHaveNoPrivacy.GetValue(string memberName)
        {
            switch (memberName)
            {
                case "PublicNameOfInternalType":
                    return PublicNameOfInternalType;
                case "InternalProperty":
                    return InternalProperty;
                case "InternalStructProperty":
                    return InternalStructProperty;
                case "InternalListProperty":
                    return InternalListProperty;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    public class ElementWithInternalProperty : Element, IHaveNoPrivacy
    {
        internal string InternalProperty { get; set; }

        public string InternalReadProperty { internal get; set; }
        public string InternalWriteProperty { get; internal set; }

        protected string ProtectedProperty { get; set; }
        private string PrivateProperty { get; set; }

        public Element NestedElement { get; set; }

        object IHaveNoPrivacy.GetValue(string memberName)
        {
            switch (memberName)
            {
                case "InternalProperty":
                    return InternalProperty;
                case "InternalReadProperty":
                    return InternalReadProperty;
                case "InternalWriteProperty":
                    return InternalWriteProperty;
                case "ProtectedProperty":
                    return ProtectedProperty;
                case "PrivateProperty":
                    return PrivateProperty;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    public class UsesInternalTypeConverter : Element
    {
        [TypeConverter(typeof(InternalTypeConverter))]
        public ElementWithTitle Element { get; set; }
    }

    public class EventElement
    {
        public string Foo { get; set; }
        public int TapEventCount { get; set; }
        public delegate void TapDelegate(object source, EventArgs args);

        public event TapDelegate Tap;

        public void RaiseTapEvent()
        {
            if (Tap != null)
            {
                EventArgs args = new EventArgs();
                Tap(this, args);
            }
        }

        internal event TapDelegate TapInternal;

        public void RaiseTapInternal()
        {
            if (TapInternal != null)
            {
                EventArgs args = new EventArgs();
                TapInternal(this, args);
            }
        }

        private void PrivateHandler(object source, EventArgs args)
        {
            TapEventCount++;
        }
    }

    public class EventElementWithHelper : EventElement
    {
        internal Delegate _CreateDelegate(Type delegateType, string handler)
        {
            return Delegate.CreateDelegate(delegateType, this, handler);
        }

        private void PrivateHandler(object source, EventArgs args)
        {
            TapEventCount++;
        }
    }

    public class AttachedEventHolder
    {
        static AttachableMemberIdentifier TapEvent = new AttachableMemberIdentifier(typeof(AttachedEventHolder), "TapEvent");

        public static void AddTapEventHandler(object target, EventElement.TapDelegate evt)
        {
            Delegate handlers;
            if (AttachablePropertyServices.TryGetProperty(target, TapEvent, out handlers))
            {
                handlers = Delegate.Combine(handlers, evt);
            }
            else
            {
                handlers = evt;
            }
            AttachablePropertyServices.SetProperty(target, TapEvent, handlers);
        }

        public static void RaiseTapEvent(object target)
        {
            Delegate handlers;
            if (AttachablePropertyServices.TryGetProperty(target, TapEvent, out handlers))
            {
                handlers.DynamicInvoke(target, new EventArgs());
            }
        }
    }

    public class HasAtt
    {
        public static List<string> GetFoo(object target) { return null; }
    }

    public class ElementWithSimpleProperties
    {
        public string String { get; set; }
        public double Double { get; set; }
    }

    public class DelegateCreatingME : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new EventElement.TapDelegate(incrementCount);
        }

        public static void incrementCount(object source, EventArgs args)
        {
            EventElement eventElement = (EventElement)source;
            eventElement.TapEventCount++;
        }
    }

    public class ElementHolderWithNameScope : HoldsOneElement, INameScope
    {
        Dictionary<string, object> _reg;

        public ElementHolderWithNameScope()
        {
            _reg = new Dictionary<string, object>();
        }

        #region INameScope Members

        public object FindName(string name)
        {
            object value;
            if (_reg.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        public void RegisterName(string name, object scopedElement)
        {
            _reg.Add(name, scopedElement);
        }

        public void UnregisterName(string name)
        {
            _reg.Remove(name);
        }

        #endregion
    }

    public class OrderedDictionary<K, V> : List<KeyValuePair<K, V>>, IDictionary
    {
        public void Add(K key, V value)
        {
            base.Add(new KeyValuePair<K, V>(key, value));
        }

        #region IDictionary Members

        void IDictionary.Add(object key, object value)
        {
            Add((K)key, (V)value);
        }

        void IDictionary.Clear()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        ICollection IDictionary.Keys
        {
            get { throw new NotImplementedException(); }
        }

        void IDictionary.Remove(object key)
        {
            throw new NotImplementedException();
        }

        ICollection IDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        object IDictionary.this[object key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        int ICollection.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

#pragma warning disable 67 // Unused event
    [ContentProperty("Content")]
    public class Shadowed
    {
        public object Value { get; set; }

        public event EventHandler Event;

        public string Content { get; set; }
    }

    public class Shadower : Shadowed
    {
        public new string Value { get; set; }

        public new event EventHandler<EventArgs> Event;

        public new string Content { get; set; }
    }
#pragma warning restore 67

    public class HasNested : Element
    {
        public static int StaticProp { get { return 5; } }
        public class NestedClass : Element
        {
            public static int StaticProp { get { return 10; } }
        }
        internal class InternalNestedClass
        {
        }
        protected class ProtectedNestedClass
        {
        }
        private class PrivateNestedClass
        {
        }
    }

    public class NameScopeElement : Element, INameScope
    {
        Dictionary<string, object> names = new Dictionary<string, object>();

        public Element StartNode
        { get; set; }

        public Element EndNode
        { get; set; }

        public object FindName(string name)
        {
            object value;
            if (names.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        public void RegisterName(string name, object scopedElement)
        {
            names[name] = scopedElement;
        }

        public void UnregisterName(string name)
        {
            names.Remove(name);
        }
    }

    public class TypeElement : Element
    {
        public Type TypeProperty { get; set; }
    }

    public class CustomResourceDictionary : ResourceDictionary
    {
    }

    public class CustomResourceDictionaryWithNameScopeButNoRegisterName : ResourceDictionary, INameScope
    {
    }

    public class CustomResourceDictionaryWithImplicitNameScope: ResourceDictionary, INameScope
    {
        public new void RegisterName(string name, object scopedElement)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomResourceDictionaryWithExplicitNameScope : ResourceDictionary, INameScope
    {
        #region INameScope Members

        object INameScope.FindName(string name)
        {
            throw new NotImplementedException();
        }

        void INameScope.RegisterName(string name, object scopedElement)
        {
            throw new NotImplementedException();
        }

        void INameScope.UnregisterName(string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
