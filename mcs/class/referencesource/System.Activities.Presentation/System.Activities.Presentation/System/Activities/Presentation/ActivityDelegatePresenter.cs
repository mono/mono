//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Activities.Presentation;

    internal class ActivityDelegatePresenter : Control
    {
        public static readonly DependencyProperty EditingContextProperty = DependencyProperty.Register("EditingContext", typeof(EditingContext), typeof(ActivityDelegatePresenter));
        public static readonly DependencyProperty FactoryProperty = DependencyProperty.Register("Factory", typeof(IActivityDelegateFactory), typeof(ActivityDelegatePresenter));

        public static readonly DependencyProperty ActivityDelegateProperty = DependencyProperty.Register("ActivityDelegate", typeof(ModelItem), typeof(ActivityDelegatePresenter), new PropertyMetadata(new PropertyChangedCallback(ActivityDelegatePresenter.OnActivityDelegateChanged)));
        public static readonly DependencyProperty HandlerProperty = DependencyProperty.Register("Handler", typeof(ModelItem), typeof(ActivityDelegatePresenter), new PropertyMetadata(new PropertyChangedCallback(ActivityDelegatePresenter.OnHandlerChanged)));
        public static readonly DependencyProperty ArgumentsProperty = DependencyProperty.Register("Arguments", typeof(ObservableCollection<ModelItem>), typeof(ActivityDelegatePresenter));

        private bool isSetInternally;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "Calls to OverrideMetadata for a dependency property should be done in the static constructor.")]
        static ActivityDelegatePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityDelegatePresenter), new FrameworkPropertyMetadata(typeof(ActivityDelegatePresenter)));
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is internal code with no derived class")]
        public ActivityDelegatePresenter()
        {
            this.Arguments = new ObservableCollection<ModelItem>();
        }

        public ModelItem Handler
        {
            get
            {
                return (ModelItem)GetValue(HandlerProperty);
            }

            set
            {
                SetValue(HandlerProperty, value);
            }
        }

        public ModelItem ActivityDelegate
        {
            get
            {
                return (ModelItem)GetValue(ActivityDelegateProperty);
            }

            set
            {
                SetValue(ActivityDelegateProperty, value);
            }
        }

        public IActivityDelegateFactory Factory
        {
            get
            {
                return (IActivityDelegateFactory)GetValue(FactoryProperty);
            }

            set
            {
                SetValue(FactoryProperty, value);
            }
        }

        public ObservableCollection<ModelItem> Arguments
        {
            get
            {
                return (ObservableCollection<ModelItem>)GetValue(ArgumentsProperty);
            }

            set
            {
                SetValue(ArgumentsProperty, value);
            }
        }

        public EditingContext EditingContext
        {
            get
            {
                return (EditingContext)GetValue(EditingContextProperty);
            }

            set
            {
                SetValue(EditingContextProperty, value);
            }
        }

        protected virtual void ReportError(string message, string details)
        {
            ErrorReporting.ShowErrorMessage(message, details);
        }

        private static void OnActivityDelegateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ActivityDelegatePresenter)sender).OnActivityDelegateChanged();
        }

        private static void OnHandlerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ActivityDelegatePresenter)sender).OnHandlerChanged();
        }

        private void OnActivityDelegateChanged()
        {
            this.Arguments.Clear();

            if (this.ActivityDelegate != null)
            {
                ActivityDelegateMetadata metadata = ActivityDelegateUtilities.GetMetadata(this.ActivityDelegate.ItemType);

                foreach (ActivityDelegateArgumentMetadata argument in metadata)
                {
                    this.Arguments.Add(this.ActivityDelegate.Properties[argument.Name].Value);
                }

                this.isSetInternally = true;
                this.Handler = this.ActivityDelegate.Properties["Handler"].Value;
                this.isSetInternally = false;
            }
            else
            {
                this.isSetInternally = true;
                this.Handler = null;
                this.isSetInternally = false;
            }
        }

        private void OnHandlerChanged()
        {
            if (!this.isSetInternally)
            {
                if (this.Handler == null)
                {
                    this.ActivityDelegate = null;
                }
                else
                {
                    if (this.Factory != null && this.EditingContext != null)
                    {
                        try
                        {
                            ActivityDelegate instance = this.Factory.Create();
                            Fx.Assert(instance != null, "Factory should not return null");
                            ModelItem modelItem = this.EditingContext.Services.GetService<ModelTreeManager>().WrapAsModelItem(instance);
                            modelItem.Properties["Handler"].SetValue(this.Handler);
                            this.ActivityDelegate = modelItem;
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }

                            string details = ex.Message;

                            if (ex is TargetInvocationException && ex.InnerException != null)
                            {
                                details = ex.InnerException.Message;
                            }

                            this.ReportError(string.Format(CultureInfo.CurrentUICulture, SR.CannotCreateInstance, TypeNameHelper.GetDisplayName(this.Factory.DelegateType, false)), details);
                            
                            this.isSetInternally = true;
                            this.Handler = null;
                            this.isSetInternally = false;
                        }
                    }
                }
            }
        }
    }
}
