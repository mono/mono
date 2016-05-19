//---------------------------------------------------------------------
// <copyright file="EntityDataSourceStatementEditor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design.WebControls.Util;
using System.Diagnostics;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceStatementEditor : UITypeEditor
    {
        private bool EditQueryChangeCallback(object pair)
        {
            // pair.First is a wrapper that contains the EntityDataSource instance and property name being edited
            // pair.Second contains the value of the property being edited
            ITypeDescriptorContext context = (ITypeDescriptorContext)((Pair)pair).First;
            string value = (string)((Pair)pair).Second;

            EntityDataSource entityDataSource = (EntityDataSource)context.Instance;
            IServiceProvider serviceProvider = entityDataSource.Site;

            IDesignerHost designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
            Debug.Assert(designerHost != null, "Did not get DesignerHost service.");

            EntityDataSourceDesigner designer = (EntityDataSourceDesigner)designerHost.GetDesigner(entityDataSource);
            
            // Configure the dialog for the specified property and display it
            return Initialize(designer, entityDataSource, context.PropertyDescriptor.Name, serviceProvider, value);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ControlDesigner.InvokeTransactedChange(
                (IComponent)context.Instance,
                new TransactedChangeCallback(EditQueryChangeCallback),
                new Pair(context, value),
                Strings.ExpressionEditorTransactionDescription);

            return value;
        }

        // Determines if the specified property is one that has an associated "AutoGenerateXXXClause" property
        private static bool GetAutoGen(string operation, EntityDataSourceDesigner entityDataSourceDesigner)
        {
            if (String.Equals("Where", operation, StringComparison.Ordinal))
            {
                return entityDataSourceDesigner.AutoGenerateWhereClause;
            }
            else if (String.Equals("OrderBy", operation, StringComparison.Ordinal))
            {
                return entityDataSourceDesigner.AutoGenerateOrderByClause;
            }
            return false;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        // Gets the name of the AutoGenerateXXXProperty associated with the specified property name
        private static string GetOperationAutoGenerateProperty(string propertyName)
        {
            if (String.Equals("Where", propertyName, StringComparison.Ordinal))
            {
                return "AutoGenerateWhereClause";
            }
            else if (String.Equals("OrderBy", propertyName, StringComparison.Ordinal))
            {
                return "AutoGenerateOrderByClause";
            }
            return null;
        }

        // Gets the label text or accessible name to display over the textbox for editing the specified property name
        private static string GetStatementLabel(string propertyName, bool accessible)
        {
            switch (propertyName)
            {
                case "CommandText":
                    return accessible ? Strings.ExpressionEditor_CommandTextLabelAccessibleName :
                                        Strings.ExpressionEditor_CommandTextLabel;
                case "OrderBy":
                case "Select":
                case "Where":
                    return accessible ? Strings.ExpressionEditor_ExpressionStatementLabelAccessibleName(propertyName) :
                                        Strings.ExpressionEditor_ExpressionStatementLabel(propertyName);
                default:
                    Debug.Fail("Unknown property name in EntityDataSourceStatementEditor: " + propertyName);
                    return null;
            }            
        }


        // Gets the F1 help topic for each of the dialogs using the specified property name
        private static string GetHelpTopic(string propertyName)
        {
            switch (propertyName)
            {
                case "CommandText": return "net.Asp.EntityDataSource.CommandTextExpression";
                case "OrderBy": return "net.Asp.EntityDataSource.OrderByExpression";
                case "Select": return "net.Asp.EntityDataSource.SelectExpression";
                case "Where": return "net.Asp.EntityDataSource.WhereExpression";
                default:
                    Debug.Fail("Unknown property name in EntityDataSourceStatementEditor: " + propertyName);
                    return String.Empty;
            }
        }

        // Gets the property name for the parameters property associated with the specified property name
        private static string GetOperationParameterProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "CommandText":
                    return "CommandParameters";
                case "OrderBy":
                    return "OrderByParameters";
                case "Select":
                     return "SelectParameters";
                case "Where":
                    return "WhereParameters";
                default:
                    Debug.Fail("Unknown property name in EntityDataSourceStatementEditor: " + propertyName);
                    return null;
            }
        }

        // Gets a clone of the parameters collection associated with the specified property name
        private static ParameterCollection GetParameters(string propertyName, EntityDataSourceDesigner designer)
        {
            switch (propertyName)
            {
                case "CommandText":
                    return designer.CloneCommandParameters();
                case "OrderBy":
                    return designer.CloneOrderByParameters();
                case "Select":
                    return designer.CloneSelectParameters();
                case "Where":
                    return designer.CloneWhereParameters();
                default:
                    Debug.Fail("Unknown property name in EntityDataSourceStatementEditor: " + propertyName);
                    return null;
            }
        }

        // Updates the parameters collection associated with the specified property name
        private static void SetParameters(string propertyName, EntityDataSourceDesigner designer, ParameterCollection parameters)
        {
            switch (propertyName)
            {
                case "CommandText":
                    designer.SetCommandParameterContents(parameters);
                    break;
                case "OrderBy":
                    designer.SetOrderByParameterContents(parameters);
                    break;
                case "Select":
                    designer.SetSelectParameterContents(parameters);
                    break;
                case "Where":
                    designer.SetWhereParameterContents(parameters);
                    break;
                default:
                    Debug.Fail("Unknown property name in EntityDataSourceStatementEditor: " + propertyName);
                    break;

            }
        }

        // Configures and displays the editor dialog based on the specified property name
        private bool Initialize(EntityDataSourceDesigner designer, EntityDataSource entityDataSource, string propertyName, IServiceProvider serviceProvider, string statement)
        {
            string propertyParameters = GetOperationParameterProperty(propertyName);
            string autoGenProperty = GetOperationAutoGenerateProperty(propertyName);
            bool hasAutoGen = (autoGenProperty != null);
            bool autoGen = GetAutoGen(propertyName, designer);
            ParameterCollection parameters = GetParameters(propertyName, designer);
            string label = GetStatementLabel(propertyName, false);
            string accessibleName = GetStatementLabel(propertyName, true);
            string helpTopic = GetHelpTopic(propertyName);

            EntityDataSourceStatementEditorForm form = new EntityDataSourceStatementEditorForm(entityDataSource, serviceProvider,
                hasAutoGen, autoGen, propertyName, label, accessibleName, helpTopic, statement, parameters);

            DialogResult result = UIHelper.ShowDialog(serviceProvider, form);

            if (result == DialogResult.OK)
            {
                // We use the property descriptors to reset the values to
                // make sure we clear out any databindings or expressions that
                // may be set.
                PropertyDescriptor propDesc = null;

                if (autoGenProperty != null)
                {
                    propDesc = TypeDescriptor.GetProperties(entityDataSource)[autoGenProperty];
                    propDesc.ResetValue(entityDataSource);
                    propDesc.SetValue(entityDataSource, form.AutoGen);
                }

                if (propertyName != null)
                {
                    propDesc = TypeDescriptor.GetProperties(entityDataSource)[propertyName];
                    propDesc.ResetValue(entityDataSource);
                    propDesc.SetValue(entityDataSource, form.Statement);
                }

                if (propertyParameters != null)
                {
                    SetParameters(propertyName, designer, form.Parameters);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
