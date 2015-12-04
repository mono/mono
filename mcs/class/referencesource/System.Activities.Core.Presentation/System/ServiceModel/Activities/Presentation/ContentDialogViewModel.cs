//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    class ContentDialogViewModel<TMessage, TParameter> : INotifyPropertyChanged
        where TMessage : new()
        where TParameter : new()
    {
        EditingMode editingMode = EditingMode.Message;
        ModelItem messageExpression;
        Type declaredMessageType;

        public ContentDialogViewModel(ModelItem modelItem)
        {
            this.ModelItem = modelItem;
            InitializeMessageAndParameterData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEditingEnabled
        {
            get
            {
                return !this.Context.Items.GetValue<ReadOnlyState>().IsReadOnly;
            }
        }

        public bool IsMessageChecked
        {
            get
            {
                return this.editingMode == EditingMode.Message;
            }
            set
            {
                if (value != this.IsMessageChecked)
                {
                    this.editingMode = value ? EditingMode.Message : EditingMode.Parameter;
                    OnModeChanged();
                }
            }
        }

        public bool IsParameterChecked
        {
            get
            {
                return this.editingMode == EditingMode.Parameter;
            }
            set
            {
                if (value != this.IsParameterChecked)
                {
                    this.editingMode = value ? EditingMode.Parameter : EditingMode.Message;
                    OnModeChanged();
                }
            }
        }

        public ModelItem ModelItem
        {
            get;
            private set;
        }

        public EditingContext Context
        {
            get
            {
                return this.ModelItem.GetEditingContext();
            }
        }

        public ModelItem MessageExpression
        {
            get
            {
                return this.messageExpression;
            }
            set
            {
                this.messageExpression = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MessageExpression"));
                }
            }
        }

        public Type DeclaredMessageType
        {
            get
            {
                return this.declaredMessageType;
            }
            set
            {
                this.declaredMessageType = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("DeclaredMessageType"));
                }
            }
        }

        public bool IsDictionary
        {
            get;
            private set;
        }

        public Type UnderlyingArgumentType
        {
            get;
            private set;
        }

        public ObservableCollection<DynamicArgumentWrapperObject> DynamicArguments
        {
            get;
            set;
        }

        internal bool OnOk()
        {
            ModelProperty contentProperty = this.ModelItem.Properties["Content"];
            if (this.editingMode == EditingMode.Parameter)
            {
                contentProperty.SetValue(new TParameter());
                DynamicArgumentDesigner.WrapperCollectionToModelItem(this.DynamicArguments,
                                        contentProperty.Value.Properties["Parameters"].Value,
                                        this.IsDictionary, this.UnderlyingArgumentType);
            }
            else
            {
                if (this.DeclaredMessageType == null && this.MessageExpression == null)
                {
                    contentProperty.SetValue(null);
                }
                else
                {
                    contentProperty.SetValue(new TMessage());
                    contentProperty.Value.Properties["Message"].SetValue(this.MessageExpression);
                    contentProperty.Value.Properties["DeclaredMessageType"].SetValue(this.DeclaredMessageType);
                }
            }

            return true;
        }

        void InitializeMessageAndParameterData()
        {
            ModelItem parameterModelItem;
            ModelTreeManager modelTreeManager = (this.ModelItem as IModelTreeItem).ModelTreeManager;

            ModelItem contentModelItem = this.ModelItem.Properties["Content"].Value;
            if (contentModelItem == null)
            {
                this.messageExpression = modelTreeManager.WrapAsModelItem(new TMessage()).Properties["Message"].Value;
                this.declaredMessageType = null;
                parameterModelItem = modelTreeManager.WrapAsModelItem(new TParameter()).Properties["Parameters"].Value;
            }
            else
            {
                if (contentModelItem.ItemType == typeof(TMessage))
                {
                    this.editingMode = EditingMode.Message;
                    this.messageExpression = contentModelItem.Properties["Message"].Value;
                    this.declaredMessageType = (Type)contentModelItem.Properties["DeclaredMessageType"].ComputedValue;
                    parameterModelItem = modelTreeManager.WrapAsModelItem(new TParameter()).Properties["Parameters"].Value;
                }
                else
                {
                    this.editingMode = EditingMode.Parameter;
                    this.messageExpression = modelTreeManager.WrapAsModelItem(new TMessage()).Properties["Message"].Value;
                    this.declaredMessageType = null;
                    parameterModelItem = contentModelItem.Properties["Parameters"].Value;
                }
            }

            bool isDictionary;
            Type underlyingArgumentType;
            this.DynamicArguments = DynamicArgumentDesigner.ModelItemToWrapperCollection(
                                                parameterModelItem,
                                                out isDictionary,
                                                out underlyingArgumentType);

            this.IsDictionary = isDictionary;
            this.UnderlyingArgumentType = underlyingArgumentType;
        }

        void OnModeChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsMessageChecked"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsParameterChecked"));
            }
        }

        enum EditingMode
        {
            Message,
            Parameter
        }
    }
}
