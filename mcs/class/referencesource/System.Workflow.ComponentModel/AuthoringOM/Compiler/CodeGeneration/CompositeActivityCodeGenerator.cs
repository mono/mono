using System.Workflow.ComponentModel.Design;
namespace System.Workflow.ComponentModel.Compiler
{
    #region Class CompositeActivityCodeGenerator

    using System.Workflow.ComponentModel.Design;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeActivityCodeGenerator : ActivityCodeGenerator
    {
        public override void GenerateCode(CodeGenerationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            CompositeActivity compositeActivity = obj as CompositeActivity;
            if (compositeActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(CompositeActivity).FullName), "obj");

            base.GenerateCode(manager, obj);

            foreach (Activity child in Helpers.GetAllEnabledActivities(compositeActivity))
            {
                foreach (ActivityCodeGenerator codeGenerator in manager.GetCodeGenerators(child.GetType()))
                    codeGenerator.GenerateCode(manager, child);
            }
        }
    }
    #endregion
}
