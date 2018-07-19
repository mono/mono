// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;

    internal class RegisterMetadataDelayedWorker
    {
        private Dictionary<string, List<Action<AttributeTableBuilder>>> delayedWorkItems;
        private AssemblyLoadEventHandler onAssemblyLoadedEventHandler;

        public Dictionary<string, List<Action<AttributeTableBuilder>>> DelayedWorkItems
        {
            get
            {
                if (this.delayedWorkItems == null)
                {
                    this.delayedWorkItems = new Dictionary<string, List<Action<AttributeTableBuilder>>>();
                }

                return this.delayedWorkItems;
            }
        }

        public void RegisterMetadataDelayed(string assemblyName, Action<AttributeTableBuilder> delayedWork)
        {
            Fx.Assert(assemblyName != null, "Checked by caller");
            Fx.Assert(delayedWork != null, "Checked by caller");
            if (this.onAssemblyLoadedEventHandler == null)
            {
                this.onAssemblyLoadedEventHandler = new AssemblyLoadEventHandler(this.OnAssemblyLoaded);
                AppDomain.CurrentDomain.AssemblyLoad += this.onAssemblyLoadedEventHandler;
            }

            List<Action<AttributeTableBuilder>> currentDelayedWorkItems;
            if (!this.DelayedWorkItems.TryGetValue(assemblyName, out currentDelayedWorkItems))
            {
                currentDelayedWorkItems = new List<Action<AttributeTableBuilder>>();
                this.DelayedWorkItems.Add(assemblyName, currentDelayedWorkItems);
            }

            currentDelayedWorkItems.Add(delayedWork);
        }

        public void WorkNowIfApplicable()
        {
            foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                this.CheckAndWork(loadedAssembly);
            }
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                this.CheckAndWork(args.LoadedAssembly);
            }
            catch
            {
                Fx.AssertAndFailFast("OnAssemblyLoad should not throw exception");
                throw;
            }
        }

        private void CheckAndWork(Assembly loadedAssembly)
        {
            List<Action<AttributeTableBuilder>> currentDelayedWorkItems;
            if (this.DelayedWorkItems.TryGetValue(loadedAssembly.GetName().Name, out currentDelayedWorkItems))
            {
                Action delayedRegisterMetadataWork = new DelayedRegisterMetadataWorkContext(currentDelayedWorkItems).Work;

                // Retrieve the top level type descriptor from the stack
                TypeDescriptionProvider currentTypeDescriptor = TypeDescriptor.GetProvider(typeof(object));

                // Intercept any existing changes.
                TypeDescriptor.AddProvider(new TypeDescriptionProviderInterceptor(currentTypeDescriptor, delayedRegisterMetadataWork), typeof(object));
            }
        }

        private class DelayedRegisterMetadataWorkContext
        {
            private List<Action<AttributeTableBuilder>> currentDelayedWorkItems;

            public DelayedRegisterMetadataWorkContext(List<Action<AttributeTableBuilder>> currentDelayedWorkItems)
            {
                this.currentDelayedWorkItems = currentDelayedWorkItems;
            }

            public void Work()
            {
                AttributeTableBuilder builder = new AttributeTableBuilder();
                foreach (Action<AttributeTableBuilder> delayedWork in this.currentDelayedWorkItems)
                {
                    delayedWork(builder);
                }

                MetadataStore.AddSystemAttributeTable(builder.CreateTable());
            }
        }

        private class TypeDescriptionProviderInterceptor : TypeDescriptionProvider
        {
            private Action interceptingWork;

            public TypeDescriptionProviderInterceptor(TypeDescriptionProvider parent, Action interceptingWork)
                : base(parent)
            {
                this.interceptingWork = interceptingWork;
            }

            public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
            {
                this.PerformInterceptingWork();
                return base.CreateInstance(provider, objectType, argTypes, args);
            }

            public override IDictionary GetCache(object instance)
            {
                this.PerformInterceptingWork();
                return base.GetCache(instance);
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                this.PerformInterceptingWork();
                return base.GetExtendedTypeDescriptor(instance);
            }

            public override string GetFullComponentName(object component)
            {
                this.PerformInterceptingWork();
                return base.GetFullComponentName(component);
            }

            public override Type GetReflectionType(Type objectType, object instance)
            {
                this.PerformInterceptingWork();
                return base.GetReflectionType(objectType, instance);
            }

            public override Type GetRuntimeType(Type reflectionType)
            {
                this.PerformInterceptingWork();
                return base.GetRuntimeType(reflectionType);
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                this.PerformInterceptingWork();
                return base.GetTypeDescriptor(objectType, instance);
            }

            public override bool IsSupportedType(Type type)
            {
                this.PerformInterceptingWork();
                return base.IsSupportedType(type);
            }

            private void PerformInterceptingWork()
            {
                // Make sure the intercepting work is done only once.
                TypeDescriptor.RemoveProvider(this, typeof(object));
                if (this.interceptingWork != null)
                {
                    this.interceptingWork();
                    this.interceptingWork = null;
                }
            }
        }
    }
}
