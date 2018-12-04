using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xaml;
using MARKUP=System.Windows.Markup;
using System.Xaml.Schema;

namespace Cdf.Test.Elements
{
    public class FakeBinding : MARKUP.MarkupExtension
    {
        public string Path
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof(MARKUP.IProvideValueTarget)) as MARKUP.IProvideValueTarget;
            var fe = (FakeFrameworkElement)pvt.TargetObject;

            var b = new RuntimeBinding()
                {
                    Target = fe,
                    TargetProperty = ((PropertyInfo)pvt.TargetProperty).Name,
                    SourceProperty = Path
                };
            b.RunOne();
            return b.Value;
        }

        // It seems likey that this will fail given the lack of observations support
        public class RuntimeBinding
        {
            object _value;

            public FakeFrameworkElement Target { get; set; }

            public string TargetProperty { get; set; }

            public string SourceProperty { get; set; }

            public object Value
            {
                get { return _value; }
            }

            public void RunOne()
            {
                var context = Target.DataContext;
                if (context != null)
                {
                    Type srcType = context.GetType();
                    PropertyInfo srcProperty = srcType.GetProperty(SourceProperty);
                    _value = srcProperty.GetValue(context, null);

                    PropertyInfo targetProperty = Target.GetType().GetProperty(TargetProperty);
                    targetProperty.SetValue(Target, _value, null);
                }
            }

            void Updated(object state)
            {
                RunOne();
            }
        }
    }


    [MARKUP.ContentProperty("Cases")]
    public class FakeSwitch<TValue, TResult>
    {
        TValue _val;

        public FakeSwitch()
        {
            Cases = new List<FakeCase<TValue, TResult>>();
            Body = new CompositeFactory<TValue, TResult>(this).Evaluate;
        }

        public TValue Value
        {
            get { return _val; }
            set { _val = value; }
        }

        public TResult Content
        {
            get { return Body(); }
        }

        public List<FakeCase<TValue, TResult>> Cases { get; private set; }
        
        [MARKUP.XamlDeferLoad(typeof(FuncFactoryTypeConverter), typeof(Func <> ))]
        public Func<TResult> Body { get; private set; }

        class CompositeFactory<TValue1, TResult1>
        {
            FakeSwitch<TValue1, TResult1> owner;

            internal CompositeFactory(FakeSwitch<TValue1, TResult1> owner)
            {
                this.owner = owner;
            }

            public TResult1 Evaluate()
            {
                foreach (var c in owner.Cases)
                {
                    if (c.Condition.Equals(owner.Value))
                    {
                        return c.Body();
                    }
                }
                return default(TResult1);
            }
        }
    }

    [MARKUP.ContentProperty("Body")]
    public class FakeCase<TValue, TResult>
    {
        public TValue Condition { get; set; }

        [MARKUP.XamlDeferLoad(typeof(FuncFactoryTypeConverter), typeof(object))]
        public Func<TResult> Body { get; set; }
    }


    [MARKUP.RuntimeNameProperty("Name")]
    public class FakeVariable
    {
        object value;

        public string Name { get; set; }

        public virtual void SetValueUntyped(FakeEnvironment environment, object value)
        {
            this.value = value;
        }

        public virtual object GetValueUntyped(FakeEnvironment environment)
        {
            return value;
        }
    }
    public class FakeVariable<T> : FakeVariable
    {
        public FakeVariable()
        {
        }

        public FakeVariable(T value)
        {
            SetValue(null, value);
        }

        public void SetValue(FakeEnvironment environment, T value)
        {
            SetValueUntyped(environment, value);
        }
        
        public T GetValue(FakeEnvironment environment)
        {
            return (T)GetValueUntyped(environment);
        }
    }

    public class FakeLookupVariable<T> : FakeVariable<T>
    {
        public FakeLookupVariable()
        {
        }
        
        public FakeLookupVariable(string name)
        {
            Name = name;
        }

        public override void SetValueUntyped(FakeEnvironment environment, object value)
        {
            environment.Variables[Name].SetValueUntyped(environment, value);
        }
        
        public override object GetValueUntyped(FakeEnvironment environment)
        {
            return environment.Variables[Name].GetValueUntyped(environment);
        }
    }

    public class FakeEnvironment
    {
        Dictionary<string, FakeVariable> variables = new Dictionary<string, FakeVariable>();

        public IDictionary<string, FakeVariable> Variables
        {
            get { return variables; }
        }
    }

    public class FakeTemplateEnvironment<T>
    {
        public FakeEnvironment Environment { get; set; }

        public FakeVariable<T> Target { get; set; }
    }

    abstract public class FakeActivity
    {
        abstract public void Invoke(FakeEnvironment environment);
    }

    public class FakeForEach<T> : FakeActivity
    {
        public FakeVariable<T> LoopVariable { get; set; }

        public IEnumerable<T> ItemsSource { get; set; }

        [MARKUP.XamlDeferLoad(typeof(FuncFactoryTypeConverter), typeof(FakeVariable<>))]
        public Func<FakeActivity> Body { get; set; }

        override public void Invoke(FakeEnvironment environment)
        {
            foreach (var item in ItemsSource)
            {
                LoopVariable.SetValue(null, item);
                var activity = Body();

                // Should create a child environment to scope the variable
                //
                environment.Variables[LoopVariable.Name] = LoopVariable;
                activity.Invoke(environment);
            }
        }
    }
    // <summary>
    // <FakeForEach x:TypeArguments='s:Int32'>
    //    <FakeForEach.LoopVariable><FakeVariable x:TypeArguments='s:Int32' x:Name='foo' /></>
    //    <FakeForEach.ItemsSource>...</>
    //    <FakeForEach.Body>
    //       <ConsoleWrite Message='["Hello number " + foo]' />
    //    </>
    // </FakeForEach>
    //
    // var l = new FakeVariable<int>();
    // new FakeForEach<int>()
    // {
    //     LoopVariable=l,
    //     ItemsSource={...},
    //     Body=
    //         FakeActivityTemplate.Create(
    //             (o) => new ConsoleWrite()
    //             {
    //                 Message = new ExpressionArgument<string>((env)=>"Hello number " + env.GetValue(l))
    //             }
    //         };
    // </summary>
    public class FakeAppendIntToString : FakeActivity
    {
        public FakeVariable<string> Target { get; set; }

        public FakeVariable<int> Content { get; set; }

        override public void Invoke(FakeEnvironment environment)
        {
            Target.SetValue(environment, Target.GetValue(environment) + Content.GetValue(environment));
        }
    }
    
    public class FakeSetString : FakeActivity
    {
        public FakeVariable<string> Target { get; set; }

        public FakeVariable<string> Content { get; set; }

        override public void Invoke(FakeEnvironment environment)
        {
            Target.SetValue(environment, Content.GetValue(environment));
        }
    }
    
    public class VariableExtension : MARKUP.MarkupExtension
    {
        public VariableExtension()
        {
        }

        public VariableExtension(string variable)
        {
            Variable = variable;
        }

        public string Variable { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof(MARKUP.IProvideValueTarget)) as MARKUP.IProvideValueTarget;
            var prop = pvt.TargetProperty as PropertyInfo;
            if (prop != null)
            {
                var type = typeof(FakeLookupVariable < > ).MakeGenericType(prop.PropertyType.GetGenericArguments()[0]);
                var variable = Activator.CreateInstance(type) as FakeVariable;
                variable.Name = Variable;
                return variable;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class FakeFrameworkElementFactoryTypeConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            var objectWriterFactory = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            return new FakeFrameworkElementFactory(xamlReader, objectWriterFactory);
        }

        public override XamlReader Save(object templateContent, IServiceProvider serviceProvider)
        {
            return ((FakeFrameworkElementFactory)templateContent).impl.CreateReader();
        }
    }

    [MARKUP.XamlDeferLoad(typeof(FakeFrameworkElementFactoryTypeConverter), typeof(object))]
    public class FakeFrameworkElementFactory
    {
        internal XamlReaderMarkupFactory impl;

        internal FakeFrameworkElementFactory(System.Xaml.XamlReader reader, IXamlObjectWriterFactory xamlObjectWriterFactory)
        {
            impl = new XamlReaderMarkupFactory(reader, xamlObjectWriterFactory);
        }

        public FakeFrameworkElement Evaluate()
        {
            return impl.Evaluate();
        }

        internal class XamlReaderMarkupFactory
        {
            bool disposed = false;
            private IXamlObjectWriterFactory _xamlObjectWriterFactory;
            private XamlNodeList _xamlNodeList;

            public XamlReaderMarkupFactory(System.Xaml.XamlReader reader, IXamlObjectWriterFactory xamlObjectWriterFactory)
            {
                _xamlObjectWriterFactory = xamlObjectWriterFactory;
                _xamlNodeList = new XamlNodeList(reader.SchemaContext);
                XamlServices.Transform(reader, _xamlNodeList.Writer);
            }

            public System.Xaml.XamlReader CreateReader()
            {
                FailIfDisposed();
                return _xamlNodeList.GetReader();
            }

            public FakeFrameworkElement Evaluate()
            {
                FailIfDisposed();
                XamlObjectWriter writer = _xamlObjectWriterFactory.GetXamlObjectWriter(null);
                System.Xaml.XamlReader reader = CreateReader();
                XamlServices.Transform(reader, writer);
                return (FakeFrameworkElement)writer.Result;
            }

            void FailIfDisposed()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(null);
                }
            }
        }
    }
    public abstract class FakeFrameworkTemplate
    {
        internal abstract object Apply(object context);
    }
    [MARKUP.ContentProperty("VisualTree")]
    public class FakeControlTemplate : FakeFrameworkTemplate
    {
        public Type TargetType { get; set; }

        public FakeFrameworkElementFactory VisualTree { get; set; }

        public FakeFrameworkElement Apply(FakeControl control)
        {
            return (FakeFrameworkElement)Apply(control);
        }
        
        internal override object Apply(object context)
        {
            return VisualTree.Evaluate();
        }
    }

    public class FakeUIElement
    {
    }
    
    public class FakeFrameworkElement : FakeUIElement
    {
        object dataContext;

        [DefaultValue(null)]
        public object DataContext
        {
            get { return dataContext; }
            set { dataContext = value; }
        }
    }
    
    public class FakeControl : FakeFrameworkElement
    {
        FakeUIElement visuals;

        public FakeControlTemplate Template { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FakeUIElement Visuals
        {
            get
            {
                if (visuals == null)
                {
                    visuals = Template.VisualTree.Evaluate();
                }
                return visuals;
            }
        }
    }
    
    [MARKUP.ContentProperty("Content")]
    public class FakeContentControl : FakeControl
    {
        [MARKUP.Ambient]
        public object Content { get; set; }
    }

    public class FakeButton : FakeContentControl
    {
    }
    
    [MARKUP.ContentProperty("Child")]
    public class FakeBorder : FakeFrameworkElement
    {
        public FakeUIElement Child { get; set; }
    }
    
    public class FakeContentPresenter : FakeFrameworkElement
    {
        public object Content { get; set; }
    }

    public class TemplateBindingExtension : MARKUP.MarkupExtension
    {
        public TemplateBindingExtension()
        {
        }

        public TemplateBindingExtension(string property)
        {
            Property = property;
        }
        
        public string Property { get; set; }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var schemaContextProvider = (IXamlSchemaContextProvider)serviceProvider.GetService(typeof(IXamlSchemaContextProvider));
            var iAmbient = (IAmbientProvider)serviceProvider.GetService(typeof(IAmbientProvider));

            Type ambientProviderType = null;
            if (this.Property == "Name")
            {
                ambientProviderType = typeof(TypeWithTemplateProperty);
            }
            else if (this.Property == "Content")
            {
                ambientProviderType = typeof(FakeContentControl);
            }
            XamlType ambientProvider = schemaContextProvider.SchemaContext.GetXamlType(ambientProviderType);
            XamlMember property = ambientProvider.GetMember(this.Property);
            IEnumerable<AmbientPropertyValue> ambientEnumerable = iAmbient.GetAllAmbientValues(null, property);
            foreach (AmbientPropertyValue apVal in ambientEnumerable)
            {
                if (apVal.Value != null)
                {
                    return apVal.Value;
                }
            }
            return null;
        }
    }

    public class TestFactory<TResult> : XamlFactory<TResult>
    {
        public TestFactory(IXamlObjectWriterFactory context, System.Xaml.XamlReader reader)
            : base(context, reader)
        {
        }
    }

    public class TypeWithTemplateInterfaceProperty
    {
        public string Name { get; set; }

        public TestFactory<object> Template { get; set; }
    }

    public class TypeWithTemplateProperty
    {
        [MARKUP.Ambient]
        public string Name { get; set; }

        public TestFactory<object> Template { get; set; }
    }

    public class TypeWithDictionaryTemplateProperty
    {
        public string Name { get; set; }

        public TestFactory<Dictionary<string, object>> Template { get; set; }
    }

    public class TypeWithListTemplateProperty
    {
        public string Name { get; set; }

        public TestFactory<List<object>> Template { get; set; }
    }

    public class TypeWithTemplateListProperty
    {
        public TypeWithTemplateListProperty()
        {
            Templates = new List<TestFactory<object>>();
        }

        public string Name { get; set; }
        
        public List<TestFactory<object>> Templates { get; private set; }
    }

    public class TypeWithTemplateDictionaryProperty
    {
        public TypeWithTemplateDictionaryProperty()
        {
            Templates = new Dictionary<string, TestFactory<object>>();
        }

        public string Name { get; set; }

        public Dictionary<string, TestFactory<object>> Templates { get; private set; }
    }

    public class FuncMarkupFactory<TReturn>
    {
        Func<TReturn> func;

        public FuncMarkupFactory(Func<TReturn> func)
        {
            this.func = func;
        }

        public TReturn Evaluate()
        {
            return func();
        }
    }
    
    public static class ExprMarkupFactory
    {
        public static ExprMarkupFactory<TReturn> Create<TReturn>(Expression<Func<TReturn>> expression)
        {
            return new ExprMarkupFactory<TReturn>(expression);
        }
    }
    
    public class ExprMarkupFactory<TReturn>
    {
        Expression<Func<TReturn>> expression;
        Func<TReturn> func;

        public ExprMarkupFactory(Expression<Func<TReturn>> expression)
        {
            this.expression = expression;
            func = expression.Compile();
        }

        public TReturn Evaluate()
        {
            return func();
        }
    }

    public class XamlFactory
    {
        internal XamlNodeList _nodes;
    }

    internal class XamlFactoryTypeConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }

            var objectWriterFactory = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            var ipvt = context.GetService(typeof(MARKUP.IProvideValueTarget)) as MARKUP.IProvideValueTarget;
            PropertyInfo pi = ipvt.TargetProperty as PropertyInfo;
            Type type = pi.PropertyType;
            object o = Activator.CreateInstance(type, objectWriterFactory, xamlReader);
            return o;
        }

        public override XamlReader Save(object value, IServiceProvider context)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XamlFactory factory = (XamlFactory)value;
            System.Xaml.XamlReader reader = factory._nodes.GetReader();
            return reader;
        }
    }

    [MARKUP.XamlDeferLoad(typeof(XamlFactoryTypeConverter), typeof(object))]
    public class XamlFactory<TReturn> : XamlFactory
    {
        IXamlObjectWriterFactory _loaderFactory;
        Func<TReturn> _impl;

        public XamlFactory(IXamlObjectWriterFactory context, System.Xaml.XamlReader reader)
        {
            _loaderFactory = context;
            _nodes = new XamlNodeList(reader.SchemaContext);
            XamlServices.Transform(reader, _nodes.Writer);
        }

        public XamlFactory(Func<TReturn> impl)
        {
            _impl = impl;
        }

        public TReturn Evaluate()
        {
            if (_loaderFactory != null)
            {
                XamlObjectWriter writer = _loaderFactory.GetXamlObjectWriter(null);
                System.Xaml.XamlReader reader = _nodes.GetReader();
                XamlServices.Transform(reader, writer);
                object root = writer.Result; 
                return (TReturn)root;
            }
            else
            {
                return _impl();
            }
        }
    }

    public class FuncFactory
    {
        internal XamlNodeList _nodes;
    }

    public class FuncFactory<TReturn> : FuncFactory
    {
        IXamlObjectWriterFactory _objectWriterFactory;

        public FuncFactory(System.Xaml.XamlReader reader, IXamlObjectWriterFactory objectWriterFactory)
        {
            _objectWriterFactory = objectWriterFactory;
            _nodes = new XamlNodeList(reader.SchemaContext);
            XamlServices.Transform(reader, _nodes.Writer);
        }

        public TReturn Evaluate()
        {
            XamlObjectWriter writer = _objectWriterFactory.GetXamlObjectWriter(null);
            System.Xaml.XamlReader reader = _nodes.GetReader();
            XamlServices.Transform(reader, writer);
            object root = writer.Result;
            return (TReturn)root;
        }
    }

    public class FuncFactoryTypeConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            var objectWriterFactory = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            var ipvt = context.GetService(typeof(MARKUP.IProvideValueTarget)) as MARKUP.IProvideValueTarget;
            PropertyInfo pi = ipvt.TargetProperty as PropertyInfo;
            Type propertyType = pi.PropertyType;
            var genericArguments = propertyType.GetGenericArguments();
            Type funcFactoryType = typeof(FuncFactory<>);
            var instance = Activator.CreateInstance(funcFactoryType.MakeGenericType(genericArguments),
                                                        xamlReader, objectWriterFactory);

            Delegate retDelegate = Delegate.CreateDelegate(propertyType, instance, instance.GetType().GetMethod("Evaluate"));
            return retDelegate;
        }

        public override XamlReader Save(object value, IServiceProvider context)
        {
            FuncFactory factory = (FuncFactory)(((Delegate)value).Target);
            System.Xaml.XamlReader reader = factory._nodes.GetReader();
            return reader;
        }
    }
}
