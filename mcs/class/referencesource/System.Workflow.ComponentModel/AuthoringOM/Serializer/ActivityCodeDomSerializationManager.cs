namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    #region Class ActivityCodeDomSerializationManager
    /// <summary>
    /// Serialization manager class for code only serialization which inherits from 
    /// WorkflowMarkupSerializationManager and overrides the GetService method to return the
    /// code-only specific reference service.
    /// </summary>
    /// <remarks>
    /// Due to the fact that the base classes used by the code-only serializer from 
    /// System.Design rely on the reference service to generate code variable ids, we
    /// need to supply our own version of the reference service, since the default
    /// generates invalid variable ids for Activity based classes.
    /// </remarks>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityCodeDomSerializationManager : IDesignerSerializationManager
    {
        private ServiceContainer serviceContainer = null;
        private IDesignerSerializationManager serializationManager;
        private ResolveNameEventHandler resolveNameEventHandler;
        private EventHandler serializationCompleteEventHandler;

        public ActivityCodeDomSerializationManager(IDesignerSerializationManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.serializationManager = manager;
            this.serviceContainer = new ServiceContainer();
            this.serializationManager.ResolveName += new ResolveNameEventHandler(OnResolveName);
            this.serializationManager.SerializationComplete += new EventHandler(OnSerializationComplete);
            if (this.serializationManager is DesignerSerializationManager)
                ((DesignerSerializationManager)this.serializationManager).SessionDisposed += new EventHandler(OnSessionDisposed);
        }
        void OnResolveName(object sender, ResolveNameEventArgs e)
        {
            if (this.resolveNameEventHandler != null)
                this.resolveNameEventHandler(this, e);
        }

        void OnSerializationComplete(object sender, EventArgs e)
        {
            if (this.serializationCompleteEventHandler != null)
                this.serializationCompleteEventHandler(this, e);
        }
        void OnSessionDisposed(object sender, EventArgs e)
        {
            try
            {
                if (this.serializationCompleteEventHandler != null)
                    this.serializationCompleteEventHandler(this, EventArgs.Empty);
            }
            finally
            {
                this.resolveNameEventHandler = null;
                this.serializationCompleteEventHandler = null;
            }
        }
        public event System.EventHandler SerializationComplete
        {
            add
            {
                this.serializationCompleteEventHandler += value;
            }
            remove
            {
                this.serializationCompleteEventHandler -= value;
            }
        }

        public void SetName(object instance, string name)
        {
            // Need to check this since code dom deserialization tries to 
            // set name twice on all object creation statements.
            if (GetInstance(name) != instance)
                this.serializationManager.SetName(instance, name);
        }


        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.LastIndexOf(System.String)", Justification = "This is not a security threat since it is called only in XAML design time scenarios")]
        public string GetName(object value)
        {
            string name = null;
            Activity a = value as Activity;
            if (a != null)
            {
                if (a.Parent == null)
                {
                    name = a.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                    if (name != null && name.LastIndexOf(".") > 0)
                        name = name.Substring(name.LastIndexOf('.') + 1);
                }
                else
                    name = a.QualifiedName.Replace('.', '_');
            }

            if (name == null)
                name = this.serializationManager.GetName(value);

            return name;
        }

        /// <summary>
        /// Overridded to force siteing of all Activity based instances created, enabling
        /// code-only programmers to write activity.Activities.Add(new activityType());
        /// </summary>
        public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            return this.serializationManager.CreateInstance(type, arguments, name, false);
        }

        public object GetService(Type serviceType)
        {
            object service = null;

            if (serviceType == typeof(IReferenceService))
                service = new ActivityCodeDomReferenceService(this.serializationManager.GetService(serviceType) as IReferenceService);

            if (serviceType == typeof(IServiceContainer))
            {
                service = serializationManager.GetService(serviceType);

                if (service == null)
                {
                    service = serviceContainer;
                }
            }

            // If we haven't provded the service, try the serialization manager we wrap
            if (service == null)
                service = this.serializationManager.GetService(serviceType);

            // Last ditch effort, see if our consumer had added the service
            if (service == null)
                service = serviceContainer.GetService(serviceType);

            return service;
        }

        // work around : PD7's PrimitiveCodeDomSerializer does not handle well strings bigger than 200 characters, 
        //       we push our own version to fix it.
        public object GetSerializer(Type objectType, Type serializerType)
        {
            if (objectType == typeof(string))
            {
                return PrimitiveCodeDomSerializer.Default;
            }
            else if (objectType != null && TypeProvider.IsAssignable(typeof(ICollection<String>), objectType) && !objectType.IsArray && serializerType == typeof(CodeDomSerializer))
            {
                PropertyDescriptor pd = Context[typeof(PropertyDescriptor)] as PropertyDescriptor;
                if (pd != null)
                {
                    if (string.Equals(pd.Name, "SynchronizationHandles", StringComparison.Ordinal) && TypeProvider.IsAssignable(typeof(Activity), pd.ComponentType))
                        return new SynchronizationHandlesCodeDomSerializer();
                }
                else
                {
                    // If property descriptor is not available, we then look at the expression context.
                    ExpressionContext context = Context[typeof(ExpressionContext)] as ExpressionContext;
                    if (context != null && context.Expression is CodePropertyReferenceExpression &&
                        string.Equals(((CodePropertyReferenceExpression)context.Expression).PropertyName, "SynchronizationHandles", StringComparison.Ordinal))
                        return new SynchronizationHandlesCodeDomSerializer();
                }
            }

            object serializer = this.serializationManager.GetSerializer(objectType, serializerType);
            if (!UseUserDefinedSerializer(objectType, serializerType))
                serializer = new SerializableTypeCodeDomSerializer(serializer as CodeDomSerializer);

            return serializer;
        }

        // We add this custom serializable to handler serializable types so they don't go into the resources
        // as majority of our types will be serializable per WinOE runtime requirements.
        // Note: we will not overwrite any custom serializer behaviors.  If the serializer does not come
        // from the Sytem.ComponentModel.dll, we will not overwrite it.
        private bool UseUserDefinedSerializer(Type objectType, Type serializerType)
        {
            if (objectType == null || serializerType == null)
                return true;

            //If objectType is not serializable or we are not looking for codedomserializer then use user defined serializer
            if (!objectType.IsSerializable || serializerType != typeof(CodeDomSerializer))
                return true;

            //For primitives always honor user defined serializer
            if (objectType.IsPrimitive || objectType.IsEnum || objectType == typeof(string) || typeof(Activity).IsAssignableFrom(objectType))
                return true;

            //If user has defined instance descriptor then we always serialize to a create expression so we honor
            //user defined serializer
            TypeConverter converter = TypeDescriptor.GetConverter(objectType);
            if (converter != null && converter.CanConvertTo(typeof(InstanceDescriptor)))
                return true;

            //If the serializer is a custom codedom serializer ie it does not come from our or system  assembly
            //defining codedomserializer then always honor user's serializer as user needs a way to override the
            //serializartion process
            object serializer = this.serializationManager.GetSerializer(objectType, serializerType);
            if (serializer.GetType().Assembly != typeof(CodeDomSerializer).Assembly &&
                serializer.GetType().Assembly != Assembly.GetExecutingAssembly() &&
                serializer.GetType().Assembly != Assembly.Load(AssemblyRef.ActivitiesAssemblyRef))
                return true;

            //Special case for UI objects they need to always come from the resource only if we are not compiling
            Activity activity = this.serializationManager.Context[typeof(Activity)] as Activity;
            if (activity != null && activity.Site != null && activity.Site.Container != null &&
                objectType.Namespace != null && objectType.Namespace.Equals(typeof(System.Drawing.Image).Namespace))
                return true;

            return false;
        }

        public object GetInstance(string name)
        {
            return this.serializationManager.GetInstance(name);
        }

        public void AddSerializationProvider(IDesignerSerializationProvider provider)
        {
            this.serializationManager.AddSerializationProvider(provider);
        }

        public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
        {
            this.serializationManager.RemoveSerializationProvider(provider);
        }

        public void ReportError(object errorInformation)
        {
            this.serializationManager.ReportError(errorInformation);
        }
        public ContextStack Context
        {
            get
            {
                return this.serializationManager.Context;
            }
        }

        public event ResolveNameEventHandler ResolveName
        {
            add
            {
                this.resolveNameEventHandler += value;
            }
            remove
            {
                this.resolveNameEventHandler -= value;
            }
        }

        public PropertyDescriptorCollection Properties
        {
            get
            {
                return this.serializationManager.Properties;
            }
        }


        public Type GetType(string typeName)
        {
            Type type = this.serializationManager.GetType(typeName);
            if (type == null)
            {
                // if this is a design time time
                ITypeProvider typeProvider = this.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                    type = typeProvider.GetType(typeName);
            }

            return type;
        }

        protected IDesignerSerializationManager SerializationManager
        {
            get
            {
                return this.serializationManager;
            }

            set
            {
                this.serializationManager = value;
            }
        }
    }
    #endregion

}
