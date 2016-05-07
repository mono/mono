//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Activities.Presentation;
    using System.Runtime;

    // <summary>
    // Container for a shared DependencyProperty that determines the width of the property value
    // column within a CategoryList.
    // This class needs to be public because it's referenced from MarkupExtensions and, apparently,
    // they require the classes to be public.
    // </summary>
    class SharedPropertyValueColumnWidthContainer : DependencyObject, INotifyPropertyChanged 
    {

        // <summary>
        // Accessor to static, inherited, attached DP that contains a pointer to this class
        // </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty OwningSharedPropertyValueColumnWidthContainerProperty = DependencyProperty.RegisterAttached(
            "OwningSharedPropertyValueColumnWidthContainer",
            typeof(SharedPropertyValueColumnWidthContainer),
            typeof(SharedPropertyValueColumnWidthContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        private const double DefaultPercentage = 0.4f;

        private bool _changeTriggeredInternally;
        private GridLength _valueColumnWidth = new GridLength(0);
        private double _containerWidth;
        private double _valueColumnPercentage = DefaultPercentage;

        // OwningSharedPropertyValueColumnWidthContainer attached, inherited DP

        // <summary>
        // Fires an event when one of the exposed properties change
        // </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event PropertyChangedEventHandler PropertyChanged;

        // <summary>
        // Gets or sets the pixel width of the shared property value column.
        // Setting this value will recalculate the ValueColumnPercentage property.
        // We bind to this property from Xaml, hence it has to be public.
        // Sigh...
        // </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Fx.Tag.KnownXamlExternalAttribute]
        public GridLength ValueColumnWidth 
        {
            get {
                return _valueColumnWidth;
            }
            set {
                if (value.GridUnitType != GridUnitType.Pixel) 
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException());
                }

                _valueColumnWidth = value;
                OnPropertyChanged("ValueColumnWidth");

                if (this.ContainerWidth > 0) 
                {
                    if (!_changeTriggeredInternally) 
                    {
                        try 
                        {
                            // Don't modify ValueColumnWidth again
                            _changeTriggeredInternally = true;

                            this.ValueColumnPercentage = value.Value / this.ContainerWidth;
                        }
                        finally 
                        {
                            _changeTriggeredInternally = false;
                        }
                    }
                }
            }
        }

        // <summary>
        // Gets or sets the width of the container.  Setting this value
        // will recalculate the ValueColumnWidth property.
        // We bind to this property from Xaml, hence it has to be public.
        // Sigh...
        // </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public double ContainerWidth 
        {
            get {
                return _containerWidth;
            }
            internal set {
                _containerWidth = value;
                OnPropertyChanged("ContainerWidth");

                if (!_changeTriggeredInternally) 
                {
                    try 
                    {
                        // Don't modify ValueColumnPercentage, just ValueColumnWidth
                        _changeTriggeredInternally = true;

                        this.ValueColumnWidth = new GridLength(value * this.ValueColumnPercentage);
                    }
                    finally 
                    {
                        _changeTriggeredInternally = false;
                    }
                }
            }
        }

        // <summary>
        // Gets or sets the percentage width of the property value column
        // as compared to the rest of the container.  Setting this value will
        // recalculate the ValueColumnWidth property.
        // </summary>
        internal double ValueColumnPercentage 
        {
            get {
                return _valueColumnPercentage;
            }
            set {
                _valueColumnPercentage = Normalize(value, 0, 1);
                OnPropertyChanged("ValueColumnPercentage");

                if (!_changeTriggeredInternally) 
                {
                    try 
                    {
                        // Don't modify ValueColumnPercentage again
                        _changeTriggeredInternally = true;

                        this.ValueColumnWidth = new GridLength(value * this.ContainerWidth);
                    }
                    finally 
                    {
                        _changeTriggeredInternally = false;
                    }
                }
            }
        }

        // <summary>
        // Setter for OwningSharedPropertyValueColumnWidthContainerProperty
        // </summary>
        // <param name="obj"></param>
        // <param name="value"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetOwningSharedPropertyValueColumnWidthContainer(DependencyObject obj, SharedPropertyValueColumnWidthContainer value) 
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(OwningSharedPropertyValueColumnWidthContainerProperty, value);
        }

        // <summary>
        // Getter for OwningSharedPropertyValueColumnWidthContainerProperty
        // </summary>
        // <param name="obj"></param>
        // <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SharedPropertyValueColumnWidthContainer GetOwningSharedPropertyValueColumnWidthContainer(DependencyObject obj) 
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (SharedPropertyValueColumnWidthContainer)obj.GetValue(OwningSharedPropertyValueColumnWidthContainerProperty);
        }



        // Normalizes the given value into the specified range
        private static double Normalize(double value, double min, double max) 
        {
            return Math.Max(min, Math.Min(max, value));
        }

        // INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName) 
        {
            if (propertyName == null) 
            {
                throw FxTrace.Exception.ArgumentNull("propertyName");
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
