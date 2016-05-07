//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;

    abstract class FlowSwitchLink<T> : DependencyObject, IFlowSwitchLink
    {
        static DependencyProperty caseProperty = DependencyProperty.Register("Case", typeof(T), typeof(FlowSwitchLink<T>), new FrameworkPropertyMetadata(new PropertyChangedCallback(FlowSwitchLink<T>.OnCasePropertyChanged)));
        static DependencyProperty isDefaultCaseProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(FlowSwitchLink<T>), new FrameworkPropertyMetadata(new PropertyChangedCallback(FlowSwitchLink<T>.OnIsDefaultCasePropertyChanged)));

        private FlowSwitch<T> parentFlowSwitch;
        protected bool internalChange = false;
        protected ModelItem flowSwitchModelItem;
        const string DefaultConnectorViewStateKey = "Default";
        const string CaseViewStateKeyAppendString = "Connector";
        bool internalDefaultCaseChange = false;

        public FlowSwitchLink(ModelItem flowSwitchMI, T caseValue, bool isDefault)
        {
            this.flowSwitchModelItem = flowSwitchMI;
            object flowSwitch = this.flowSwitchModelItem.GetCurrentValue();
            this.parentFlowSwitch = (FlowSwitch<T>)this.flowSwitchModelItem.GetCurrentValue();
            this.internalChange = true;
            this.internalDefaultCaseChange = true;
            if (!isDefault)
            {
                this.CaseObject = caseValue;
            }
            this.IsDefaultCase = isDefault;
            this.internalDefaultCaseChange = false;
            this.internalChange = false;
        }

        [BrowsableAttribute(false)]
        public ModelItem ModelItem
        { get; set; }

        public CaseKeyValidationCallbackDelegate ValidateCaseKey
        {
            get
            {
                return (object obj, out string reason) =>
                {
                    return GenericFlowSwitchHelper.ValidateCaseKey(obj,
                        this.flowSwitchModelItem.Properties["Cases"],
                        typeof(T),
                        out reason);
                };
            }
        }

        [BrowsableAttribute(false)]
        public FlowNode ParentFlowSwitch
        {
            get
            {
                return this.parentFlowSwitch;
            }
            set
            {
                this.parentFlowSwitch = value as FlowSwitch<T>;
            }
        }

        public bool IsDefaultCase
        {
            get
            {
                return (bool)GetValue(FlowSwitchLink<T>.isDefaultCaseProperty);
            }
            set
            {
                SetValue(FlowSwitchLink<T>.isDefaultCaseProperty, value);
            }
        }

        [Browsable(false)]
        public string CaseName
        {
            get
            {
                object value = GetValue(FlowSwitchLink<T>.caseProperty);
                return GenericFlowSwitchHelper.GetString((T)value, typeof(T));
            }
        }

        [Browsable(false)]
        public object CaseObject
        {
            get
            {
                return GetValue(FlowSwitchLink<T>.caseProperty);
            }
            set
            {
                SetValue(FlowSwitchLink<T>.caseProperty, value);
            }
        }
        
        [BrowsableAttribute(false)]
        public T Case
        {
            get
            {
                return (T)GetValue(FlowSwitchLink<T>.caseProperty);
            }
            set
            {
                SetValue(FlowSwitchLink<T>.caseProperty, value);
            }
        }

        DependencyProperty CaseProperty
        {
            get
            {
                return caseProperty;
            }
        }

        DependencyProperty IsDefaultCaseProperty
        {
            get
            {
                return isDefaultCaseProperty;
            }
        }

        [Browsable(false)]
        public Type GenericType
        {
            get
            {
                return typeof(T);
            }
        }

        bool ContainsKey(object key)
        {
            return this.parentFlowSwitch.Cases.ContainsKey((T)key);
        }

        void OnIsDefaultPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isUndoRedoInProgress = this.IsUndoRedoInProgress();

            if (!this.internalDefaultCaseChange && !isUndoRedoInProgress)
            {
                bool value = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                if (value)
                {
                    if (object.Equals(this.flowSwitchModelItem.Properties["Default"].Value, null))
                    {
                        using (EditingScope es = (EditingScope)this.flowSwitchModelItem.BeginEdit(SR.FlowSwitchCaseRenameEditingScopeDesc))
                        {
                            ModelItem flowNodeMI = GenericFlowSwitchHelper.GetCaseModelItem(this.flowSwitchModelItem.Properties["Cases"], this.CaseObject);
                            GenericFlowSwitchHelper.RemoveCase(this.flowSwitchModelItem.Properties["Cases"], this.CaseObject);
                            this.flowSwitchModelItem.Properties["Default"].SetValue(flowNodeMI);
                            this.UpdateViewState(this.CaseName + CaseViewStateKeyAppendString, DefaultConnectorViewStateKey);
                            this.internalChange = true;
                            es.Complete();
                        }
                    }
                    else
                    {
                        this.internalDefaultCaseChange = true;
                        this.IsDefaultCase = oldValue;
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DefaultCaseExists));
                    }
                }
                else
                {
                    if (oldValue)
                    {
                        using (EditingScope es = (EditingScope)this.flowSwitchModelItem.BeginEdit(SR.FlowSwitchCaseRenameEditingScopeDesc))
                        {
                            ModelItem defaultCase = this.flowSwitchModelItem.Properties["Default"].Value;
                            object uniqueCase = null;
                            string errorMessage = string.Empty;
                            Type typeArgument = typeof(T);
                            if (GenericFlowSwitchHelper.CanBeGeneratedUniquely(typeArgument))
                            {
                                string caseName = GenericFlowSwitchHelper.GetCaseName(this.flowSwitchModelItem.Properties["Cases"], typeArgument, out errorMessage);
                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    this.internalDefaultCaseChange = true;
                                    this.IsDefaultCase = oldValue;
                                    throw FxTrace.Exception.AsError(new InvalidOperationException(errorMessage));
                                }
                                uniqueCase = GenericFlowSwitchHelper.GetObject(caseName, typeArgument);

                            }
                            else
                            {
                                FlowSwitchCaseEditorDialog editor = new FlowSwitchCaseEditorDialog(this.flowSwitchModelItem, ((WorkflowViewElement)this.flowSwitchModelItem.View).Context, this.flowSwitchModelItem.View, SR.ChangeCaseValue, this.flowSwitchModelItem.ItemType.GetGenericArguments()[0]);
                                editor.WindowSizeToContent = SizeToContent.WidthAndHeight;

                                if (!editor.ShowOkCancel())
                                {
                                    this.internalDefaultCaseChange = true;
                                    this.IsDefaultCase = oldValue;
                                    return;
                                }
                                uniqueCase = editor.Case;
                                if (GenericFlowSwitchHelper.ContainsCaseKey(this.flowSwitchModelItem.Properties["Cases"], uniqueCase))
                                {
                                    this.internalDefaultCaseChange = true;
                                    this.IsDefaultCase = oldValue;
                                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidFlowSwitchCaseMessage));
                                }
                            }

                            this.flowSwitchModelItem.Properties["Default"].SetValue(null);
                            this.flowSwitchModelItem.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].SetValue(FlowSwitchLabelFeature.DefaultCaseDisplayNameDefaultValue);
                            
                            this.internalChange = true;
                            if (typeof(string) != typeof(T))
                            {
                                this.ModelItem.Properties["Case"].SetValue(uniqueCase);
                                GenericFlowSwitchHelper.AddCase(this.flowSwitchModelItem.Properties["Cases"], uniqueCase, defaultCase.GetCurrentValue());
                            }
                            else
                            {
                                this.ModelItem.Properties["Case"].SetValue(uniqueCase);
                                GenericFlowSwitchHelper.AddCase(this.flowSwitchModelItem.Properties["Cases"], uniqueCase, defaultCase.GetCurrentValue());
                            }
                            this.UpdateViewState(DefaultConnectorViewStateKey, GenericFlowSwitchHelper.GetString(uniqueCase, typeof(T)) + CaseViewStateKeyAppendString);
                            es.Complete();
                            this.internalChange = false;
                        }
                        this.internalDefaultCaseChange = false;
                    }
                }
            }
            this.internalDefaultCaseChange = false;
        }

        protected bool IsUndoRedoInProgress()
        {
            bool isUndoRedoInProgress;
            WorkflowViewElement designer = (WorkflowViewElement)this.flowSwitchModelItem.View;
            if (designer == null)
            {
                isUndoRedoInProgress = false;
            }
            else
            {
                isUndoRedoInProgress = designer.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;
            }
            return isUndoRedoInProgress;
        }

        void OnCasePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isUndoRedoInProgress = this.IsUndoRedoInProgress();

            if (!this.internalChange && !isUndoRedoInProgress)
            {
                T oldValue = (T)e.OldValue;
                T newValue = (T)e.NewValue;

                if (newValue is string && newValue != null)
                {
                    newValue = (T)((object)((string)((object)newValue)).Trim());
                }

                string oldViewStateKey = string.Empty;
                if (!this.ContainsKey(newValue))
                {
                    using (EditingScope es = (EditingScope)this.flowSwitchModelItem.BeginEdit(SR.FlowSwitchCaseRenameEditingScopeDesc))
                    {
                        ModelItem flowElementMI = null;

                        flowElementMI = GenericFlowSwitchHelper.GetCaseModelItem(this.flowSwitchModelItem.Properties["Cases"], oldValue);
                        GenericFlowSwitchHelper.RemoveCase(this.flowSwitchModelItem.Properties["Cases"], oldValue);
                        oldViewStateKey = GenericFlowSwitchHelper.GetString(oldValue, typeof(T)) + CaseViewStateKeyAppendString;
                        //Add the new value
                        GenericFlowSwitchHelper.AddCase(this.flowSwitchModelItem.Properties["Cases"], newValue, flowElementMI.GetCurrentValue());
                        //Update the viewstate for the flowswitch.
                        this.UpdateViewState(oldViewStateKey, GenericFlowSwitchHelper.GetString(newValue, typeof(T)) + CaseViewStateKeyAppendString);
                        //Making sure the value for Case is always trimmed.
                        this.internalChange = true;
                        this.ModelItem.Properties["Case"].SetValue(newValue);
                        es.Complete();
                    }
                }
                else
                {
                    this.internalChange = true;
                    this.CaseObject = oldValue;
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidFlowSwitchCaseMessage));
                }
            }
            this.internalChange = false;
        }

        void UpdateViewState(string oldValue, string newValue)
        {
            EditingContext context = this.flowSwitchModelItem.GetEditingContext();
            ViewStateService viewStateService = (ViewStateService)context.Services.GetService(typeof(ViewStateService));
            if (viewStateService != null)
            {
                object viewState = viewStateService.RetrieveViewState(this.flowSwitchModelItem, oldValue);
                if (viewState != null)
                {
                    viewStateService.StoreViewStateWithUndo(this.flowSwitchModelItem, oldValue, null);
                    viewStateService.StoreViewStateWithUndo(this.flowSwitchModelItem, newValue, viewState);
                }
            }
        }

        static void OnCasePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FlowSwitchLink<T> link = (FlowSwitchLink<T>)dependencyObject;
            link.OnCasePropertyChanged(e);
        }

        static void OnIsDefaultCasePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FlowSwitchLink<T> link = (FlowSwitchLink<T>)dependencyObject;
            link.OnIsDefaultPropertyChanged(e);
        }

        public virtual MultiBinding CreateConnectorLabelTextBinding()
        {
            return new MultiBinding
            {
                Converter = new FlowSwitchLinkMultiValueConverter(),
                ConverterParameter = this.CaseProperty.PropertyType,
                Bindings = 
                {
                    new Binding { Source = this, Mode = BindingMode.OneWay, Path = new PropertyPath(this.CaseProperty) },
                    new Binding { Source = this, Mode = BindingMode.OneWay, Path = new PropertyPath(this.IsDefaultCaseProperty) },
                },
            };
        }
    }

    class FlowSwitchDefaultLink<T> : FlowSwitchLink<T>, IFlowSwitchDefaultLink
    {
        static DependencyProperty defaultCaseDisplayNameProperty = DependencyProperty.Register(FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName, typeof(string), typeof(FlowSwitchDefaultLink<T>), new FrameworkPropertyMetadata(new PropertyChangedCallback(FlowSwitchDefaultLink<T>.OnDefaultCaseDisplayNamePropertyChanged)));

        public FlowSwitchDefaultLink(ModelItem flowSwitchMI, T caseValue, bool isDefault)
            : base(flowSwitchMI, caseValue, isDefault)
        {
            this.internalChange = true;
            this.DefaultCaseDisplayName = (string)this.flowSwitchModelItem.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].Value.GetCurrentValue();
            this.internalChange = false;
        }

        public string DefaultCaseDisplayName
        {
            get
            {
                return (string)GetValue(FlowSwitchDefaultLink<T>.defaultCaseDisplayNameProperty);
            }
            set
            {
                SetValue(FlowSwitchDefaultLink<T>.defaultCaseDisplayNameProperty, value);
            }
        }

        DependencyProperty DefaultCaseDisplayNameProperty
        {
            get
            {
                return defaultCaseDisplayNameProperty;
            }
        }

        void OnDefaultCaseDisplayNamePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isUndoRedoInProgress = this.IsUndoRedoInProgress();
            if (!this.internalChange && !isUndoRedoInProgress)
            {
                string newValue = (string)e.NewValue;
                this.internalChange = true;
                using (ModelEditingScope scope = this.flowSwitchModelItem.BeginEdit(SR.FlowSwitchDefaultCaseDisplayNameEditingScopeDesc))
                {
                    this.flowSwitchModelItem.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].SetValue(newValue);
                    scope.Complete();
                }
                this.internalChange = false;
            }
        }

        static void OnDefaultCaseDisplayNamePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FlowSwitchDefaultLink<T> link = (FlowSwitchDefaultLink<T>)dependencyObject;
            link.OnDefaultCaseDisplayNamePropertyChanged(e);
        }

        public override MultiBinding CreateConnectorLabelTextBinding()
        {
            MultiBinding result = base.CreateConnectorLabelTextBinding();
            result.Bindings.Add(new Binding { Source = this, Mode = BindingMode.OneWay, Path = new PropertyPath(this.DefaultCaseDisplayNameProperty) });
            return result;
        }
    }

    class FlowSwitchCaseLink<T> : FlowSwitchLink<T>
    {
        public FlowSwitchCaseLink(ModelItem flowSwitchMI, T caseValue, bool isDefault)
            : base(flowSwitchMI, caseValue, isDefault)
        {
        }

        [BrowsableAttribute(true)]
        public new T Case
        {
            get { return base.Case; }
            set { base.Case = value; }
        }
    }
}
