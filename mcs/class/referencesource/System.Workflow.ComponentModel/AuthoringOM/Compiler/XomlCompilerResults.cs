namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.CodeDom.Compiler;

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowCompilerResults : CompilerResults
    {
        private CodeCompileUnit compiledCCU;

        internal WorkflowCompilerResults(TempFileCollection tempFiles)
            : base(tempFiles)
        {
        }

        public CodeCompileUnit CompiledUnit
        {
            get
            {
                return this.compiledCCU;
            }
            internal set
            {
                this.compiledCCU = value;
            }
        }

        internal void AddCompilerErrorsFromCompilerResults(CompilerResults results)
        {
            foreach (CompilerError error in results.Errors)
                base.Errors.Add(new WorkflowCompilerError(error));
            foreach (string msg in results.Output)
                base.Output.Add(msg);
        }
    }
}
