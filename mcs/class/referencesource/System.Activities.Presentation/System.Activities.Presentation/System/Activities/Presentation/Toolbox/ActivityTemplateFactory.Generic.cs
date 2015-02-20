// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Toolbox
{
    using System.Activities.Presentation;
    using System.Activities.XamlIntegration;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Markup;
    using System.Xaml;

    /// <summary>
    /// ActivityTemplateFactory&lt;T&gt; is the XAML representation for an IActivityTemplateFactory&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">The type this factory created.</typeparam>
    [ContentProperty("Implementation")]
    public abstract class ActivityTemplateFactory<T> : IActivityTemplateFactory<T> where T : class
    {
        /// <summary>
        /// Gets or sets the a factory method that create an activity as the implementation.
        /// </summary>
        [XamlDeferLoad(typeof(FuncDeferringLoader), typeof(ActivityTemplateFactory))]
        [DefaultValue(null)]
        [Browsable(false)]
        [Ambient]
        protected virtual Func<T> Implementation
        {
            get;
            set;
        }

        /// <summary>
        /// Create an activity by calling Implementation.
        /// </summary>
        /// <param name="target">A reference the target- not used.</param>
        /// <param name="dataObject">A reference to the data object - not used.</param>
        /// <returns>The activity created by the implementation method or null if implementation is not set.</returns>
        public T Create(DependencyObject target, IDataObject dataObject)
        {
            if (this.Implementation != null)
            {
                return this.Implementation();
            }
            else
            {
                return default(T);
            }
        }
    }
}
