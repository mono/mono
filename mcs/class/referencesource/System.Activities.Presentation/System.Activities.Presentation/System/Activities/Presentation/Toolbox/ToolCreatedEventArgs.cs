//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;

    // This class represents event arguments for tool created event

    [Fx.Tag.XamlVisible(false)]
    public sealed class ToolCreatedEventArgs : RoutedEventArgs
    {
        IComponent[] components;

        internal ToolCreatedEventArgs(RoutedEvent eventName, object sender, IComponent[] components)
            : base(eventName, sender)
        {
            this.components = components;
        }


        [SuppressMessage(FxCop.Category.Performance, "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Array type property does not clone the array in the getter. It references the same array instance.")]
        public IComponent[] Components
        {
            get { return this.components; }
        }
    }
}
