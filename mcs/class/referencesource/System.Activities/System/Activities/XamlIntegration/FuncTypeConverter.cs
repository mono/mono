//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Windows.Markup;
    using System.Xaml;

    public class FuncDeferringLoader : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            FuncFactory factory = FuncFactory.CreateFactory(xamlReader, context);
            factory.IgnoreParentSettings = true;
            return factory.GetFunc();
        }

        public override XamlReader Save(object value, IServiceProvider serviceProvider)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR.SavingActivityToXamlNotSupported));
        }
    }
}


