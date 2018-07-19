#region Imports

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Text;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Configuration;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Tracking;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Security.Cryptography;
using System.Workflow.ComponentModel.Design;

#endregion


namespace System.Workflow.Runtime
{
    internal sealed class WorkflowDefinitionDispenser : IDisposable
    {
        private MruCache workflowTypes;
        private MruCache xomlFragments;
        private Dictionary<Type, List<PropertyInfo>> workflowOutParameters;
        private WorkflowRuntime workflowRuntime;
        private bool validateOnCreate = true;
        internal static DependencyProperty WorkflowDefinitionHashCodeProperty = DependencyProperty.RegisterAttached("WorkflowDefinitionHashCode", typeof(byte[]), typeof(WorkflowDefinitionDispenser));
        internal event EventHandler<WorkflowDefinitionEventArgs> WorkflowDefinitionLoaded;

        private ReaderWriterLock parametersLock;

        internal WorkflowDefinitionDispenser(WorkflowRuntime runtime, bool validateOnCreate, int capacity)
        {
            if (capacity <= 0)
            {
                capacity = 2000;
            }
            this.workflowRuntime = runtime;
            this.workflowTypes = new MruCache(capacity, this, CacheType.Type);
            this.xomlFragments = new MruCache(capacity, this, CacheType.Xoml);
            this.workflowOutParameters = new Dictionary<Type, List<PropertyInfo>>();
            this.parametersLock = new ReaderWriterLock();
            this.validateOnCreate = validateOnCreate;
        }

        internal ReadOnlyCollection<PropertyInfo> GetOutputParameters(Activity rootActivity)
        {
            Type workflowType = rootActivity.GetType();
            this.parametersLock.AcquireReaderLock(-1);
            try
            {
                if (this.workflowOutParameters.ContainsKey(workflowType))
                    return new ReadOnlyCollection<PropertyInfo>(this.workflowOutParameters[workflowType]);
            }
            finally
            {
                this.parametersLock.ReleaseLock();
            }

            // We will recurse at most once because CacheOutputParameters() will perform negative caching.
            CacheOutputParameters(rootActivity);
            return GetOutputParameters(rootActivity);
        }

        internal void GetWorkflowTypes(out ReadOnlyCollection<Type> keys, out ReadOnlyCollection<Activity> values)
        {
            this.workflowTypes.GetWorkflowDefinitions<Type>(out keys, out values);
        }

        internal void GetWorkflowDefinitions(out ReadOnlyCollection<byte[]> keys, out ReadOnlyCollection<Activity> values)
        {
            this.xomlFragments.GetWorkflowDefinitions<byte[]>(out keys, out values);
        }

        internal Activity GetWorkflowDefinition(byte[] xomlHashCode)
        {
            Activity workflowDefinition = null;
            if (xomlHashCode == null)
                throw new ArgumentNullException("xomlHashCode");

            workflowDefinition = xomlFragments.GetDefinition(xomlHashCode);

            if (workflowDefinition == null)
                throw new ArgumentException("xomlHashCode");

            return workflowDefinition;
        }

        internal Activity GetWorkflowDefinition(Type workflowType)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");

            return this.GetRootActivity(workflowType, false, true);
        }

        internal Activity GetRootActivity(Type workflowType, bool createNew, bool initForRuntime)
        {
            Activity root = null;

            if (createNew)
                return LoadRootActivity(workflowType, false, initForRuntime);
            bool exist;
            root = workflowTypes.GetOrGenerateDefinition(workflowType, null, null, null, initForRuntime, out exist);
            if (exist)
            {
                return root;
            }
            // Set the locking object used for cloning the definition
            // for non-internal use (WorkflowInstance.GetWorkflowDefinition
            // and WorkflowCompletedEventArgs.WorkflowDefinition)
            WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(root, new object());

            EventHandler<WorkflowDefinitionEventArgs> localWorkflowDefinitionLoaded = WorkflowDefinitionLoaded;
            if (localWorkflowDefinitionLoaded != null)
                localWorkflowDefinitionLoaded(this.workflowRuntime, new WorkflowDefinitionEventArgs(workflowType));

            return root;
        }

        // This function will create a new root activity definition tree by deserializing the xoml and the rules file.
        // The last parameter createNew should be true when the caller is asking for a new definition for performing 
        // dynamic updates instead of a cached definition.
        internal Activity GetRootActivity(string xomlText, string rulesText, bool createNew, bool initForRuntime)
        {
            if (string.IsNullOrEmpty(xomlText))
                throw new ArgumentNullException("xomlText");

            //calculate the "hash". Think 60s!
            byte[] xomlHashCode = null;
            MemoryStream xomlBytesStream = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(xomlBytesStream))
            {
                streamWriter.Write(xomlText);

                //consider rules, if they exist
                if (!string.IsNullOrEmpty(rulesText))
                    streamWriter.Write(rulesText);

                streamWriter.Flush();
                xomlBytesStream.Position = 0;

                xomlHashCode = MD5HashHelper.ComputeHash(xomlBytesStream.GetBuffer());
            }

