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

    #region Class ActivityTypeCodeDomSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityTypeCodeDomSerializer : TypeCodeDomSerializer
    {
        private static object _initMethodKey = new object();
        private const string _initMethodName = "InitializeComponent";

        protected override CodeMemberMethod GetInitializeMethod(IDesignerSerializationManager manager, CodeTypeDeclaration typeDecl, object value)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (typeDecl == null)
                throw new ArgumentNullException("typeDecl");
            if (value == null)
                throw new ArgumentNullException("value");

            CodeMemberMethod method = typeDecl.UserData[_initMethodKey] as CodeMemberMethod;
            if (method == null)
            {
                method = new CodeMemberMethod();
                method.Name = _initMethodName;
                method.Attributes = MemberAttributes.Private;
                typeDecl.UserData[_initMethodKey] = method;

                // Now create a ctor that calls this method.
                CodeConstructor ctor = new CodeConstructor();
                ctor.Attributes = MemberAttributes.Public;
                ctor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), _initMethodName));
                typeDecl.Members.Add(ctor);
            }
            return method;
        }

        protected override CodeMemberMethod[] GetInitializeMethods(IDesignerSerializationManager manager, CodeTypeDeclaration typeDecl)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (typeDecl == null)
                throw new ArgumentNullException("typeDecl");

            foreach (CodeTypeMember member in typeDecl.Members)
            {
                CodeMemberMethod method = member as CodeMemberMethod;

                // Note: the order is important here for performance! 
                // method.Parameters causes OnMethodPopulateParameters callback and therefore it is much more 
                // expensive than method.Name.Equals
                if (method != null && method.Name.Equals(_initMethodName) && method.Parameters.Count == 0)
                {
                    return new CodeMemberMethod[] { method };
                }
            }
            return new CodeMemberMethod[0];
        }

        public override CodeTypeDeclaration Serialize(IDesignerSerializationManager manager, object root, ICollection members)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (root == null)
                throw new ArgumentNullException("root");

            Activity rootActivity = root as Activity;
            if (rootActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "root");

            CodeTypeDeclaration codeTypeDeclaration = base.Serialize(manager, root, members);

            // Emit CanModifyActivities properties
            CodeMemberMethod method = codeTypeDeclaration.UserData[_initMethodKey] as CodeMemberMethod;
            if (method != null && rootActivity is CompositeActivity)
            {
                CodeStatement[] codeStatements = new CodeStatement[method.Statements.Count];
                method.Statements.CopyTo(codeStatements, 0);
                method.Statements.Clear();

                CodeAssignStatement beginInit = new CodeAssignStatement();
                beginInit.Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CanModifyActivities");
                beginInit.Right = new CodePrimitiveExpression(true);
                method.Statements.Add(beginInit);

                foreach (CodeStatement s in codeStatements)
                    method.Statements.Add(s);

                CodeAssignStatement endInit = new CodeAssignStatement();
                endInit.Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CanModifyActivities");
                endInit.Right = new CodePrimitiveExpression(false);
                method.Statements.Add(endInit);
            }

            foreach (CodeTypeMember member in codeTypeDeclaration.Members)
            {
                CodeMemberField field = member as CodeMemberField;
                if (field != null)
                {
                    foreach (object objectActivity in members)
                    {
                        if (!(objectActivity is Activity))
                            throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "members");

                        Activity activity = objectActivity as Activity;
                        if (field.Name == manager.GetName(activity) &&
                            (int)activity.GetValue(ActivityMarkupSerializer.StartLineProperty) != -1 &&
                            rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty) != null)
                        {
                            // generate line pragma for fields
                            field.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)activity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                        }
                    }
                }
            }
            return codeTypeDeclaration;
        }

        public override object Deserialize(IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
        {
            return base.Deserialize(manager, declaration);
        }
    }
    #endregion

}
