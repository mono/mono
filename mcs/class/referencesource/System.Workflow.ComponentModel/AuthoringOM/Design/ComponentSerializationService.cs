namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Reflection;
    using System.Collections;
    using System.Globalization;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Diagnostics.CodeAnalysis;

    internal class XomlComponentSerializationService : ComponentSerializationService
    {
        private IServiceProvider serviceProvider = null;
        internal XomlComponentSerializationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override SerializationStore CreateStore()
        {
            return new WorkflowMarkupSerializationStore(this.serviceProvider);
        }

        public override SerializationStore LoadStore(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            BinaryFormatter f = new BinaryFormatter();
            return (WorkflowMarkupSerializationStore)f.Deserialize(stream);
        }

        // error CS0534: 'System.Workflow.ComponentModel.Design.XomlComponentSerializationService' does not implement inherited abstract member 'System.ComponentModel.Design.Serialization.ComponentSerializationService.SerializeAbsolute(System.ComponentModel.Design.Serialization.SerializationStore, object)'
        public override void SerializeAbsolute(SerializationStore store, object value)
        {
            this.Serialize(store, value);
        }

        public override void Serialize(SerializationStore store, object value)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (value == null) throw new ArgumentNullException("value");
            WorkflowMarkupSerializationStore xomlStore = store as WorkflowMarkupSerializationStore;
            if (xomlStore == null) throw new InvalidOperationException(SR.GetString(SR.Error_UnknownSerializationStore));
            xomlStore.AddObject(value);
        }

        //error CS0534: 'System.Workflow.ComponentModel.Design.XomlComponentSerializationService' does not implement inherited abstract member 'System.ComponentModel.Design.Serialization.ComponentSerializationService.SerializeMemberAbsolute(System.ComponentModel.Design.Serialization.SerializationStore, object, System.ComponentModel.MemberDescriptor)'
        public override void SerializeMemberAbsolute(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            this.SerializeMember(store, owningObject, member);
        }

        public override void SerializeMember(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (owningObject == null) throw new ArgumentNullException("owningObject");
            if (member == null) throw new ArgumentNullException("member");
            WorkflowMarkupSerializationStore xomlStore = store as WorkflowMarkupSerializationStore;
            if (xomlStore == null) throw new InvalidOperationException(SR.GetString(SR.Error_UnknownSerializationStore));
            xomlStore.AddMember(owningObject, member);
        }

        public override ICollection Deserialize(SerializationStore store)
        {
            if (store == null) throw new ArgumentNullException("store");
            WorkflowMarkupSerializationStore xomlStore = store as WorkflowMarkupSerializationStore;
            if (xomlStore == null) throw new InvalidOperationException(SR.GetString(SR.Error_UnknownSerializationStore));
            return (ICollection)xomlStore.Deserialize(this.serviceProvider);
        }
        public override ICollection Deserialize(SerializationStore store, IContainer container)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (container == null) throw new ArgumentNullException("container");
            WorkflowMarkupSerializationStore xomlStore = store as WorkflowMarkupSerializationStore;
            if (xomlStore == null) throw new InvalidOperationException(SR.GetString(SR.Error_UnknownSerializationStore));
            return xomlStore.Deserialize(this.serviceProvider, container);
        }

        // build 40409
        // error CS0115: 'System.Workflow.ComponentModel.Design.XomlComponentSerializationService.DeserializeTo(System.ComponentModel.Design.Serialization.SerializationStore, System.ComponentModel.IContainer)': no suitable method found to override
        // build 40420
        // error CS0506: 'System.Workflow.ComponentModel.Design.XomlComponentSerializationService.DeserializeTo(System.ComponentModel.Design.Serialization.SerializationStore, System.ComponentModel.IContainer, bool)': cannot override inherited member 'System.ComponentModel.Design.Serialization.ComponentSerializationService.DeserializeTo(System.ComponentModel.Design.Serialization.SerializationStore, System.ComponentModel.IContainer, bool)' because it is not marked virtual, abstract, or override
        public override void DeserializeTo(SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults)
        {
            // 
            if (store == null) throw new ArgumentNullException("store");
            if (container == null) throw new ArgumentNullException("container");
            WorkflowMarkupSerializationStore xomlStore = store as WorkflowMarkupSerializationStore;
            if (xomlStore == null) throw new InvalidOperationException(SR.GetString(SR.Error_UnknownSerializationStore));
            xomlStore.DeserializeTo(this.serviceProvider, container);
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingFlags)
        {
            PropertyInfo propertyInfo = null;

            try
            {
                propertyInfo = type.GetProperty(name, bindingFlags);
            }
            catch (AmbiguousMatchException)
            {
                // this will ensure properties with "new" keyword are detected
                PropertyInfo[] properties = type.GetProperties(bindingFlags);
                foreach (PropertyInfo prop in properties)
                {
                    if (prop.Name.Equals(name, StringComparison.Ordinal))
                    {
                        propertyInfo = prop;
                        break;
                    }
                }
            }
            return propertyInfo;
        }
    }

    [Serializable]
    internal class WorkflowMarkupSerializationStore : SerializationStore,
                                            ISerializable
    {
        // keys used to persist data in binary stream
        private const string SerializedXmlString = "XmlString";
        private const string AssembliesKey = "Assemblies";

        // these field are only used for Store creation.
        private IServiceProvider serviceProvider = null;
        private List<Activity> activities = new List<Activity>();
        private List<string> parentObjectNameList = new List<string>();
        private List<MemberDescriptor> memberList = new List<MemberDescriptor>();

        // these fields persist across the store
        private string serializedXmlString;
        private AssemblyName[] assemblies;


        internal WorkflowMarkupSerializationStore(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private WorkflowMarkupSerializationStore(SerializationInfo info, StreamingContext context)
        {
            this.serializedXmlString = (String)info.GetValue(SerializedXmlString, typeof(String));
            this.assemblies = (AssemblyName[])info.GetValue(AssembliesKey, typeof(AssemblyName[]));
        }

        private AssemblyName[] AssemblyNames
        {
            get
            {
                return this.assemblies;
            }
        }

        internal void AddObject(object value)
        {
            if (this.serializedXmlString != null)
                throw new InvalidOperationException(DR.GetString(DR.InvalidOperationStoreAlreadyClosed));

            Activity activity = value as Activity;
            if (activity == null)
                throw new ArgumentException("value");

            this.activities.Add(activity);
        }

        internal void AddMember(object value, MemberDescriptor member)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (member == null)
                throw new ArgumentNullException("member");
            if (this.serializedXmlString != null)
                throw new InvalidOperationException(DR.GetString(DR.InvalidOperationStoreAlreadyClosed));

            IReferenceService referenceService = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            this.parentObjectNameList.Add(referenceService.GetName(value));
            this.memberList.Add(member);
        }

        internal IList Deserialize(IServiceProvider serviceProvider)
        {
            DesignerSerializationManager serializationManager = new LocalDesignerSerializationManager(this, serviceProvider);
            using (serializationManager.CreateSession())
            {
                ArrayList objects = new ArrayList();
                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
                XmlTextReader reader = new XmlTextReader(this.serializedXmlString, XmlNodeType.Element, null);
                reader.MoveToElement();
                do
                {
                    if (!reader.Read())
                        return objects;

                    xomlSerializationManager.FoundDefTag += delegate(object sender, WorkflowMarkupElementEventArgs eventArgs)
                    {
                        if (eventArgs.XmlReader.LookupNamespace(eventArgs.XmlReader.Prefix) == StandardXomlKeys.Definitions_XmlNs &&
                            xomlSerializationManager.Context.Current is Activity
                            )
                            WorkflowMarkupSerializationHelpers.ProcessDefTag(xomlSerializationManager, eventArgs.XmlReader, xomlSerializationManager.Context.Current as Activity, true, string.Empty);
                    };

                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                    object activityDecl = xomlSerializer.DeserializeObject(xomlSerializationManager, reader);
                    if (activityDecl == null)
                        throw new InvalidOperationException(DR.GetString(DR.InvalidOperationDeserializationReturnedNonActivity));
                    if (activityDecl is Activity)
                        (activityDecl as Activity).UserData.Remove(UserDataKeys.CustomActivity);
                    objects.Add(activityDecl);
                } while (true);
            }
        }

        internal ICollection Deserialize(IServiceProvider serviceProvider, IContainer container)
        {
            throw new NotImplementedException();
        }

        internal void DeserializeTo(IServiceProvider serviceProvider, IContainer container)
        {
            DesignerSerializationManager serializationManager = new LocalDesignerSerializationManager(this, serviceProvider);

            using (serializationManager.CreateSession())
            {
                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
                PropertySegmentSerializationProvider propertySegmentSerializationProvider = new PropertySegmentSerializationProvider();
                xomlSerializationManager.AddSerializationProvider(propertySegmentSerializationProvider);

                StringReader stringReader = new StringReader(this.serializedXmlString);
                using (XmlTextReader reader = new XmlTextReader(stringReader))
                {
                    while (reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.ProcessingInstruction && reader.Read());

                    IReferenceService referenceService = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    IComponentChangeService componentChangeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    for (int loop = 0; loop < this.memberList.Count; loop++)
                    {
                        object obj = referenceService.GetReference(this.parentObjectNameList[loop]);
                        if (obj != null)
                        {
                            bool needChangeEvent = (componentChangeService != null) && (!(obj is IComponent) || (((IComponent)obj).Site == null));

                            PropertyDescriptor member = this.memberList[loop] as PropertyDescriptor;
                            if (needChangeEvent)
                                componentChangeService.OnComponentChanging(obj, member);

                            xomlSerializationManager.Context.Push(obj);
                            PropertySegmentSerializer serializer = new PropertySegmentSerializer(null);
                            PropertySegment propertySegment = serializer.DeserializeObject(xomlSerializationManager, reader) as PropertySegment;
                            System.Diagnostics.Debug.Assert(obj == xomlSerializationManager.Context.Current, "Serialization Store did not remove object which it pushed onto the stack.");
                            xomlSerializationManager.Context.Pop();

                            if (needChangeEvent)
                                componentChangeService.OnComponentChanged(obj, member, null, null);
                        }
                    }
                }

                xomlSerializationManager.RemoveSerializationProvider(propertySegmentSerializationProvider);
            }
        }

        #region ISerializable implementation

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SerializedXmlString, this.serializedXmlString);
            info.AddValue(AssembliesKey, this.assemblies);
        }

        #endregion

        #region SerializationStore overrides

        public override void Save(Stream stream)
        {
            Close();
            BinaryFormatter f = new BinaryFormatter();
            f.Serialize(stream, this);
        }

        public override void Close()
        {
            if (this.serializedXmlString != null)
                return;

            DesignerSerializationManager serializationManager = new LocalDesignerSerializationManager(this, serviceProvider);
            using (serializationManager.CreateSession())
            {
                // serialize all objects 
                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                {
                    if (this.memberList.Count == 0)
                    {
                        WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                        foreach (Activity activity in this.activities)
                            xomlSerializer.SerializeObject(xomlSerializationManager, activity, writer);
                    }
                    else
                    {
                        PropertySegmentSerializationProvider propertySegmentSerializationProvider = new PropertySegmentSerializationProvider();
                        xomlSerializationManager.AddSerializationProvider(propertySegmentSerializationProvider);

                        xomlSerializationManager.Context.Push(new StringWriter(CultureInfo.InvariantCulture));

                        IReferenceService referenceService = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                        if (referenceService != null)
                        {
                            for (int loop = 0; loop < this.memberList.Count; loop++)
                            {
                                object obj = referenceService.GetReference(this.parentObjectNameList[loop]);
                                PropertySegmentSerializer serializer = new PropertySegmentSerializer(null);
                                if (this.memberList[loop] is PropertyDescriptor)
                                {
                                    PropertyInfo propertyInfo = XomlComponentSerializationService.GetProperty(obj.GetType(), (this.memberList[loop] as PropertyDescriptor).Name, BindingFlags.Public | BindingFlags.Instance);
                                    if (propertyInfo != null)
                                        serializer.SerializeObject(xomlSerializationManager, new PropertySegment(this.serviceProvider, obj, propertyInfo), writer);
                                    else
                                        serializer.SerializeObject(xomlSerializationManager, new PropertySegment(this.serviceProvider, obj, this.memberList[loop] as PropertyDescriptor), writer);
                                }
                                else if (this.memberList[loop] is EventDescriptor)
                                {
                                    // Events.
                                    IEventBindingService eventBindingService = this.serviceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
                                    if (eventBindingService != null)
                                    {
                                        PropertySegment propertySegment = new PropertySegment(serviceProvider, obj, eventBindingService.GetEventProperty(this.memberList[loop] as EventDescriptor));
                                        serializer.SerializeObject(xomlSerializationManager, propertySegment, writer);
                                    }
                                }
                            }
                        }

                        xomlSerializationManager.Context.Pop();
                        xomlSerializationManager.RemoveSerializationProvider(propertySegmentSerializationProvider);
                    }
                }
                this.serializedXmlString = stringWriter.ToString();

                // store all the assembly names
                List<AssemblyName> assemblyList = new List<AssemblyName>();
                foreach (Activity activity in this.activities)
                {
                    Assembly a = activity.GetType().Assembly;
                    assemblyList.Add(a.GetName(true));
                }
                this.assemblies = assemblyList.ToArray();
                this.activities.Clear();
                this.activities = null;
            }
        }

        public override System.Collections.ICollection Errors
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region Class LocalDesignerSerializationManager
        private class LocalDesignerSerializationManager : DesignerSerializationManager
        {
            private WorkflowMarkupSerializationStore store;

            internal LocalDesignerSerializationManager(WorkflowMarkupSerializationStore store, IServiceProvider provider)
                : base(provider)
            {
                this.store = store;
            }


            [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)", Justification = "This is not a security issue because its a design time class")]
            protected override Type GetType(string name)
            {
                Type t = base.GetType(name);
                if (t == null)
                {
                    //When we try to load type from an assembly we need only the type name and not
                    //assembly qualified type name
                    int index = name.IndexOf(",");
                    if (index != -1)
                        name = name.Substring(0, index);

                    AssemblyName[] names = this.store.AssemblyNames;
                    foreach (AssemblyName n in names)
                    {
                        Assembly a = Assembly.Load(n);
                        if (a != null)
                        {
                            t = a.GetType(name);
                            if (t != null)
                                break;
                        }
                    }

                    if (t == null)
                    {
                        // Failing that go after their dependencies.
                        foreach (AssemblyName n in names)
                        {
                            Assembly a = Assembly.Load(n);
                            if (a != null)
                            {
                                foreach (AssemblyName dep in a.GetReferencedAssemblies())
                                {
                                    Assembly aDep = Assembly.Load(dep);
                                    if (aDep != null)
                                    {
                                        t = aDep.GetType(name);
                                        if (t != null)
                                            break;
                                    }
                                }
                                if (t != null)
                                    break;
                            }
                        }
                    }
                }
                return t;
            }
        }
        #endregion
    }

    #region Class PropertySegment
    internal sealed class PropertySegment
    {
        private IServiceProvider serviceProvider = null;
        private object obj = null;
        private PropertyInfo property = null;
        private PropertyDescriptor propertyDescriptor = null;

        public PropertySegment(IServiceProvider serviceProvider, object obj)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
        }

        internal PropertySegment(IServiceProvider serviceProvider, object obj, PropertyInfo property)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
            this.property = property;
        }

        internal PropertySegment(IServiceProvider serviceProvider, object obj, PropertyDescriptor propertyDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
            this.propertyDescriptor = propertyDescriptor;
        }

        internal object Object
        {
            get { return this.obj; }
        }

        internal IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }

        internal PropertyDescriptor PropertyDescriptor
        {
            get
            {
                PropertyDescriptor propertyDesc = this.propertyDescriptor;
                if (propertyDesc == null && this.obj != null && this.property != null)
                    propertyDesc = TypeDescriptor.GetProperties(this.obj)[this.property.Name];
                return propertyDesc;
            }
        }

        internal PropertyInfo[] GetProperties(IServiceProvider serviceProvider)
        {
            ArrayList properties = new ArrayList(GetType().GetProperties());

            if (this.property != null)
            {
                properties.Add(new PropertySegmentPropertyInfo(this, this.property));
            }
            else if (this.propertyDescriptor != null)
            {
                properties.Add(new PropertySegmentPropertyInfo(this, this.propertyDescriptor));
            }
            else if (this.obj != null)
            {
                PropertyDescriptorCollection props = null;
                TypeConverter converter = TypeDescriptor.GetConverter(this.obj);
                if (converter != null && converter.GetPropertiesSupported())
                {
                    DummyTypeDescriptorContext dummyContext = new DummyTypeDescriptorContext(this.serviceProvider, GetComponent(this.obj, serviceProvider), null);
                    props = converter.GetProperties(dummyContext, this.obj, new Attribute[] { });
                }
                else
                    props = TypeDescriptor.GetProperties(this.obj);

                foreach (PropertyDescriptor propDesc in props)
                {
                    PropertyInfo propInfo = XomlComponentSerializationService.GetProperty(this.obj.GetType(), propDesc.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo != null)
                    {
                        if (Helpers.GetSerializationVisibility(propInfo) == DesignerSerializationVisibility.Hidden)
                            continue;

                        properties.Add(new PropertySegmentPropertyInfo(this, propInfo));
                    }
                    else
                    {
                        properties.Add(new PropertySegmentPropertyInfo(this, propDesc));
                        if (propDesc.Converter != null)
                        {
                            DummyTypeDescriptorContext dummyContext = new DummyTypeDescriptorContext(this.serviceProvider, GetComponent(this.obj, serviceProvider), propDesc);
                            if (propDesc.Converter.GetPropertiesSupported(dummyContext))
                            {
                                foreach (PropertyDescriptor childDesc in propDesc.Converter.GetProperties(dummyContext, this.obj, new Attribute[] { }))
                                {
                                    properties.Add(new PropertySegmentPropertyInfo(this, childDesc));
                                }
                            }
                        }
                    }
                }
            }

            return properties.ToArray(typeof(PropertyInfo)) as PropertyInfo[];
        }

        private IComponent GetComponent(object obj, IServiceProvider serviceProvider)
        {
            IComponent component = obj as IComponent;

            if ((component == null || component.Site == null) && serviceProvider != null)
            {
                IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                if (rs != null)
                    component = rs.GetComponent(obj);
            }
            return component;
        }
    }
    #endregion

    #region Class DummyTypeDescriptorContext
    internal class DummyTypeDescriptorContext : ITypeDescriptorContext
    {
        private IServiceProvider serviceProvider;
        private object component = null;
        private PropertyDescriptor propDescriptor = null;

        public DummyTypeDescriptorContext(IServiceProvider serviceProvider, object component, PropertyDescriptor propDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.propDescriptor = propDescriptor;
            this.component = component;
        }

        public IContainer Container { get { return null; } }

        public object Instance { get { return this.component; } }

        public PropertyDescriptor PropertyDescriptor { get { return this.propDescriptor; } }

        public void OnComponentChanged() { }
        public bool OnComponentChanging() { return true; }

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider != null)
                return this.serviceProvider.GetService(serviceType);
            else
                return null;
        }
    }
    #endregion

    #region Class PropertySegmentPropertyInfo
    internal sealed class PropertySegmentPropertyInfo : PropertyInfo
    {
        private PropertyInfo realPropInfo = null;
        private PropertyDescriptor realPropDesc = null;
        private PropertySegment propertySegment;

        internal PropertySegmentPropertyInfo(PropertySegment propertySegment, PropertyInfo realPropInfo)
        {
            this.realPropInfo = realPropInfo;
            this.propertySegment = propertySegment;
        }

        internal PropertySegmentPropertyInfo(PropertySegment propertySegment, PropertyDescriptor realPropDesc)
        {
            this.realPropDesc = realPropDesc;
            this.propertySegment = propertySegment;
        }

        internal PropertySegment PropertySegment
        {
            get
            {
                return this.propertySegment;
            }
        }

        #region Property Info overrides

        public override Type PropertyType
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.PropertyType;
                else if (this.realPropDesc != null)
                    return this.realPropDesc.PropertyType;

                return null;
            }
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            DependencyProperty dependencyProperty = null;
            Activity activity = null;
            if (this.propertySegment != null)
                activity = this.propertySegment.Object as Activity;

            if (activity != null)
            {
                string propertyName = Name;
                Type propertyType = DeclaringType;
                if (!String.IsNullOrEmpty(propertyName) && propertyType != null)
                    dependencyProperty = DependencyProperty.FromName(propertyName, propertyType);
            }

            object value = null;
            object targetObj = (this.propertySegment == null) ? obj : this.propertySegment.Object;
            if (dependencyProperty != null && !dependencyProperty.DefaultMetadata.IsMetaProperty)
            {
                // If this is not a Bind, we retrieve the value through the property descriptor.
                // If we have directly assigned the value to the property then GetBinding is going to return null
                // If that happens then we need to make sure that we get at the actual value
                if (activity.IsBindingSet(dependencyProperty))
                    value = activity.GetBinding(dependencyProperty);
                else if (!dependencyProperty.IsEvent)
                    value = activity.GetValue(dependencyProperty);
                else
                    value = activity.GetHandler(dependencyProperty);
            }

            if (value == null)
            {
                if (this.realPropInfo != null)
                    value = this.realPropInfo.GetValue(targetObj, invokeAttr, binder, index, culture);
                else if (this.realPropDesc != null)
                    value = this.realPropDesc.GetValue(targetObj);
            }

            return value;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            string propertyName = Name;

            DependencyProperty dependencyProperty = null;
            PropertySegment propertySegment = obj as PropertySegment;
            Activity activity = (propertySegment != null) ? propertySegment.Object as Activity : obj as Activity;
            if (activity != null)
            {
                Type propertyType = DeclaringType;
                if (!String.IsNullOrEmpty(propertyName) && propertyType != null)
                    dependencyProperty = DependencyProperty.FromName(propertyName, propertyType);
            }

            PropertyDescriptor propertyDescriptor = null;
            object destnObject = null;
            if (propertySegment != null)
            {
                PropertyDescriptorCollection props = null;
                TypeConverter converter = TypeDescriptor.GetConverter(propertySegment.Object);
                if (converter != null && converter.GetPropertiesSupported())
                {
                    DummyTypeDescriptorContext dummyContext = new DummyTypeDescriptorContext(propertySegment.ServiceProvider, propertySegment.Object, this.realPropDesc);
                    props = converter.GetProperties(dummyContext, propertySegment.Object, new Attribute[] { });
                }
                else
                    props = TypeDescriptor.GetProperties(propertySegment.Object);

                foreach (PropertyDescriptor propDesc in props)
                {
                    if (propDesc.Name == propertyName)
                    {
                        propertyDescriptor = propDesc;
                    }
                    else if (propDesc.Converter != null)
                    {
                        DummyTypeDescriptorContext dummyContext = new DummyTypeDescriptorContext(propertySegment.ServiceProvider, propertySegment.Object, propDesc);
                        if (propDesc.GetValue(propertySegment.Object) != null && propDesc.Converter.GetPropertiesSupported(dummyContext))
                        {
                            foreach (PropertyDescriptor childDesc in propDesc.Converter.GetProperties(dummyContext, propDesc.GetValue(propertySegment.Object), new Attribute[] { }))
                            {
                                if (childDesc.Name == propertyName)
                                    propertyDescriptor = childDesc;
                            }
                        }
                    }
                }

                destnObject = propertySegment.Object;
            }
            else
            {
                propertyDescriptor = TypeDescriptor.GetProperties(obj)[propertyName];
                destnObject = obj;
            }

            if (propertyDescriptor != null && destnObject != null)
                propertyDescriptor.SetValue(destnObject, value);
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetAccessors(nonPublic);

            return new MethodInfo[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetGetMethod(nonPublic);

            return null;
        }
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetSetMethod(nonPublic);

            return null;
        }
        public override ParameterInfo[] GetIndexParameters()
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetIndexParameters();

            return new ParameterInfo[0];
        }
        public override PropertyAttributes Attributes
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.Attributes;

                return PropertyAttributes.None;
            }
        }
        public override bool CanRead
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.CanRead;

                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.CanWrite;
                else if (this.realPropDesc != null)
                    return !(this.realPropDesc.IsReadOnly);

                return false;
            }
        }
        public override string Name
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.Name;
                else if (this.realPropDesc != null)
                    return this.realPropDesc.Name;

                return String.Empty;
            }
        }
        public override Type DeclaringType
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.DeclaringType;
                else if (this.realPropDesc != null)
                    return this.realPropDesc.ComponentType;

                return null;
            }
        }
        public override Type ReflectedType
        {
            get
            {
                if (this.realPropInfo != null)
                    return this.realPropInfo.ReflectedType;

                return null;
            }
        }

        #endregion

        #region MemberInfo Overrides

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetCustomAttributes(inherit);

            return new AttributeInfoAttribute[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.GetCustomAttributes(attributeType, inherit);

            return new AttributeInfoAttribute[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (this.realPropInfo != null)
                return this.realPropInfo.IsDefined(attributeType, inherit);

            return false;
        }

        #endregion
    }
    #endregion
}
