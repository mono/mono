namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.ComponentModel.Compiler;

    #region Class ComponentDispenser
    internal static class ComponentDispenser
    {
        private static IDictionary<Type, List<IExtenderProvider>> componentExtenderMap = new Dictionary<Type, List<IExtenderProvider>>();

        //it is super critical to note that even though we pass activity instead of a System.Type
        //here, the method impl does not rely on any objectness but relies only on typeness
        //- this is done to keep a static type level executor metadata (and not instance level)
        // scoped in an app domain
        internal static ActivityExecutor[] CreateActivityExecutors(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            List<ActivityExecutor> executors = new List<ActivityExecutor>();

            if (activity.SupportsSynchronization)
                executors.Add(new SynchronizationFilter());
            if (activity.SupportsTransaction)
                executors.Add(new TransactedContextFilter());
            if (activity is CompositeActivity)
            {
                if (activity is ICompensatableActivity)
                    executors.Add(new CompensationHandlingFilter());

                executors.Add(new FaultAndCancellationHandlingFilter());
                executors.Add(new CompositeActivityExecutor<CompositeActivity>());
            }
            else
                executors.Add(new ActivityExecutor<Activity>());

            return executors.ToArray();
        }

        internal static object[] CreateComponents(Type objectType, Type componentTypeAttribute)
        {
            //*******DO NOT CHANGE THE ORDER OF THE EXECUTION AS IT HAS SIGNIFICANCE AT RUNTIME
            Dictionary<Type, object> components = new Dictionary<Type, object>();

            // Goto all the attributes and collect matching attributes and component factories
            ArrayList supportsTransactionComponents = new ArrayList();
            ArrayList supportsCancelHandlerComponents = new ArrayList();
            ArrayList supportsSynchronizationComponents = new ArrayList();
            ArrayList supportsCompensationHandlerComponents = new ArrayList();

            // dummy calls to make sure that attributes are in good shape
            GetCustomAttributes(objectType, typeof(ActivityCodeGeneratorAttribute), true);
            GetCustomAttributes(objectType, typeof(ActivityValidatorAttribute), true);
            GetCustomAttributes(objectType, typeof(System.ComponentModel.DesignerAttribute), true);
            GetCustomAttributes(objectType, typeof(System.ComponentModel.Design.Serialization.DesignerSerializerAttribute), true);

            if (objectType.GetCustomAttributes(typeof(SupportsTransactionAttribute), true).Length > 0)
            {
                if (componentTypeAttribute == typeof(ActivityValidatorAttribute))
                    supportsTransactionComponents.Add(new TransactionContextValidator());
            }

            if (objectType.GetCustomAttributes(typeof(SupportsSynchronizationAttribute), true).Length > 0)
            {
                if (componentTypeAttribute == typeof(ActivityValidatorAttribute))
                    supportsSynchronizationComponents.Add(new SynchronizationValidator());
            }

            // IMPORTANT: sequence of these components is really critical
            AddComponents(components, supportsSynchronizationComponents.ToArray());
            AddComponents(components, supportsTransactionComponents.ToArray());
            AddComponents(components, supportsCompensationHandlerComponents.ToArray());
            AddComponents(components, supportsCancelHandlerComponents.ToArray());

            //Goto all the interfaces and collect matching attributes and component factories
            ArrayList customAttributes = new ArrayList();
            foreach (Type interfaceType in objectType.GetInterfaces())
                customAttributes.AddRange(ComponentDispenser.GetCustomAttributes(interfaceType, componentTypeAttribute, true));

            //Add all the component's attributes
            customAttributes.AddRange(ComponentDispenser.GetCustomAttributes(objectType, componentTypeAttribute, true));

            string typeName = null;
            foreach (Attribute attribute in customAttributes)
            {
                Type expectedBaseType = null;
                if (componentTypeAttribute == typeof(ActivityCodeGeneratorAttribute))
                {
                    typeName = ((ActivityCodeGeneratorAttribute)attribute).CodeGeneratorTypeName;
                    expectedBaseType = typeof(ActivityCodeGenerator);
                }
                else if (componentTypeAttribute == typeof(ActivityValidatorAttribute))
                {
                    typeName = ((ActivityValidatorAttribute)attribute).ValidatorTypeName;
                    expectedBaseType = typeof(Validator);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "wrong attribute type");
                }

                object component = null;
                try
                {
                    if (!String.IsNullOrEmpty(typeName))
                        component = ComponentDispenser.CreateComponentInstance(typeName, objectType);
                }
                catch
                {
                }

                if ((component != null && expectedBaseType != null && expectedBaseType.IsAssignableFrom(component.GetType())))
                {
                    if (!components.ContainsKey(component.GetType()))
                        components.Add(component.GetType(), component);
                }
                else
                {
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidAttribute, componentTypeAttribute.Name, objectType.FullName));
                }
            }
            return new ArrayList(components.Values).ToArray();
        }

        private static void AddComponents(Dictionary<Type, object> components, object[] attribComponents)
        {
            foreach (object component in attribComponents)
            {
                if (!components.ContainsKey(component.GetType()))
                    components.Add(component.GetType(), component);
            }
        }
        internal static void RegisterComponentExtenders(Type extendingType, IExtenderProvider[] extenders)
        {
            //Make sure that there are no previous registered components
            List<IExtenderProvider> extenderProviders = null;
            if (!componentExtenderMap.ContainsKey(extendingType))
            {
                extenderProviders = new List<IExtenderProvider>();
                componentExtenderMap.Add(extendingType, extenderProviders);
            }
            else
            {
                extenderProviders = componentExtenderMap[extendingType];
            }

            extenderProviders.AddRange(extenders);
        }

        internal static IList<IExtenderProvider> Extenders
        {
            get
            {
                List<IExtenderProvider> extenders = new List<IExtenderProvider>();
                foreach (IList<IExtenderProvider> registeredExtenders in componentExtenderMap.Values)
                    extenders.AddRange(registeredExtenders);
                return extenders.AsReadOnly();
            }
        }

        // The referenceType parameter provides a way to identify the assembly in which to look for the type.
        private static object CreateComponentInstance(string typeName, Type referenceType)
        {
            object component = null;

            Type componentType = null;
            try
            {
                string typeFullName = typeName;
                int squareBracketCloseIndex = typeName.LastIndexOf(']');
                if (squareBracketCloseIndex != -1)
                {
                    typeFullName = typeName.Substring(0, squareBracketCloseIndex + 1);
                }
                else
                {
                    int commaIndex = typeName.IndexOf(',');
                    if (commaIndex != -1)
                        typeFullName = typeName.Substring(0, commaIndex);
                }
                componentType = referenceType.Assembly.GetType(typeFullName, false);
            }
            catch
            {
            }

            if (componentType == null)
            {
                try
                {
                    componentType = Type.GetType(typeName, false);
                }
                catch
                {
                }
            }

            string message = null;
            if (componentType != null)
            {
                try
                {
                    component = Activator.CreateInstance(componentType);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    // Process error condition below
                }
            }

            if (component == null)
            {
                System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("System.Workflow.ComponentModel.StringResources", typeof(System.Workflow.ComponentModel.Activity).Assembly);
                if (resourceManager != null)
                    message = string.Format(CultureInfo.CurrentCulture, resourceManager.GetString("Error_CantCreateInstanceOfComponent"), new object[] { typeName, message });
                throw new Exception(message);
            }
            return component;
        }

        private static object[] GetCustomAttributes(Type objectType, Type attributeType, bool inherit)
        {
            object[] attribs = null;
            try
            {
                if (attributeType == null)
                    attribs = objectType.GetCustomAttributes(inherit);
                else
                    attribs = objectType.GetCustomAttributes(attributeType, inherit);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidAttributes, objectType.FullName), e);
            }

            return attribs;
        }
    }
    #endregion
}
