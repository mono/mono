using System;
using System.Xaml;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;

namespace Test.Elements
{
    [System.Windows.Markup.XamlDeferLoad(typeof(TestTemplateConverter), typeof(object))]
    public class TestTemplate
    {
        private XamlNodeList _xamlNodeList;

        public IXamlObjectWriterFactory XamlObjectWriterFactory { get; private set; }
        
        public TestTemplate(XamlReader xamlReader, IXamlObjectWriterFactory factory)
        {
            XamlObjectWriterFactory = factory;
            _xamlNodeList = new XamlNodeList(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, _xamlNodeList.Writer);
        }

        public XamlReader GetXamlReader()
        {
            return _xamlNodeList.GetReader();
        }

        // this function is just a helper.  The calling code could do this.
        public object LoadTemplate(XamlObjectWriterSettings settings)
        {
            XamlObjectWriter xamlWriter = XamlObjectWriterFactory.GetXamlObjectWriter(settings);
            XamlReader xamlReader = GetXamlReader();
            XamlServices.Transform(xamlReader, xamlWriter);
            return xamlWriter.Result;
        }
    }

    public class TestTemplateConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }

            var factory = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            if (factory == null)
            {
                throw new InvalidOperationException("Missing Service Provider Service 'IXamlObjectWriterFactory'");
            }

            var testTemplate = new TestTemplate(xamlReader, factory);
            return testTemplate;
        }

        public override XamlReader Save(object value, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    [System.Windows.Markup.XamlDeferLoad(typeof(NameTemplateConverter), typeof(object))]
    public class TemplateWithNameResolver
    {
        public IXamlNameResolver Resolver
        {
            get;
            private set;
        }

        public void Handler(object sender, EventArgs e)
        {
            Resolver = (IXamlNameResolver)sender;            
        }
    }

    public class NameTemplateConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            var resolver = context.GetService(typeof(IXamlNameResolver)) as IXamlNameResolver;

            if (resolver == null)
            {
                throw new InvalidOperationException("Missing Name Resolving Service 'IXamlNameResolver'");
            }

            var testTemplate = new TemplateWithNameResolver();

            resolver.OnNameScopeInitializationComplete += new EventHandler(testTemplate.Handler);
            
            return testTemplate;
        }

        public override XamlReader Save(object value, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}	
