namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Collections.Generic;

    #region CodeGenerationManager

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CodeGenerationManager : IServiceProvider
    {
        private Hashtable hashOfGenerators = new Hashtable();
        private IServiceProvider serviceProvider = null;
        private ContextStack context = null;

        public CodeGenerationManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ContextStack Context
        {
            get
            {
                if (this.context == null)
                    this.context = new ContextStack();

                return this.context;
            }
        }

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider == null)
                return null;

            return this.serviceProvider.GetService(serviceType);
        }
        #endregion

        public ActivityCodeGenerator[] GetCodeGenerators(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (this.hashOfGenerators.Contains(type))
                return ((List<ActivityCodeGenerator>)this.hashOfGenerators[type]).ToArray();

            List<ActivityCodeGenerator> generators = new List<ActivityCodeGenerator>();

            // Return validators for other types such as Bind, XmolDocument, etc.
            foreach (ActivityCodeGenerator generator in ComponentDispenser.CreateComponents(type, typeof(ActivityCodeGeneratorAttribute)))
            {
                generators.Add(generator);
            }

            this.hashOfGenerators[type] = generators;
            return generators.ToArray();
        }
    }
    #endregion
}
