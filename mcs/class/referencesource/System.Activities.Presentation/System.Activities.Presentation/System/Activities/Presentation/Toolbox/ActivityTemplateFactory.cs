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
    /// ActivityTemplateFactory is the XAML representation for an IActivityTemplateFactory. This class is for XAML serialization purpose only and is not expected to be used by developers.
    /// </summary>
    [ContentProperty("Implementation")]
    public abstract class ActivityTemplateFactory : IActivityTemplateFactory
    {
        /// <summary>
        /// Gets or sets the a factory method that create an activity as the implementation.
        /// </summary>
        [XamlDeferLoad(typeof(FuncDeferringLoader), typeof(ActivityTemplateFactory))]
        [DefaultValue(null)]
        [Browsable(false)]
        [Ambient]
        protected virtual Func<Activity> Implementation
        {
            get;
            set;
        }

        /// <summary>
        /// Create an activity by calling Implementation.
        /// </summary>
        /// <param name="target">A reference to the user interface - not used.</param>
        /// <returns>The activity created by the implementation method or null if implementation is not set.</returns>
        public Activity Create(DependencyObject target)
        {
            if (this.Implementation != null)
            {
                return this.Implementation();
            }
            else
            {
                return null;
            }
        }
    }
}
