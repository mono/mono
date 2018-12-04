// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xaml;

namespace System.Windows.Markup
{
    [ContentProperty("Name")]
    public class Reference : MarkupExtension
    {
        public Reference()
        {
        }

        public Reference(string name)
        {
            Name = name;
        }

        [ConstructorArgument("name")]
        public string Name { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            IXamlNameResolver nameResolver = serviceProvider.GetService(typeof(IXamlNameResolver)) as IXamlNameResolver;
            if (nameResolver == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.MissingNameResolver));
            }
            if (String.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException(SR.Get(SRID.MustHaveName));
            }
            object obj = nameResolver.Resolve(Name);
            if (obj == null)
            {
                string[] names = new string[] { Name };
                obj = nameResolver.GetFixupToken(names, true);
            }
            return obj;
        }
    }
}
