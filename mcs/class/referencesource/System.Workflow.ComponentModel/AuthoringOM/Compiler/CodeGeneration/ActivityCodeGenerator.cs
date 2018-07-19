namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    #region Class ActivityCodeGenerator
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityCodeGenerator
    {
        public virtual void GenerateCode(CodeGenerationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            manager.Context.Push(activity);

            // Generate code for all the member Binds.
            Walker walker = new Walker();
            walker.FoundProperty += delegate(Walker w, WalkerEventArgs args)
            {
                //
                ActivityBind bindBase = args.CurrentValue as ActivityBind;
                if (bindBase != null)
                {
                    // push
                    if (args.CurrentProperty != null)
                        manager.Context.Push(args.CurrentProperty);
                    manager.Context.Push(args.CurrentPropertyOwner);

                    // call generate code
                    foreach (ActivityCodeGenerator codeGenerator in manager.GetCodeGenerators(bindBase.GetType()))
                        codeGenerator.GenerateCode(manager, args.CurrentValue);

                    // pops
                    manager.Context.Pop();
                    if (args.CurrentProperty != null)
                        manager.Context.Pop();
                }
            };
            walker.WalkProperties(activity, obj);
            manager.Context.Pop();
        }

        protected CodeTypeDeclaration GetCodeTypeDeclaration(CodeGenerationManager manager, string fullClassName)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (fullClassName == null)
                throw new ArgumentNullException("fullClassName");

            string namespaceName;
            string className;
            Helpers.GetNamespaceAndClassName(fullClassName, out namespaceName, out className);

            CodeNamespaceCollection codeNamespaces = manager.Context[typeof(CodeNamespaceCollection)] as CodeNamespaceCollection;
            if (codeNamespaces == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(CodeNamespaceCollection).Name));

            CodeNamespace codeNS = null;
            return Helpers.GetCodeNamespaceAndClass(codeNamespaces, namespaceName, className, out codeNS);
        }
    }
    #endregion
}
