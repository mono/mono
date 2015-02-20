// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Toolbox
{
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;

    /// <summary>
    /// ActivityTemplateFactoryBuilder represents an ActivityTemplateFactory class. This class is for XAML serialization purpose only and is not expected to be used by developers.
    /// </summary>
    [ContentProperty("Implementation")]
    [Designer(typeof(ActivityTypeDesigner))]
    public sealed class ActivityTemplateFactoryBuilder
    {
        /// <summary>
        /// Gets or sets the name of an ActivityTemplateFactoryBuilder
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the TargetType of an ActivityTemplateFactoryBuilder
        /// </summary>
        [DependsOn("Name")]
        [DefaultValue(null)]
        public Type TargetType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the implementation of an ActivityTemplateFactoryBuilder
        /// </summary>
        [DependsOn("TargetType")]
        [Browsable(false)]
        public object Implementation { get; set; }
    }
}
