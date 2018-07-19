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

    #region Class SerializableTypeCodeDomSerializer
    internal sealed class SerializableTypeCodeDomSerializer : CodeDomSerializer
    {
        private CodeDomSerializer originalSerializer;

        public SerializableTypeCodeDomSerializer(CodeDomSerializer originalSerializer)
        {
            if (originalSerializer == null)
                throw new ArgumentNullException("originalSerializer");

            this.originalSerializer = originalSerializer;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            object result = null;
            if (value == null)
                return result;

            CodeStatementCollection statements = null;
            ExpressionContext cxt = manager.Context[typeof(ExpressionContext)] as ExpressionContext;

            if (value.GetType().GetConstructor(new Type[0]) != null)
            {
                if (value is ICollection)
                {
                    ExpressionContext varExct = null;
                    if (cxt != null)
                    {
                        if (cxt.PresetValue != value)
                        {
                            try
                            {
                                statements = new CodeStatementCollection();
                                CodeVariableReferenceExpression varExpression = AddVariableExpression(manager, statements, value);
                                varExct = new ExpressionContext(varExpression, value.GetType(), cxt.Owner, value);
                                manager.Context.Push(varExct);
                                result = this.originalSerializer.Serialize(manager, value);
                                if (result is CodeStatementCollection)
                                    statements.AddRange(result as CodeStatementCollection);
                                else if (result is CodeStatement)
                                    statements.Add(result as CodeStatement);
                                else if (result is CodeExpression)
                                    // If the returned result is an expression, it mostly likely means the collection
                                    // can not be serialized using statements, instead it has been serialized as resources.
                                    // In this case, we just over-write the variable init expression with the "GetObject"
                                    // expression for resource objects.
                                    statements.Add(new CodeAssignStatement(varExpression, result as CodeExpression));

                                result = statements;
                            }
                            finally
                            {
                                if (varExct != null)
                                    manager.Context.Pop();
                            }
                        }
                        else
                        {
                            result = this.originalSerializer.Serialize(manager, value);
                        }
                    }
                }
                else
                {
                    statements = new CodeStatementCollection();
                    CodeVariableReferenceExpression varExpression = AddVariableExpression(manager, statements, value);
                    SerializeProperties(manager, statements, value, new Attribute[] { DesignOnlyAttribute.No });
                    SerializeEvents(manager, statements, value, new Attribute[] { DesignOnlyAttribute.No });
                    result = statements;
                }
            }
            else if (cxt != null)
            {
                result = this.originalSerializer.Serialize(manager, value);
            }

            return result;
        }

        private CodeVariableReferenceExpression AddVariableExpression(IDesignerSerializationManager manager, CodeStatementCollection statements, object value)
        {
            string varName = GetUniqueName(manager, value).Replace('`', '_');
            CodeVariableDeclarationStatement varDecl = new CodeVariableDeclarationStatement(TypeDescriptor.GetClassName(value), varName);
            varDecl.InitExpression = new CodeObjectCreateExpression(TypeDescriptor.GetClassName(value), new CodeExpression[0]);
            statements.Add(varDecl);
            CodeVariableReferenceExpression varExpression = new CodeVariableReferenceExpression(varName);
            SetExpression(manager, value, varExpression);

            return varExpression;
        }
    }
    #endregion
}
