//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime;
    using System.Windows.Threading;
    
    partial class InteropDesigner
    {
        private static Func<Type, bool> filter; 
        private static string interopTypeAssemblyQualifiedName = null;

        public InteropDesigner()
        {
            this.InitializeComponent();
        }

        public static Func<Type, bool> Filter
        {
            get 
            {
                if (InteropDesigner.filter == null)
                {
                    // We will build type name for System.Workflow.ComponentModel.Activity
                    string typeName = typeof(Activity).AssemblyQualifiedName;
                    typeName = typeName.Replace("System.Activities", "System.Workflow.ComponentModel");

                    Type activityType = GetTypeByQualifiedName(typeName);
                    if (activityType != null)
                    {
                        //Interop.Body has to be a 3.5 Activity
                        InteropDesigner.filter = (type) => activityType.IsAssignableFrom(type);
                    }
                }

                return InteropDesigner.filter; 
            }
        }

        public static string InteropTypeAssemblyQualifiedName
        {
            get
            {
                if (interopTypeAssemblyQualifiedName == null)
                {
                    // Construct the type name dynamically to avoid hardcoding the version number and public key token.
                    // The constructed type name should look like: "System.Activities.Statements.Interop, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
                    interopTypeAssemblyQualifiedName = typeof(Parallel).AssemblyQualifiedName.Replace("Parallel", "Interop").Replace("System.Activities,", "System.Workflow.Runtime,");
                }

                return interopTypeAssemblyQualifiedName;
            }
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            this.ModelItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnModelItemPropertyChanged);
        }

        private void OnModelItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActivityType")
            {
                //Whenever ActivityType property changes, the activity will generate a new set of 
                // dynamic properties. the property browser will not pick up the changes till
                // we select some other modelitem and then select this back.
                // modelItem.root is theone that will be always available.

                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)(() =>
                {
                    Selection.SelectOnly(this.Context, this.ModelItem.Root);
                }));
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)(() =>
                {
                    Selection.SelectOnly(this.Context, this.ModelItem);
                }));
               
            }
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type activityType = GetTypeByQualifiedName(InteropDesigner.InteropTypeAssemblyQualifiedName);

            //activityType will be null in ClientSKU since System.Workflow.Runtime.dll is not a part of clientSKU.
            if (activityType != null)
            {
                builder.AddCustomAttributes(activityType, new DesignerAttribute(typeof(InteropDesigner)));
                builder.AddCustomAttributes(
                            activityType,
                            "ActivityType",
                            new EditorOptionAttribute { Name = TypePropertyEditor.BrowseTypeDirectly, Value = true });
                builder.AddCustomAttributes(
                            activityType,
                            "ActivityType",
                            new EditorOptionAttribute { Name = TypePropertyEditor.Filter, Value = Filter });
                builder.AddCustomAttributes(
                            activityType,
                            "ActivityType",
                            new RefreshPropertiesAttribute(RefreshProperties.All));
                builder.AddCustomAttributes(activityType, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
            }
        }

        // Gets a System.Type object using the type's assembly qualified name.
        // Non-fatal exceptions are ----ed.
        private static Type GetTypeByQualifiedName(string assemblyQualifiedName)
        {
            Fx.Assert(assemblyQualifiedName != null, "assemblyQualifiedName cannot be null.");

            try
            {
                Type type = Type.GetType(assemblyQualifiedName);
                return type;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
            }

            return null;
        }
    }
}
