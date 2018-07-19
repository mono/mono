namespace System.Workflow.ComponentModel.Compiler
{
    #region Imports

    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;

    #endregion

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ValidationManager : IServiceProvider
    {
        #region Data members

        private Hashtable hashOfValidators = new Hashtable();
        private IServiceProvider serviceProvider = null;
        private ContextStack context = null;
        private bool validateChildActivities = true;

        #endregion

        #region Constructors

        public ValidationManager(IServiceProvider serviceProvider)
            :
            this(serviceProvider, true)
        {
        }

        public ValidationManager(IServiceProvider serviceProvider, bool validateChildActivities)
        {
            this.serviceProvider = serviceProvider;
            this.validateChildActivities = validateChildActivities;
        }

        #endregion

        #region Public members

        public ContextStack Context
        {
            get
            {
                if (this.context == null)
                    this.context = new ContextStack();

                return this.context;
            }
        }

        public bool ValidateChildActivities
        {
            get
            {
                return this.validateChildActivities;
            }
        }

        public Validator[] GetValidators(Type type)
        {
            if (this.hashOfValidators.Contains(type))
                return ((List<Validator>)this.hashOfValidators[type]).ToArray();

            List<Validator> validators = new List<Validator>();
            foreach (Validator validator in ComponentDispenser.CreateComponents(type, typeof(ActivityValidatorAttribute)))
                validators.Add(validator);

            this.hashOfValidators[type] = validators;
            return validators.ToArray();
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        #endregion
    }
}
