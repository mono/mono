using System.ComponentModel;
using System.Xaml;
using System.Windows.Markup;
using System;

namespace Test.Elements
{
    public class InitializableBase : Element, ISupportInitialize 
    {
        bool _beginInitWasCalled;
        bool _endInitWasCalled;

        public bool BeginInitWasCalled { get { return _beginInitWasCalled; } }
        public bool EndInitWasCalled { get { return _endInitWasCalled; } }

        #region ISupportInitialize Members

        public void BeginInit()
        {
            if (_beginInitWasCalled)
            {
                throw new InvalidOperationException("BeginInit was called twice");
            }
            _beginInitWasCalled = true;
        }

        public void EndInit()
        {
            if (_endInitWasCalled)
            {
                throw new InvalidOperationException("EndInit was called twice");
            }
            if (!_beginInitWasCalled)
            {
                throw new InvalidOperationException("EndInit was called before BeginInit");
            }
            _endInitWasCalled = true;
        }

        #endregion

        public void CheckBeginInit(string propertyName)
        {
            if (_beginInitWasCalled != true)
            {
                throw new Exception(string.Format("Property '{0}' was set before BeginInit was called.", propertyName));
            }
        }

        public void CheckEndInit(string propertyName)
        {
            if (_endInitWasCalled != false)
            {
                throw new Exception(string.Format("Property '{0}' was set after EndInit was called.", propertyName));
            }
        }

        public bool HasEndInited
        {
            get { return _endInitWasCalled; }
        }
    }

    public class InitializableElement : InitializableBase
    {
        int _prop1;
        int _prop2;

        public int Prop1
        {
            get { return _prop1; }
            set
            {
                CheckBeginInit("Prop1");
                CheckEndInit("Prop1");
                _prop1 = value;
            }
        }

        public int Prop2
        {
            get { return _prop2; }
            set
            {
                CheckBeginInit("Prop2");
                CheckEndInit("Prop2");
                _prop2 = value;
            }
        }
    }

    public class InitializableElementHolder : InitializableBase
    {
        TopDown _topDown;
        TopDownTurnedOff _topDownTurnedOff;

        public TopDown TopDown
        {
            get { return _topDown; }
            set
            {
                CheckBeginInit("TopDown");
                CheckEndInit("TopDown");
                if (value.Prop1 != 0 || value.Prop2 != 0)
                {
                    throw new Exception("UseDuringInit element is already init'ed at property assigment time.");
                }
                _topDown = value;
            }
        }

        public TopDownTurnedOff TopDownTurnedOff
        {
            get { return _topDownTurnedOff; }
            set
            {
                CheckBeginInit("TopDownTurnedOff");
                CheckEndInit("TopDownTurnedOff");
                if (value.Prop1 == 0 || value.Prop2 == 0)
                {
                    throw new Exception("non-UseDuringInit element is not init'ed at property assigment time.");
                }
                _topDownTurnedOff = value;
            }
        }
    }

