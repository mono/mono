// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;

namespace System.Xaml.Replacements
{

    // This is a helper class used by the DateTimeConverter2 to call the DateTimeValueSerializer.
    // It provides no functionality.

    internal class DateTimeValueSerializerContext : IValueSerializerContext
    {
        public ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor)
        {
            return null;
        }

        public ValueSerializer GetValueSerializerFor(Type type)
        {
            return null;
        }


        public IContainer Container
        {
            get { return null; }
        }

        public object Instance
        {
            get { return null; }
        }

        public void OnComponentChanged()
        {
        }

        public bool OnComponentChanging()
        {
            return false;
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get { return null; }
        }

        public object GetService(Type serviceType)
        {
            return null;
        }

    }


}

