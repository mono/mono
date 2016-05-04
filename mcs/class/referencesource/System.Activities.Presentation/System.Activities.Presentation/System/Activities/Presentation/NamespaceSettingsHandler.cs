//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Debugger;
    using System.Activities.Expressions;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.ServiceModel.Activities;
    using Microsoft.Activities.Presentation;
    using Microsoft.VisualBasic.Activities;

    //the class does several things:
    //1. make sure a special property "Imports" (implemented using VisualBasicSettings attached properties) is added to the root object before it's loaded into ModelTree
    //2. make sure the "root workflow" of the root object always have the same VisualBasicSettings
    static class NamespaceSettingsHandler
    {
        static Type WorkflowServiceType = typeof(WorkflowService);

        static public void PreviewLoadRoot(object sender, WorkflowDesigner.PreviewLoadEventArgs args)
        {
            object root = args.Instance;

            DesignerConfigurationService configService = args.Context.Services.GetService<DesignerConfigurationService>();

            if (configService != null && configService.NamespaceConversionEnabled)
            {
                ConvertNamespaces(root, args.Context);
            }

            if (root.GetType() == WorkflowServiceType)
            {
                args.Context.Services.Subscribe<ModelTreeManager>(manager => manager.Root.PropertyChanged += new PropertyChangedEventHandler(OnRootPropertyChanged));
            }

            TypeDescriptor.AddProvider(new RootModelTypeDescriptionProvider(root), root);
        }

        private static void ConvertNamespaces(object root, EditingContext context)
        {
            VisualBasicSettings settings = VisualBasic.GetSettings(root);
            IList<AssemblyReference> references;
            IList<string> importedNamespaces = NamespaceHelper.GetTextExpressionNamespaces(root, out references);
            FrameworkName targetFramework = WorkflowDesigner.GetTargetFramework(context);
            if (targetFramework.IsLessThan45())
            {
                if (settings == null)
                {
                    if ((importedNamespaces != null) && (importedNamespaces.Count > 0))
                    {
                        NamespaceHelper.ConvertToVBSettings(
                            importedNamespaces,
                            references,
                            context,
                            out settings);
                    }
                    else
                    {
                        settings = new VisualBasicSettings();
                    }

                    NamespaceHelper.SetVisualBasicSettings(root, settings);
                    NamespaceHelper.SetTextExpressionNamespaces(root, null, null);
                }

                IDebuggableWorkflowTree debuggableWorkflowTree = root as IDebuggableWorkflowTree;
                if (debuggableWorkflowTree != null)
                {
                    Activity rootActivity = debuggableWorkflowTree.GetWorkflowRoot();
                    if (rootActivity != null)
                    {
                        NamespaceHelper.SetVisualBasicSettings(rootActivity, settings);
                        NamespaceHelper.SetTextExpressionNamespaces(rootActivity, null, null);
                    }                 
                }
            }
            else
            {
                if ((importedNamespaces == null) || (importedNamespaces.Count == 0))
                {
                    if (settings != null)
                    {
                        NamespaceHelper.ConvertToTextExpressionImports(settings, out importedNamespaces, out references);
                        NamespaceHelper.SetTextExpressionNamespaces(root, importedNamespaces, references);
                        NamespaceHelper.SetVisualBasicSettings(root, null);
                    }
                    else
                    {
                        NamespaceHelper.SetTextExpressionNamespaces(root, new Collection<string>(), new Collection<AssemblyReference>());
                    }
                }
            }
        }

        static void OnRootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ModelItem rootModel = sender as ModelItem;
            Fx.Assert(rootModel != null, "sender item could not be null");
            ModelProperty changedProperty = rootModel.Properties[e.PropertyName];
            if (changedProperty == null)
            {
                return;
            }

            object changedPropertyValue = changedProperty.ComputedValue;
            if (changedPropertyValue == null)
            {
                return;
            }

            Fx.Assert(rootModel.GetCurrentValue().GetType() == WorkflowServiceType, "This handler should only be attached when the root is WorkflowService");
            IDebuggableWorkflowTree root = rootModel.GetCurrentValue() as IDebuggableWorkflowTree;            
            Activity rootActivity = root.GetWorkflowRoot();
            if (rootActivity == changedPropertyValue)
            {
                if (WorkflowDesigner.GetTargetFramework(rootModel.GetEditingContext()).IsLessThan45())
                {
                    VisualBasicSettings settings = VisualBasic.GetSettings(root);
                    NamespaceHelper.SetVisualBasicSettings(changedPropertyValue, settings);
                }
                else
                {
                    IList<AssemblyReference> referencedAssemblies;
                    IList<string> namespaces = NamespaceHelper.GetTextExpressionNamespaces(root, out referencedAssemblies);
                    NamespaceHelper.SetTextExpressionNamespaces(rootActivity, namespaces, referencedAssemblies);
                }
            }
        }
    }

    class RootModelTypeDescriptionProvider : TypeDescriptionProvider
    {
        public RootModelTypeDescriptionProvider(object instance)
            : base(TypeDescriptor.GetProvider(instance))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor defaultDescriptor = base.GetTypeDescriptor(objectType, instance);
            return new RootModelTypeDescriptor(defaultDescriptor, instance);
        }
    }

    class RootModelTypeDescriptor : CustomTypeDescriptor
    {
        object root;
        NamespaceListPropertyDescriptor importDescriptor;

        public RootModelTypeDescriptor(ICustomTypeDescriptor parent, object root)
            : base(parent)
        {
            this.root = root;
        }

        PropertyDescriptor ImportDescriptor
        {
            get
            {
                if (this.importDescriptor == null)
                {
                    this.importDescriptor = new NamespaceListPropertyDescriptor(this.root);
                }

                return this.importDescriptor;
            }
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(base.GetProperties(attributes).Cast<PropertyDescriptor>()
                .Union(new PropertyDescriptor[] { this.ImportDescriptor }).ToArray());
        }
    }        
}
