//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.Activities.Presentation;

    // This class is used to resolve generic type in case when a generic activity is 
    // dropped on design surface
    internal partial class ActivityTypeResolver : DialogWindow
    {
        public static readonly DependencyProperty GenericTypeMappingProperty =
            DependencyProperty.Register("GenericTypeMapping",
            typeof(ObservableCollection<TypeKeyValue>),
            typeof(ActivityTypeResolver));

        public static readonly DependencyProperty EditedTypeProperty =
            DependencyProperty.Register("EditedType",
            typeof(Type),
            typeof(ActivityTypeResolver),
            new PropertyMetadata(new PropertyChangedCallback(OnEditedTypeChanged)));

        static readonly DependencyPropertyKey IsTypeResolvedKey =
            DependencyProperty.RegisterReadOnly("IsTypeResolved",
            typeof(bool),
            typeof(ActivityTypeResolver),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsTypeResolvedProperty = IsTypeResolvedKey.DependencyProperty;

        public ActivityTypeResolver()
        {
            InitializeComponent();
            this.HelpKeyword = HelpKeywords.ActivityTypeResolver;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            SetValue(GenericTypeMappingProperty, new ObservableCollection<TypeKeyValue>());
            this.Title = SR.TypeResolverWindowTitle;
            this.typeResolver.Focus();

        }

        public Type ConcreteType
        {
            get;
            private set;
        }



        public Type EditedType
        {
            get { return (Type)GetValue(EditedTypeProperty); }
            set { SetValue(EditedTypeProperty, value); }
        }

        public ObservableCollection<TypeKeyValue> GenericTypeMapping
        {
            get { return (ObservableCollection<TypeKeyValue>)GetValue(GenericTypeMappingProperty); }
            set { SetValue(GenericTypeMappingProperty, value); }
        }

        public bool IsTypeResolved
        {
            get { return (bool)GetValue(IsTypeResolvedProperty); }
            private set { SetValue(IsTypeResolvedKey, value); }
        }

        public TypeResolvingOptions Options
        {
            get;
            set;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Catching all exceptions to avoid VS Crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Catching all exceptions to avoid VS Crash")]
        public void NotifyTypeChanged(TypeKeyValue sender)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        try
                        {
                            IsTypeResolved = (null != ResolveType() ? true : false);
                            ClearError();
                        }
                        catch (Exception err)
                        {
                            SetError(err.Message);
                            IsTypeResolved = false;
                        }
                    }));
        }

        private void SetError(string message)
        {
            if (this.GenericTypeMapping != null)
            {
                foreach (TypeKeyValue tkv in this.GenericTypeMapping)
                {
                    tkv.IsValid = false;
                    tkv.ErrorText = message;
                }
            }
        }

        private void ClearError()
        {
            if (this.GenericTypeMapping != null)
            {
                foreach (TypeKeyValue tkv in this.GenericTypeMapping)
                {
                    tkv.IsValid = true;
                    tkv.ErrorText = null;
                }
            }
        }


        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UIElementAutomationPeer(this);
        }

        static void OnEditedTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ActivityTypeResolver resolver = (ActivityTypeResolver)sender;
            resolver.OnEditTypeAssigned();
        }

        void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void OnEditTypeAssigned()
        {
            if (null != this.EditedType && this.EditedType.IsGenericTypeDefinition)
            {
                this.typeName.Text = TypeNameHelper.GetDisplayName(this.EditedType, false);

                Type[] generics = this.EditedType.GetGenericArguments();
                foreach (Type type in generics)
                {
                    Type temp = type; // reference this temp variable instead of reference type in the anonymous delegate
                    TypeKeyValue tkv = new TypeKeyValue(type, new Action<TypeKeyValue>(NotifyTypeChanged))
                                    {
                                        IsSelected = false,
                                        Filter = delegate(Type t)
                                        {
                                            if (!TypeUtilities.CanSubstituteGenericParameter(temp, t))
                                            {
                                                return false;
                                            }
                                            
                                            return this.Options == null
                                                || this.Options.Filter == null
                                                || this.Options.Filter(t);
                                        },
                                        MostRecentlyUsedTypes = this.Options != null ? this.Options.MostRecentlyUsedTypes : null,
                                    };
                    string hintText = null;
                    if (this.Options != null && this.Options.HintTextMap.TryGetValue(type.Name, out hintText))
                    {
                        tkv.HintText = hintText;
                    }

                    this.GenericTypeMapping.Add(tkv);

                    if (this.Options == null || !this.Options.BrowseTypeDirectly)
                    {
                        tkv.BrowseTypeDirectly = false;
                        //this has to happen after the tkv is added GenericTypeMapping because:
                        //when TargetType is set, TypeResolver will try to resolve the generic type with this TargetType as type argument,
                        //and when resolvig the type, TypeResolver needs to know all the mappings                        
                        if (tkv.MostRecentlyUsedTypes == null)
                        {
                            if (tkv.Filter == null || tkv.Filter(typeof(int)))
                            {
                                tkv.TargetType = typeof(int);
                            }
                        }
                        else if (tkv.MostRecentlyUsedTypes.Contains(typeof(int)))
                        {
                            tkv.TargetType = typeof(int);
                        }
                        else if (tkv.MostRecentlyUsedTypes.Count > 0)
                        {
                            tkv.TargetType = tkv.MostRecentlyUsedTypes[0];
                        }
                    }
                }
            }
        }

        void OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Type type = ResolveType();
                if (null != type)
                {
                    this.ConcreteType = type;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show(SR.TypeResolverError, SR.TypeResolverErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (ArgumentException err)
            {
                MessageBox.Show(err.Message, err.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void TypeKeyDown(object sender, KeyEventArgs e)
        {
            if (TypePresenter.IsPreviewKey(e.Key))
            {
                ListViewItem typeView = (ListViewItem)sender;
                //always focus on the type presenter so the presenter could handle keyboard events
                TypePresenter typePresenter = FindChildElement<TypePresenter>(typeView);
                if (typePresenter != null)
                {
                    typePresenter.Preview();
                }
                e.Handled = true;
            }
        }

        ChildType FindChildElement<ChildType>(DependencyObject tree) where ChildType : DependencyObject
        {
            //recursively traverse the visual tree and find the element of a given type
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(tree); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(tree, i);
                if (child != null && child is ChildType)
                {
                    return child as ChildType;
                }
                else
                {
                    ChildType childInSubtree = FindChildElement<ChildType>(child);
                    if (childInSubtree != null)
                    {
                        return childInSubtree;
                    }
                }
            }

            return null;
        }

        Type ResolveType()
        {
            Type result = null;
            bool isValid = true;
            //get number of generic parameters in edited type
            Type[] arguments = new Type[this.GenericTypeMapping.Count];

            //for each argument, get resolved type
            for (int i = 0; i < this.GenericTypeMapping.Count && isValid; ++i)
            {
                arguments[i] = this.GenericTypeMapping[i].GetConcreteType();
                isValid = isValid && (null != arguments[i]);
            }
            //if all parameters are resolved, create concrete type
            if (isValid)
            {
                result = this.EditedType.MakeGenericType(arguments);
            }
            return result;
        }
    }

}
