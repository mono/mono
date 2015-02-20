//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// DesignObjectWrapper. this class is used to enable more detailed control over edting model objects. especially, if underlying object 
    /// requires some more complex logic when setting property values - i.e. value of a real property is splitted in the ui to different design properties
    /// (like in ArgumentDesigner - actual argument's property depends on two factors: direction (in, out, ...) and actual CLR type.
    /// the DesignObjectWrapper contains that logic and is able to interact with underlying real object, but from ui perspective offeres different set of properties.
    /// 
    /// the model can be presented as follows:
    /// 
    ///      UI                |           interaction logic             |           actual model
    /// -----------------------+-----------------------------------------+-------------------------------
    ///                         
    ///    FakeModelItem  <---------------- DesignObjectWrapper ---------------------> ModelItem
    ///                                             ^
    ///                                             |
    ///                            DesignObjectWrapper implementation
    ///                            
    ///  Where:
    ///  - FakeModelItem - is a class which exposes any properties which are required to edit actual model. those properties do not have
    ///                    to exist on the real object, you are responsible to provide getters (required),  setters (optional) and validation (optional)
    ///                    code for them. In UI, you can access that property using Content property.
    ///                    
    /// - DesignObjectWrapper - implementing that class you have to provide a set of property descriptors (get, set, validate, name, type) methods for each of your property
    ///                    It is required that you provide static implementation for following method:
    ///                         PropertyDescriptorData[] InitializeTypeProperties()
    ///                    After you are done with editing of this object, call Dispose, so it unhooks from property change notificatons
    ///                         
    /// - ModelItem      - actual model you bind to. DesignObjectWrapper implmentation registers for PropertyChanged notifications from that object, and will notify you via
    ///                 OnReflectedObjectPropertyChanged. This object can be accessed using ReflectedObject property
    /// 
    /// </summary>
    abstract class DesignObjectWrapper : ICustomTypeDescriptor, INotifyPropertyChanged, IDisposable
    {
        protected static readonly string HasErrorsProperty = "HasErrors";
        protected static readonly string ContentProperty = "Content";
        protected static readonly string ValidationErrorSuffix = "ValidationError";
        protected static readonly string AutomationIdProperty = "AutomationId";
        protected internal static readonly string TimestampProperty = "Timestamp";
        readonly static string[] DefaultProperties = new string[] { HasErrorsProperty, AutomationIdProperty, TimestampProperty };
        static IDictionary<Type, PropertyDescriptorCollection> TypePropertyCollection = new Dictionary<Type, PropertyDescriptorCollection>();

        IDictionary<string, string> validationErrors = null;
        IDictionary<string, PropertyValueEditor> customValueEditors = null;
        FakeModelItemImpl content;
        bool isDisposed = false;
        DateTime timestamp;
        HashSet<string> changingProperties;

        protected DesignObjectWrapper()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR.InvalidConstructorCall));
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors,
            Justification = "This class is internal with limited usage inside framework assemblies only. The code written should be safe enough to allow such usage.")]
        protected DesignObjectWrapper(ModelItem reflectedObject)
        {
            this.changingProperties = new HashSet<string>();
            this.Initialize(reflectedObject);
        }

        internal void Initialize(ModelItem reflectedObject)
        {
            this.isDisposed = false;
            this.changingProperties.Clear();
            this.ReflectedObject = reflectedObject;
            this.Context = ((IModelTreeItem)reflectedObject).ModelTreeManager.Context;
            this.ModelTreeManager = ((IModelTreeItem)reflectedObject).ModelTreeManager;
            this.ReflectedObject.PropertyChanged += OnReflectedObjectPropertyChanged;
            this.RaisePropertyChangedEvent("ReflectedObject");
            //update timestamp if we do reinitialize wrapper
            this.UpdateTimestamp();
            this.Content.PropertyChanged += this.OnFakeModelPropertyChanged;
        }

        void OnFakeModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!this.changingProperties.Contains(e.PropertyName))
            {
                this.changingProperties.Add(e.PropertyName);
                this.RaisePropertyChangedEvent(e.PropertyName);
                this.changingProperties.Remove(e.PropertyName);
            }
        }

        public ModelItem ReflectedObject
        {
            get;
            private set;
        }

        public EditingContext Context
        {
            get;
            private set;
        }

        protected ModelTreeManager ModelTreeManager
        {
            get;
            private set;
        }

        public ModelItem Content
        {
            get
            {
                if (null == this.content)
                {
                    ModelTreeManager manager = this.Context.Services.GetService<ModelTreeManager>();
                    this.content = new FakeModelItemImpl(manager, this.GetType(), this, null);
                }
                return this.content;
            }
            private set
            {
                this.content = (FakeModelItemImpl)value;
            }
        }

        IDictionary<string, string> ValidationErrors
        {
            get
            {
                if (null == this.validationErrors)
                {
                    this.validationErrors = new Dictionary<string, string>();
                }
                return this.validationErrors;
            }
        }

        protected IDictionary<string, PropertyValueEditor> CustomValueEditors
        {
            get
            {
                if (null == this.customValueEditors)
                {
                    this.customValueEditors = new Dictionary<string, PropertyValueEditor>();
                }
                return this.customValueEditors;
            }
        }

        public bool HasErrors
        {
            get
            {
                return null != this.validationErrors && this.validationErrors.Count != 0;
            }
        }

        protected abstract string AutomationId { get; }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return new AttributeCollection(this.GetType().GetCustomAttributes(false).OfType<Attribute>().ToArray());
        }

        public string GetClassName()
        {
            return this.GetType().FullName;
        }

        public string GetComponentName()
        {
            return this.GetType().FullName;
        }

        public TypeConverter GetConverter()
        {
            object[] attributes = this.GetType().GetCustomAttributes(typeof(TypeConverterAttribute), false);
            if (attributes.Length != 0)
            {
                TypeConverterAttribute attribute = (TypeConverterAttribute)attributes[0];
                return (TypeConverter)Activator.CreateInstance(Type.GetType(attribute.ConverterTypeName));
            }
            return null;
        }

        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public string GetValidationErrors(IList<string> invalidProperties)
        {
            var result = string.Empty;
            if (this.HasErrors)
            {
                var content = new StringBuilder();
                bool newRowRequired = false;
                foreach (var entry in this.validationErrors)
                {
                    if (newRowRequired)
                    {
                        content.AppendLine();
                    }
                    content.Append(entry.Key);
                    content.AppendLine(":");
                    content.Append(entry.Value);
                    newRowRequired = true;
                    if (null != invalidProperties)
                    {
                        invalidProperties.Add(entry.Key);
                    }
                }
                result = content.ToString();
            }
            return result;
        }

        public string GetValidationErrors()
        {
            return this.GetValidationErrors(null);
        }

        public void ClearValidationErrors()
        {
            this.ClearValidationErrors(null);
        }

        public void ClearValidationErrors(IEnumerable<string> properties)
        {
            if (null != this.validationErrors)
            {
                if (null != properties)
                {
                    foreach (var propertyName in properties)
                    {
                        if (this.validationErrors.ContainsKey(propertyName))
                        {
                            this.validationErrors.Remove(propertyName);
                        }
                    }
                }
                else
                {
                    this.validationErrors.Clear();
                }
            }
        }

        public object GetEditor(Type editorBaseType)
        {
            object[] attributes = this.GetType().GetCustomAttributes(typeof(EditorAttribute), false);
            if (attributes.Length != 0)
            {
                EditorAttribute attribute = (EditorAttribute)attributes[0];
                return Activator.CreateInstance(Type.GetType(attribute.EditorTypeName));
            }
            return null;
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents()
        {
            return null;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return ((ICustomTypeDescriptor)this).GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            Type type = this.GetType();
            if (!DesignObjectWrapper.TypePropertyCollection.ContainsKey(type))
            {
                MethodInfo initMethod = type.GetMethod("InitializeTypeProperties", BindingFlags.Static | BindingFlags.Public);
                PropertyDescriptorData[] properties = (PropertyDescriptorData[])initMethod.Invoke(null, null);
                List<DesignObjectPropertyDescriptor> descriptors = new List<DesignObjectPropertyDescriptor>(properties.Length);
                for (int i = 0; i < properties.Length; ++i)
                {
                    properties[i].OwnerType = type;
                    DesignObjectPropertyDescriptor descriptor = new DesignObjectPropertyDescriptor(properties[i]);
                    if (null != properties[i].PropertyValidator)
                    {
                        string localPropertyName = properties[i].PropertyName;
                        PropertyDescriptorData data = new PropertyDescriptorData()
                        {
                            OwnerType = type,
                            PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                            PropertyValidator = null,
                            PropertySetter = null,
                            PropertyType = typeof(string),
                            PropertyName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", localPropertyName, ValidationErrorSuffix),
                            PropertyGetter = (instance) => (!instance.IsPropertyValid(localPropertyName) ? instance.validationErrors[localPropertyName] : string.Empty),
                        };
                        descriptors.Add(new DesignObjectPropertyDescriptor(data));
                    }
                    descriptors.Add(descriptor);
                }
                for (int i = 0; i < DesignObjectWrapper.DefaultProperties.Length; ++i)
                {
                    descriptors.Add(this.ConstructDefaultPropertyPropertyDescriptor(DesignObjectWrapper.DefaultProperties[i]));
                }
                DesignObjectWrapper.TypePropertyCollection[type] = new PropertyDescriptorCollection(descriptors.ToArray(), true);
            }

            return DesignObjectWrapper.TypePropertyCollection[type];
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public bool IsPropertyValid(string propertyName)
        {
            return this.validationErrors == null || !this.validationErrors.ContainsKey(propertyName);
        }

        public virtual void Dispose()
        {
            if (null != this.ReflectedObject && !this.isDisposed)
            {
                this.isDisposed = true;
                this.ReflectedObject.PropertyChanged -= this.OnReflectedObjectPropertyChanged;
                this.Content.PropertyChanged -= this.OnFakeModelPropertyChanged;
                if (null != this.customValueEditors)
                {
                    this.customValueEditors.Clear();
                }
                this.RaisePropertyChangedEvent("ReflectedObject");
            }
        }

        //GetDynamicPropertyValueEditor - if user marks one of the properties with DesignObjectWrapperDynamicPropertyEditor attribte,
        //it is expected that valid property value editor for each instance of design object wrapper will be provided
        internal PropertyValueEditor GetDynamicPropertyValueEditor(string propertyName)
        {
            PropertyValueEditor result = null;
            //look in the value editor cache - perhaps there is one available for given object
            if (this.CustomValueEditors.ContainsKey(propertyName))
            {
                result = this.CustomValueEditors[propertyName];
            }
            else
            {
                //no, get type of the editor
                Type editorType = this.GetDynamicPropertyValueEditorType(propertyName);
                if (null == editorType)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException("GetDynamicPropertyValueEditorType() returned null for propertyName."));
                }
                //create one
                result = (PropertyValueEditor)Activator.CreateInstance(editorType);
                //store it in cache
                this.CustomValueEditors[propertyName] = result;
            }
            return result;
        }

        internal Type GetDynamicPropertyValueEditorType(string propertyName)
        {
            //get editor type for dynamic property
            var editorType = this.OnGetDynamicPropertyValueEditorType(propertyName);
            if (null == editorType)
            {
                //there should be always be one...
                Fx.Assert(false, "PropertyValueEditor not defined for property '" + propertyName + "'");
            }
            //and it should be assignable from PropertyValueEditor
            else if (!typeof(PropertyValueEditor).IsAssignableFrom(editorType))
            {
                Fx.Assert(false, "Type '" + editorType.FullName + "' is not assignable from PropertyValueEditor");
                editorType = null;
            }
            return editorType;
        }

        //virtual OnGetDynamicProperyValueEditorType - if user marks property with DesignObjectWrapperDynamicPropertyEditor, 
        //this method has to be overriden
        protected virtual Type OnGetDynamicPropertyValueEditorType(string propertyName)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        //bool GetPropertyChangeTriggerState(string propertyName)
        //{
        //    if (this.propertyChangeTriggerState.ContainsKey(propertyName))
        //    {
        //        return this.propertyChangeTriggerState[propertyName];
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //void SetPropertyChangeTriggerState(string propertyName, bool state)
        //{
        //    if (!this.propertyChangeTriggerState.ContainsKey(propertyName))
        //    {
        //        this.propertyChangeTriggerState.Add(propertyName, state);
        //    }
        //    else
        //    {
        //        this.propertyChangeTriggerState[propertyName] = state;
        //    }
        //}

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (!this.isDisposed)
            {
                (this.Content as IModelTreeItem).OnPropertyChanged(propertyName);
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.UseEventsWhereAppropriate, Justification = "Procedure to raise the event")]
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            //don't raise property changed events if object is disposed 
            //- the underlying ModelItem might not be valid, doesn't make sense to do anything on it.
            if (!this.isDisposed)
            {
                //let the implementation react on property change
                this.OnPropertyChanged(propertyName);

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                if (!this.changingProperties.Contains(propertyName))
                {
                    if (this.Content.Properties[propertyName] != null)
                    {
                        this.changingProperties.Add(propertyName);
                        (this.Content as IModelTreeItem).OnPropertyChanged(propertyName);
                        this.changingProperties.Remove(propertyName);
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        DesignObjectPropertyDescriptor ConstructDefaultPropertyPropertyDescriptor(string propertyName)
        {
            DesignObjectPropertyDescriptor result = null;
            if (string.Equals(propertyName, HasErrorsProperty))
            {
                PropertyDescriptorData data = new PropertyDescriptorData()
                {
                    OwnerType = this.GetType(),
                    PropertyName = propertyName,
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = null,
                    PropertyType = typeof(bool),
                    PropertyValidator = null,
                    PropertyGetter = (instance) => (instance.HasErrors)
                };
                result = new DesignObjectPropertyDescriptor(data);
            }
            else if (string.Equals(propertyName, ContentProperty))
            {
                PropertyDescriptorData data = new PropertyDescriptorData()
                {
                    OwnerType = this.GetType(),
                    PropertyName = propertyName,
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = (instance, value) => { instance.Content = (ModelItem)value; },
                    PropertyType = typeof(ModelItem),
                    PropertyValidator = null,
                    PropertyGetter = (instance) => (instance.content)
                };
                result = new DesignObjectPropertyDescriptor(data);
            }
            else if (string.Equals(propertyName, AutomationIdProperty))
            {
                PropertyDescriptorData data = new PropertyDescriptorData()
                {
                    OwnerType = this.GetType(),
                    PropertyName = propertyName,
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = null,
                    PropertyType = typeof(string),
                    PropertyValidator = null,
                    PropertyGetter = (instance) => (instance.AutomationId)
                };
                result = new DesignObjectPropertyDescriptor(data);
            }
            else if (string.Equals(propertyName, TimestampProperty))
            {
                PropertyDescriptorData data = new PropertyDescriptorData()
                {
                    OwnerType = this.GetType(),
                    PropertyName = propertyName,
                    PropertyType = typeof(DateTime),
                    PropertyValidator = null,
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = (instance, value) => { instance.UpdateTimestamp(); },
                    PropertyGetter = (instance) => (instance.GetTimestamp())
                };
                result = new DesignObjectPropertyDescriptor(data);
            }
            return result;
        }

        protected bool IsUndoRedoInProgress
        {
            get
            {
                return null != this.Context && this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;
            }
        }

        void UpdateTimestamp()
        {
            this.timestamp = DateTime.Now;
            this.RaisePropertyChangedEvent(TimestampProperty);
        }

        protected DateTime GetTimestamp()
        {
            return this.timestamp;
        }

        void OnReflectedObjectPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            if (this.IsUndoRedoInProgress)
            {
                this.OnReflectedObjectPropertyChanged(e.PropertyName);
                Type type = this.GetType();
                if (DesignObjectWrapper.TypePropertyCollection.ContainsKey(type))
                {
                    PropertyDescriptorCollection properties = DesignObjectWrapper.TypePropertyCollection[type];
                    for (int i = 0; i < properties.Count; ++i)
                    {
                        if (string.Equals(properties[i].Name, e.PropertyName))
                        {
                            this.RaisePropertyChangedEvent(e.PropertyName);
                            break;
                        }
                    }
                }
            }

            //whenever data within reflected object changes, we do update timestamp property on wrapped object - 
            //this allows to create triggers and bindings which do need to be reevaluated whenever overal state of the object changes
            this.UpdateTimestamp();
        }

        protected virtual void OnReflectedObjectPropertyChanged(string propertyName)
        {
        }

        sealed class DesignObjectPropertyDescriptor : PropertyDescriptor
        {
            PropertyDescriptorData descriptorData;
            string validationErrorPropertyName;

            public DesignObjectPropertyDescriptor(PropertyDescriptorData descriptorData)
                : base(descriptorData.PropertyName, descriptorData.PropertyAttributes)
            {
                this.descriptorData = descriptorData;
                this.validationErrorPropertyName = (null != this.descriptorData.PropertyValidator ?
                    string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.descriptorData.PropertyName, DesignObjectWrapper.ValidationErrorSuffix) :
                    null);
            }

            public override bool CanResetValue(object component)
            {
                return null != this.descriptorData.PropertySetter;
            }

            public override Type ComponentType
            {
                get { return this.descriptorData.OwnerType; }
            }

            public override object GetValue(object component)
            {
                DesignObjectWrapper instance = (DesignObjectWrapper)component;
                return !instance.isDisposed ? this.descriptorData.PropertyGetter(instance) : null;
            }

            public override bool IsReadOnly
            {
                get { return null == this.descriptorData.PropertySetter; }
            }

            public override Type PropertyType
            {
                get { return this.descriptorData.PropertyType; }
            }

            public override void ResetValue(object component)
            {
                DesignObjectWrapper instance = (DesignObjectWrapper)component;
                this.descriptorData.PropertySetter(instance, null);
            }

            [SuppressMessage("Reliability", "Reliability108",
                Justification = "Exception not eaten away. If its a fatal exception we rethrow, else we wrap in another exception and throw.")]
            public override void SetValue(object component, object value)
            {
                DesignObjectWrapper instance = (DesignObjectWrapper)component;
                if (!instance.IsUndoRedoInProgress)
                {
                    if (null != this.descriptorData.PropertyValidator)
                    {
                        string error = null;
                        ValidationException exception = null;
                        try
                        {
                            List<string> errors = new List<string>();
                            if (!this.descriptorData.PropertyValidator(instance, value, errors))
                            {
                                StringBuilder sb = new StringBuilder();
                                errors.ForEach((errMessage) =>
                                    {
                                        sb.AppendLine(errMessage);
                                    });
                                error = sb.ToString();
                                exception = new ValidationException(error);
                            }
                        }
                        catch (Exception err)
                        {
                            if (Fx.IsFatal(err))
                            {
                                throw FxTrace.Exception.AsError(err);
                            }
                            else
                            {
                                exception = new ValidationException(err.Message, err);
                            }
                        }

                        if (null != exception)
                        {
                            instance.ValidationErrors[this.Name] = exception.Message;
                            instance.RaisePropertyChangedEvent(this.validationErrorPropertyName);
                            instance.RaisePropertyChangedEvent(DesignObjectWrapper.HasErrorsProperty);
                            throw FxTrace.Exception.AsError(exception);
                        }
                        else if (null != instance.validationErrors && instance.validationErrors.ContainsKey(this.Name))
                        {
                            instance.validationErrors.Remove(this.Name);
                            if (0 == instance.validationErrors.Count)
                            {
                                instance.validationErrors = null;
                            }
                            instance.RaisePropertyChangedEvent(this.validationErrorPropertyName);
                            instance.RaisePropertyChangedEvent(DesignObjectWrapper.HasErrorsProperty);
                        }
                    }

                    this.descriptorData.PropertySetter(instance, value);

                    (instance.Content as IModelTreeItem).ModelTreeManager.AddToCurrentEditingScope(new FakeModelNotifyPropertyChange(instance.Content as IModelTreeItem, this.Name));
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }

    sealed class PropertyDescriptorData
    {
        public Type OwnerType { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public Func<DesignObjectWrapper, object> PropertyGetter { get; set; }
        public Action<DesignObjectWrapper, object> PropertySetter { get; set; }
        public Func<DesignObjectWrapper, object, List<string>, bool> PropertyValidator { get; set; }
        [SuppressMessage(FxCop.Category.Performance, "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Array type property does not clone the array in the getter. It references the same array instance.")]
        public Attribute[] PropertyAttributes { get; set; }

    }

    //DesignObjectWrapperDynamicPropertyEditor - this class is used to allow defining different value editors for given set of DesignObjectWrappers.
    //i.e. for generic Variable<T>, user may want to provide different editors for variable's value, depending on generic type placeholder -
    // Variable<string> - would use default editor, but Variable<CustomType> can provide different editing expirience
    sealed class DesignObjectWrapperDynamicPropertyEditor : DialogPropertyValueEditor
    {
        static DataTemplate dynamicExpressionTemplate;

        //DynamicExpressionTemplate - this template defines a content presenter, which will be filled with default or custom type editor
        static DataTemplate DynamicExpressionTemplate
        {
            get
            {
                if (null == dynamicExpressionTemplate)
                {
                    dynamicExpressionTemplate = new DataTemplate();
                    var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                    contentPresenterFactory.SetBinding(ContentPresenter.ContentProperty, new Binding());
                    contentPresenterFactory.SetBinding(ContentPresenter.TagProperty, new Binding() { Converter = ModelPropertyEntryToOwnerActivityConverter, ConverterParameter = false, Path = new PropertyPath("ParentProperty") });
                    MultiBinding binding = new MultiBinding() { Converter = new TemplateConverter() };
                    binding.Bindings.Add(new Binding());
                    binding.Bindings.Add(new Binding() { Path = new PropertyPath("Tag.Timestamp"), Mode = BindingMode.OneWay, RelativeSource = RelativeSource.Self });
                    contentPresenterFactory.SetBinding(ContentPresenter.ContentTemplateProperty, binding);
                    dynamicExpressionTemplate.VisualTree = contentPresenterFactory;
                    dynamicExpressionTemplate.Seal();
                }
                //dynamicExpressionTemplate = (DataTemplate)EditorResources.GetResources()["dynamicExpressionTemplate"];
                return dynamicExpressionTemplate;
            }
        }

        static IValueConverter ModelPropertyEntryToModelItemConverter
        {
            get
            {
                return (ModelPropertyEntryToModelItemConverter)EditorResources.GetResources()["ModelPropertyEntryToContainerConverter"];
            }
        }

        static IValueConverter ModelPropertyEntryToOwnerActivityConverter
        {
            get
            {
                return (ModelPropertyEntryToOwnerActivityConverter)EditorResources.GetResources()["ModelPropertyEntryToOwnerActivityConverter"];
            }
        }

        //helper method - gets property value editor for property marked with DesignObjectWrapperDynamicPropertyEditor 
        static PropertyValueEditor GetEditor(PropertyValue propertyValue)
        {
            //convert property value to set of { ModelItem, Context, PropertyValue } 
            var content = (ModelPropertyEntryToModelItemConverter.Container)
                DesignObjectWrapperDynamicPropertyEditor.ModelPropertyEntryToModelItemConverter.Convert(propertyValue, null, null, null);

            //get current instance of design object wrapper
            var wrapper = (DesignObjectWrapper)content.ModelItem.GetCurrentValue();

            //query it for actual value editor
            var editor = wrapper.GetDynamicPropertyValueEditor(propertyValue.ParentProperty.PropertyName);

            if (null == editor)
            {
                Fx.Assert(false, "PropertyValue editor not found for '" + propertyValue.ParentProperty.PropertyName + "'");
            }
            return editor;
        }

        public DesignObjectWrapperDynamicPropertyEditor()
        {
            this.InlineEditorTemplate = DesignObjectWrapperDynamicPropertyEditor.DynamicExpressionTemplate;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Propagating exceptions might lead to VS crash.")]
        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            //get actual value editor
            var editor = DesignObjectWrapperDynamicPropertyEditor.GetEditor(propertyValue);

            Fx.Assert(editor is DialogPropertyValueEditor, "PropertyValueEditor is not assigned or is not derived from DialogPropertyValueEditor");

            //if we are here, the editor must derive from DialogPropertyEditor, if it doesn't user provided wrong template
            if (editor is DialogPropertyValueEditor)
            {
                try
                {
                    ((DialogPropertyValueEditor)editor).ShowDialog(propertyValue, commandSource);
                }
                catch (Exception err)
                {
                    ErrorReporting.ShowErrorMessage(err.ToString());
                }
            }
        }

        //helper class - allows pulling template definition for dynamic property value
        private sealed class TemplateConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                object template = Binding.DoNothing;
                if (null != values[0])
                {
                    var editor = DesignObjectWrapperDynamicPropertyEditor.GetEditor((PropertyValue)values[0]);
                    if (null != editor)
                    {
                        template = editor.InlineEditorTemplate;
                    }
                }
                return template;
            }

            public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }
        }
    }

}
