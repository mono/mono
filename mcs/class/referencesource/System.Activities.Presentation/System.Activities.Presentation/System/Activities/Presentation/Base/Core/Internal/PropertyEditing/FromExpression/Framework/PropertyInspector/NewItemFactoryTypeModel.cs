// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Globalization;

    internal class NewItemFactoryTypeModel
    {
        private Type type;
        private NewItemFactory factory;
        private Size desiredSize;
        private IMessageLogger exceptionLogger;

        public NewItemFactoryTypeModel(Type type, NewItemFactory factory)
        {
            this.type = type;
            this.factory = factory;
            this.desiredSize = new Size(0, 0);
            this.exceptionLogger = null;
        }

        public NewItemFactoryTypeModel(Type type, NewItemFactory factory, IMessageLogger exceptionLogger) : this(type, factory)
        {
            this.exceptionLogger = exceptionLogger;
        }

        public string DisplayName
        {
            get { return this.factory.GetDisplayName(this.type); }
        }

        public Type Type
        {
            get { return this.type; }
        }

        public object Image
        {
            get
            {
                object image = this.factory.GetImage(this.type, this.desiredSize);
                ImageSource imageSource = image as ImageSource;
                if (imageSource != null && imageSource is ISupportInitialize)
                {
                    try
                    {
                        double dummyHeight = imageSource.Height;
                    }
                    catch (InvalidOperationException exception)
                    {
                        this.ReportException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.NewItemFactoryIconLoadFailed, this.factory.GetType().Name, exception.Message));
                    }
                }
                return image;
            }
        }

        public Size DesiredSize
        {
            get { return this.desiredSize; }
            set { this.desiredSize = value; }
        }

        public NewItemFactory ItemFactory
        {
            get { return this.factory; }
        }

        public object CreateInstance()
        {
            return this.factory.CreateInstance(this.type);
        }

        private void ReportException(string message)
        {
            if (this.exceptionLogger != null)
            {
                this.exceptionLogger.WriteLine(message);
            }
        }

        // <summary>
        //  Seems like the ComboBoxAutomation peer, calls the object.ToString() to read out
        //  the item, if the item doesnt have its content set when queried by AutomationClient.
        //  As a result, when this NewItemFactoryTypeModel,
        //  is added to the combo-box in a SubPropertyEditor, we need to return the DisplayName
        //  property in the ToString() implementation so that that the AutomationClient
        //  reads out the correct value instead of the type of the object.
        // </summary>
        // <returns></returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
