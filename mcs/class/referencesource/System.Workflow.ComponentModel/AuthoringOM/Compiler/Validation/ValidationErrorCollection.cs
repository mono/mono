namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    #region ValidationErrorCollection

    [Serializable()]
    public sealed class ValidationErrorCollection : Collection<ValidationError>
    {
        public ValidationErrorCollection()
        {
        }

        public ValidationErrorCollection(ValidationErrorCollection value)
        {
            this.AddRange(value);
        }

        public ValidationErrorCollection(IEnumerable<ValidationError> value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            this.AddRange(value);
        }

        protected override void InsertItem(int index, ValidationError item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ValidationError item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            base.SetItem(index, item);
        }

        public void AddRange(IEnumerable<ValidationError> value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            foreach (ValidationError error in value)
                this.Add(error);
        }

        public bool HasErrors
        {
            get
            {
                if (Count > 0)
                {
                    foreach (ValidationError e in this)
                    {
                        if (e != null && !e.IsWarning)
                            return true;
                    }
                }
                return false;
            }
        }

        public bool HasWarnings
        {
            get
            {
                if (Count > 0)
                {
                    foreach (ValidationError e in this)
                    {
                        if (e != null && e.IsWarning)
                            return true;
                    }
                }
                return false;
            }
        }

        public ValidationError[] ToArray()
        {
            ValidationError[] errorsArray = new ValidationError[this.Count];
            this.CopyTo(errorsArray, 0);
            return errorsArray;
        }
    }
    #endregion
}
