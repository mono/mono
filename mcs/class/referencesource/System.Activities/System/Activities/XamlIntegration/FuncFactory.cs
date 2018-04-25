//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;

    abstract class FuncFactory
    {
        public static Func<object> CreateFunc(XamlReader reader, Type returnType)
        {
            FuncFactory factory = CreateFactory(null, reader, returnType);
            return factory.GetFunc();
        }

        public static Func<T> CreateFunc<T>(XamlReader reader) where T : class
        {
            FuncFactory<T> factory = new FuncFactory<T>(null, reader);
            return factory.GetTypedFunc();
        }

        internal IList<NamespaceDeclaration> ParentNamespaces
        {
            get;
            set;
        }

        internal XamlNodeList Nodes
        {
            get;
            set;
        }

        // Back-compat switch: we don't want to copy parent settings on Activity/DynamicActivity
        internal bool IgnoreParentSettings
        {
            get;
            set;
        }

        internal abstract Func<object> GetFunc();

        internal static FuncFactory CreateFactory(XamlReader xamlReader, IServiceProvider context)
        {
            IXamlObjectWriterFactory objectWriterFactory = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            IProvideValueTarget provideValueService = context.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            Type propertyType = null;
            //
            // IProvideValueTarget.TargetProperty can return DP, Attached Property or MemberInfo for clr property
            // In this case it should always be a regular clr property here - we are always targeting Activity.Body.
            PropertyInfo propertyInfo = provideValueService.TargetProperty as PropertyInfo;

            if (propertyInfo != null)
            {
                propertyType = propertyInfo.PropertyType;
            }

            FuncFactory funcFactory = CreateFactory(objectWriterFactory, xamlReader, propertyType.GetGenericArguments());
            return funcFactory;
        }

        // Back-compat workaround: returnType should only be a single value. But in 4.0 we didn't
        // validate this; we just passed the array in to MakeGenericType, which would throw if there
        // were multiple values. To preserve the same exception, we allow passing in an array here.
        static FuncFactory CreateFactory(IXamlObjectWriterFactory objectWriterFactory, XamlReader xamlReader, params Type[] returnType)
        {
            Type closedType = typeof(FuncFactory<>).MakeGenericType(returnType);
            return (FuncFactory)Activator.CreateInstance(closedType, objectWriterFactory, xamlReader);
        }
    }

    class FuncFactory<T> : FuncFactory where T : class
    {
        IXamlObjectWriterFactory objectWriterFactory;

        public FuncFactory(IXamlObjectWriterFactory objectWriterFactory, XamlReader reader)
        {
            this.objectWriterFactory = objectWriterFactory;
            this.Nodes = new XamlNodeList(reader.SchemaContext);
            XamlServices.Transform(reader, this.Nodes.Writer);
        }

        internal T Evaluate()
        {
            XamlObjectWriter writer = GetWriter();
            XamlServices.Transform(this.Nodes.GetReader(), writer);
            return (T)writer.Result;
        }

        internal override Func<object> GetFunc()
        {
            return (Func<T>)Evaluate;
        }

        internal Func<T> GetTypedFunc()
        {
            return Evaluate;
        }

        XamlObjectWriter GetWriter()
        {
            if (this.objectWriterFactory != null)
            {
                return this.objectWriterFactory.GetXamlObjectWriter(GetObjectWriterSettings());
            }
            else
            {
                return new XamlObjectWriter(this.Nodes.Writer.SchemaContext);
            }
        }

        XamlObjectWriterSettings GetObjectWriterSettings()
        {
            if (IgnoreParentSettings)
            {
                return new XamlObjectWriterSettings();
            }
            XamlObjectWriterSettings result = new XamlObjectWriterSettings(this.objectWriterFactory.GetParentSettings());
            // The delegate settings are already stripped by XOW. Some other settings don't make sense to copy.
            result.ExternalNameScope = null;
            result.RegisterNamesOnExternalNamescope = false;
            result.RootObjectInstance = null;
            result.SkipProvideValueOnRoot = false;
            return result;
        }
    }
}