            if (createNew)
                return LoadRootActivity(xomlText, rulesText, xomlHashCode, false, initForRuntime);

            bool exist;
            Activity root = xomlFragments.GetOrGenerateDefinition(null, xomlText, rulesText, xomlHashCode, initForRuntime, out exist);
            if (exist)
            {
                return root;
            }
            // Set the locking object used for cloning the definition
            // for non-internal use (WorkflowInstance.GetWorkflowDefinition
            // and WorkflowCompletedEventArgs.WorkflowDefinition)
            WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(root, new object());

            EventHandler<WorkflowDefinitionEventArgs> localWorkflowDefinitionLoaded = WorkflowDefinitionLoaded;
            if (localWorkflowDefinitionLoaded != null)
                localWorkflowDefinitionLoaded(this.workflowRuntime, new WorkflowDefinitionEventArgs(xomlHashCode));

            return root;
        }

        internal void ValidateDefinition(Activity root, bool isNewType, ITypeProvider typeProvider)
        {
            if (!this.validateOnCreate)
                return;

            ValidationErrorCollection errors = new ValidationErrorCollection();

            // For validation purposes, create a type provider in the type case if the 
            // host did not push one.
            if (typeProvider == null)
                typeProvider = WorkflowRuntime.CreateTypeProvider(root);

            // Validate that we are purely XAML.
            if (!isNewType)
            {
                if (!string.IsNullOrEmpty(root.GetValue(WorkflowMarkupSerializer.XClassProperty) as string))
                    errors.Add(new ValidationError(ExecutionStringManager.XomlWorkflowHasClassName, ErrorNumbers.Error_XomlWorkflowHasClassName));

                Queue compositeActivities = new Queue();
                compositeActivities.Enqueue(root);
                while (compositeActivities.Count > 0)
                {
                    Activity activity = compositeActivities.Dequeue() as Activity;

                    if (activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) != null)
                        errors.Add(new ValidationError(ExecutionStringManager.XomlWorkflowHasCode, ErrorNumbers.Error_XomlWorkflowHasCode));

                    CompositeActivity compositeActivity = activity as CompositeActivity;
                    if (compositeActivity != null)
                    {
                        foreach (Activity childActivity in compositeActivity.EnabledActivities)
                            compositeActivities.Enqueue(childActivity);
                    }
                }
            }

            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ITypeProvider), typeProvider);

            ValidationManager validationManager = new ValidationManager(serviceContainer);
            using (WorkflowCompilationContext.CreateScope(validationManager))
            {
                foreach (Validator validator in validationManager.GetValidators(root.GetType()))
                {
                    foreach (ValidationError error in validator.Validate(validationManager, root))
                    {
                        if (!error.UserData.Contains(typeof(Activity)))
                            error.UserData[typeof(Activity)] = root;

                        errors.Add(error);
                    }
                }
            }
            if (errors.HasErrors)
                throw new WorkflowValidationFailedException(ExecutionStringManager.WorkflowValidationFailure, errors);
        }

        public void Dispose()
        {
            xomlFragments.Dispose();
            workflowTypes.Dispose();
        }

        private Activity LoadRootActivity(Type workflowType, bool createDefinition, bool initForRuntime)
        {
            WorkflowLoaderService loader = workflowRuntime.GetService<WorkflowLoaderService>();
            Activity root = loader.CreateInstance(workflowType);
            if (root == null)
                throw new InvalidOperationException(ExecutionStringManager.CannotCreateRootActivity);
            if (root.GetType() != workflowType)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowTypeMismatch, workflowType.FullName));

            if (createDefinition)
                ValidateDefinition(root, true, workflowRuntime.GetService<ITypeProvider>());

            if (initForRuntime)
                ((IDependencyObjectAccessor)root).InitializeDefinitionForRuntime(null);

            root.SetValue(Activity.WorkflowRuntimeProperty, workflowRuntime);

            return root;
        }

        private Activity LoadRootActivity(string xomlText, string rulesText, byte[] xomlHashCode, bool createDefinition, bool initForRuntime)
        {
            Activity root = null;
            WorkflowLoaderService loader = workflowRuntime.GetService<WorkflowLoaderService>();

            using (StringReader xomlTextReader = new StringReader(xomlText))
            {
                using (XmlReader xomlReader = XmlReader.Create(xomlTextReader))
                {
                    XmlReader rulesReader = null;
                    StringReader rulesTextReader = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(rulesText))
                        {
                            rulesTextReader = new StringReader(rulesText);
                            rulesReader = XmlReader.Create(rulesTextReader);
                        }
                        root = loader.CreateInstance(xomlReader, rulesReader);
                    }
                    finally
                    {
                        if (rulesReader != null)
                            rulesReader.Close();
                        if (rulesTextReader != null)
                            rulesTextReader.Close();
                    }
                }
            }

            if (root == null)
                throw new InvalidOperationException(ExecutionStringManager.CannotCreateRootActivity);

            if (createDefinition)
            {
                ITypeProvider typeProvider = workflowRuntime.GetService<ITypeProvider>();
                ValidateDefinition(root, false, typeProvider);
            }

            if (initForRuntime)
                ((IDependencyObjectAccessor)root).InitializeDefinitionForRuntime(null);

            // Save the original markup.
            root.SetValue(Activity.WorkflowXamlMarkupProperty, xomlText);
            root.SetValue(Activity.WorkflowRulesMarkupProperty, rulesText);
            root.SetValue(WorkflowDefinitionHashCodeProperty, xomlHashCode);
            root.SetValue(Activity.WorkflowRuntimeProperty, workflowRuntime);

            return root;
        }

        private void CacheOutputParameters(Activity rootActivity)
        {
            Type workflowType = rootActivity.GetType();
            List<PropertyInfo> outputParameters = null;

            this.parametersLock.AcquireWriterLock(-1);

            try
            {
                if (this.workflowOutParameters.ContainsKey(workflowType))
                    return;

                // Cache negative and positive cases!
                outputParameters = new List<PropertyInfo>();
                this.workflowOutParameters.Add(workflowType, outputParameters);

                PropertyInfo[] properties = workflowType.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (!property.CanRead || property.DeclaringType == typeof(DependencyObject) || property.DeclaringType == typeof(Activity) || property.DeclaringType == typeof(CompositeActivity))
                        continue;

                    bool ignoreProperty = false;
                    foreach (DependencyProperty dependencyProperty in rootActivity.MetaDependencyProperties)
                    {
                        if (dependencyProperty.Name == property.Name && dependencyProperty.DefaultMetadata.IsMetaProperty)
                        {
                            ignoreProperty = true;
                            break;
                        }
                    }

                    if (!ignoreProperty)
                        outputParameters.Add(property);
                }
            }
            finally
            {
                Thread.MemoryBarrier();
                this.parametersLock.ReleaseLock();
            }
        }

        private enum CacheType
        {
            Type = 0,
            Xoml = 1,
        }

        private class MruCache : IDisposable
        {
            Hashtable hashtable;
            LinkedList<Activity> mruList;
            int size;
            int capacity;
            WorkflowDefinitionDispenser dispenser;
            CacheType type;

            internal MruCache(int capacity, WorkflowDefinitionDispenser dispenser, CacheType type)
            {
                if (type == CacheType.Xoml)
                {
                    this.hashtable = new Hashtable((IEqualityComparer)new DigestComparerWrapper());
                }
                else
                {
                    this.hashtable = new Hashtable();
                }
                this.mruList = new LinkedList<Activity>();
                this.capacity = capacity;
                this.dispenser = dispenser;
                this.type = type;
            }

            private void RemoveFromDictionary(Activity activity)
            {
                byte[] key = activity.GetValue(WorkflowDefinitionHashCodeProperty) as byte[];
                if (key != null)
                {
                    this.hashtable.Remove(key);
                }
                else
                {
                    Type type = activity.GetType();
                    this.hashtable.Remove(type);
                }
            }

            private void AddToDictionary(LinkedListNode<Activity> node)
            {
                byte[] key = node.Value.GetValue(WorkflowDefinitionHashCodeProperty) as byte[];
                if (key != null)
                {
                    this.hashtable.Add(key, node);
                }
                else
                {
                    Type type = node.Value.GetType();
                    this.hashtable.Add(type, node);
                }
            }

            internal Activity GetDefinition(byte[] md5Codes)
            {
                LinkedListNode<Activity> node;
                node = this.hashtable[md5Codes] as LinkedListNode<Activity>;
                if (node != null)
                {
                    return node.Value;
                }
                else
                {
                    return null;
                }
            }

            internal Activity GetOrGenerateDefinition(Type type, string xomlText, string rulesText, byte[] md5Codes, bool initForRuntime, out bool exist)
            {
                LinkedListNode<Activity> node;
                object key;

                if (type != null)
                {
                    key = type;
                }
                else
                {
                    key = md5Codes;
                }
                try
                {
                    exist = false;
                    node = this.hashtable[key] as LinkedListNode<Activity>;

                    if (node != null)
                    {
                        lock (this.mruList)
                        {
                            node = this.hashtable[key] as LinkedListNode<Activity>;
                            if (node != null)
                            {
                                exist = true;
                                this.mruList.Remove(node);
                                this.mruList.AddFirst(node);
                            }
                            else
                            {
                                exist = false;
                            }
                        }
                    }

                    if (!exist)
                    {
                        lock (this.hashtable)
                        {
                            node = this.hashtable[key] as LinkedListNode<Activity>;
                            if (node != null)
                            {
                                exist = true;
                                lock (this.mruList)
                                {
                                    this.mruList.Remove(node);
                                    this.mruList.AddFirst(node);
                                }
                            }
                            else
                            {
                                exist = false;
                                Activity activity;
                                if (type != null)
                                {
                                    activity = this.dispenser.LoadRootActivity(type, true, initForRuntime);
                                }
                                else
                                {
                                    activity = this.dispenser.LoadRootActivity(xomlText, rulesText, key as byte[], true, initForRuntime);
                                }
                                lock (this.mruList)
                                {
                                    if (this.size < this.capacity)
                                    {
                                        this.size++;
                                    }
                                    else
                                    {
                                        RemoveFromDictionary(this.mruList.Last.Value);
                                        this.mruList.RemoveLast();
                                    }
                                    node = new LinkedListNode<Activity>(activity);
                                    AddToDictionary(node);
                                    this.mruList.AddFirst(node);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Thread.MemoryBarrier();
                }
                return node.Value;
            }

            internal void GetWorkflowDefinitions<K>(out ReadOnlyCollection<K> keys, out ReadOnlyCollection<Activity> values)
            {
                lock (this.hashtable)
                {
                    if (((typeof(K) == typeof(Type)) && (this.type == CacheType.Type)) || ((typeof(K) == typeof(byte[])) && (this.type == CacheType.Xoml)))
                    {
                        List<K> keyList = new List<K>();
                        foreach (K key in this.hashtable.Keys)
                        {
                            keyList.Add(key);
                        }
                        keys = new ReadOnlyCollection<K>(keyList);
                        List<Activity> list = new List<Activity>();
                        foreach (LinkedListNode<Activity> node in this.hashtable.Values)
                        {
                            list.Add(node.Value);
                        }
                        values = new ReadOnlyCollection<Activity>(list);
                    }
                    else
                    {
                        keys = null;
                        values = null;
                    }
                }
            }

            public void Dispose()
            {
                foreach (LinkedListNode<Activity> node in hashtable.Values)
                {
                    try
                    {
                        node.Value.Dispose();
                    }
                    catch (Exception)//ignore any dispose exception.
                    {
                    }
                }
            }
        }

        private class DigestComparerWrapper : IEqualityComparer
        {
            IEqualityComparer<byte[]> comparer = (IEqualityComparer<byte[]>)new DigestComparer();
            bool IEqualityComparer.Equals(object object1, object object2)
            {
                return comparer.Equals((byte[])object1, (byte[])object2);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return comparer.GetHashCode((byte[])obj);
            }
        }

    }

    internal class WorkflowDefinitionLock : IDisposable
    {
        internal static readonly DependencyProperty WorkflowDefinitionLockObjectProperty = DependencyProperty.RegisterAttached("WorkflowDefinitionLockObject", typeof(object), typeof(WorkflowDefinitionLock), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        internal static object GetWorkflowDefinitionLockObject(DependencyObject dependencyObject)
        {
            // The Dependency Properties are kept in a Dictionary<>, which is not thread safe between
            // "getters" and "setters", so lock around the "getter", too.
            lock (dependencyObject)
            {
                return dependencyObject.GetValue(WorkflowDefinitionLockObjectProperty);
            }
        }

        internal static void SetWorkflowDefinitionLockObject(DependencyObject dependencyObject, object value)
        {
            lock (dependencyObject)
            {
                if (dependencyObject.GetValue(WorkflowDefinitionLockObjectProperty) == null)
                {
                    dependencyObject.SetValue(WorkflowDefinitionLockObjectProperty, value);
                }
            }
        }

        private object _syncObj;

        public WorkflowDefinitionLock(Activity definition)
        {
            this._syncObj = GetWorkflowDefinitionLockObject(definition);

            Debug.Assert(this._syncObj != null, "Definition's synchronization object was null.  This should always be set.");

#pragma warning disable 0618
            //@
            Monitor.Enter(this._syncObj);
#pragma warning restore 0618
        }

        #region IDisposable Members

        public void Dispose()
        {
            Monitor.Exit(this._syncObj);
        }

        #endregion
    }
}
