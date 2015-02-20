//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Runtime;
    using System.Activities.Presentation;
    using System.Reflection;

    /// <summary>
    /// Interaction logic for ForEachDesigner.xaml
    /// </summary>
    partial class ForEachDesigner
    {
        public ForEachDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type[] types = new Type[]
            {
                typeof(System.Activities.Statements.ForEach<>),
                typeof(System.Activities.Statements.ParallelForEach<>)
            };

            foreach (Type type in types)
            {
                builder.AddCustomAttributes(type, new DesignerAttribute(typeof(ForEachDesigner)));
                builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            }

            builder.AddCustomAttributes(typeof(System.Activities.Statements.ForEach<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));
            builder.AddCustomAttributes(typeof(System.Activities.Statements.ParallelForEach<>), new FeatureAttribute(typeof(UpdatableGenericArgumentsFeature)));
            builder.AddCustomAttributes(typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<>), new DefaultTypeArgumentAttribute(typeof(int)));
            builder.AddCustomAttributes(typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<>), new DefaultTypeArgumentAttribute(typeof(int)));
         
        }

        void OnValuesBoxLoaded(object sender, RoutedEventArgs e)
        {
            ExpressionTextBox etb = sender as ExpressionTextBox;
            Fx.Assert(null != etb, "sender should not be null!");
            etb.ExpressionType = typeof(IEnumerable<>).MakeGenericType(this.ModelItem.ItemType.GetGenericArguments());
        }
    }
}