    [UsableDuringInitialization(true)]
    public class TopDown : InitializableBase
    {
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }
    }

    [UsableDuringInitialization(false)]
    public class TopDownTurnedOff : TopDown
    {
        // reuse inherited Prop1 and Prop2
    }

    public class ReferenceHolder
    {
        public ReferenceHolder()
        {
        }

        [TypeConverter(typeof(System.Windows.Markup.NameReferenceConverter))]
        public object Object { get; set; }
    }

    public class Object10i : InitializableBase
    {
        Object _object0;
        Object _object1;
        Object _object2;
        Object _object3;
        Object _object4;
        Object _object5;
        Object _object6;
        Object _object7;
        Object _object8;
        Object _object9;

        private void Chk(string propertyName)
        {
            CheckBeginInit(propertyName);
            CheckEndInit(propertyName);
        }

        public Object Object0
        {
            get { return _object0; }
            set { Chk("Object0"); _object0 = value; }
        }

        public Object Object1
        {
            get { return _object1; }
            set { Chk("Object1"); _object1 = value; }
        }

        public Object Object2
        {
            get { return _object2; }
            set { Chk("Object2"); _object2 = value; }
        }

        public Object Object3
        {
            get { return _object3; }
            set { Chk("Object3"); _object3 = value; }
        }

        public Object Object4
        {
            get { return _object4; }
            set { Chk("Object4"); _object4 = value; }
        }

        public Object Object5
        {
            get { return _object5; }
            set { Chk("Object5"); _object5 = value; }
        }

        public Object Object6
        {
            get { return _object6; }
            set { Chk("Object6"); _object6 = value; }
        }

        public Object Object7
        {
            get { return _object7; }
            set { Chk("Object7"); _object7 = value; }
        }

        public Object Object8
        {
            get { return _object8; }
            set { Chk("Object8"); _object8 = value; }
        }

        public Object Object9
        {
            get { return _object9; }
            set { Chk("Object9"); _object9 = value; }
        }
    }

    public class Element10i : InitializableBase
    {
        Element _element0;
        Element _element1;
        Element _element2;
        Element _element3;
        Element _element4;
        Element _element5;
        Element _element6;
        Element _element7;
        Element _element8;
        Element _element9;

        private void Chk(string propertyName)
        {
            CheckBeginInit(propertyName);
            CheckEndInit(propertyName);
        }

        public Element Element0
        {
            get { return _element0; }
            set { Chk("Element0"); _element0 = value; }
        }

        public Element Element1
        {
            get { return _element1; }
            set { Chk("Element1"); _element1 = value; }
        }

        public Element Element2
        {
            get { return _element2; }
            set { Chk("Element2"); _element2 = value; }
        }

        public Element Element3
        {
            get { return _element3; }
            set { Chk("Element3"); _element3 = value; }
        }

        public Element Element4
        {
            get { return _element4; }
            set { Chk("Element4"); _element4 = value; }
        }

        public Element Element5
        {
            get { return _element5; }
            set { Chk("Element5"); _element5 = value; }
        }

        public Element Element6
        {
            get { return _element6; }
            set { Chk("Element6"); _element6 = value; }
        }

        public Element Element7
        {
            get { return _element7; }
            set { Chk("Element7"); _element7 = value; }
        }

        public Element Element8
        {
            get { return _element8; }
            set { Chk("Element8"); _element8 = value; }
        }

        public Element Element9
        {
            get { return _element9; }
            set { Chk("Element9"); _element9 = value; }
        }
    }

    public class InitializableMarkupExtension : MarkupExtension, ISupportInitialize
    {
        bool _beginInitWasCalled;
        bool _endInitWasCalled;

        int _prop1;
        int _prop2;
        double _param1;

        public InitializableMarkupExtension(double param1)
        {
            _param1 = param1;
        }

        public int Prop1
        {
            get { return _prop1; }
            set
            {
                CheckBeginInit();
                CheckEndInit();
                _prop1 = value;
            }
        }

        public int Prop2
        {
            get { return _prop2; }
            set
            {
                CheckBeginInit();
                CheckEndInit();
                _prop2 = value;
            }
        }

        public bool BeginInitWasCalled { get { return _beginInitWasCalled; } }
        public bool EndInitWasCalled { get { return _endInitWasCalled; } }

        #region ISupportInitialize Members

        public void BeginInit()
        {
            _beginInitWasCalled = true;
        }

        public void EndInit()
        {
            _endInitWasCalled = true;
        }

        #endregion

        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            CheckBeginInit();
            CheckEndInit();
            return _param1 * 2.0;
        }
        protected void CheckBeginInit()
        {
            if (_beginInitWasCalled != true)
            {
                throw new Exception("Begin Init was not called before property was set");
            }
        }

        protected void CheckEndInit()
        {
            if (_endInitWasCalled != false)
            {
                throw new Exception("End Init was called before property was set");
            }
        }
    }
}

