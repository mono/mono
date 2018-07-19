using System;
using System.Collections.Generic;



namespace System.Workflow.ComponentModel
{
    [Flags]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DependencyPropertyOptions : byte
    {
        Default = 1,
        ReadOnly = 2,
        Optional = 4,
        Metadata = 8,
        NonSerialized = 16,
        DelegateProperty = 32
    }


    //overrides so you dont need to do inheritence
    public delegate object GetValueOverride(DependencyObject d);
    public delegate void SetValueOverride(DependencyObject d, object value);

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class PropertyMetadata
    {
        private Attribute[] attributes = null;
        private object defaultValue = null;
        private DependencyPropertyOptions options = DependencyPropertyOptions.Default;
        private bool propertySealed = false;

        private GetValueOverride getValueOverride = null;
        private SetValueOverride setValueOverride = null;
        private bool shouldAlwaysCallOverride = false;

        public PropertyMetadata() { }

        public PropertyMetadata(object defaultValue)
            : this(defaultValue, default(DependencyPropertyOptions))
        {

        }

        public PropertyMetadata(DependencyPropertyOptions options)
            : this(null, options)
        {

        }
        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options)
            : this(defaultValue, options, null, null, new Attribute[] { })
        {

        }
        public PropertyMetadata(object defaultValue, params Attribute[] attributes)
            : this(defaultValue, default(DependencyPropertyOptions), null, null, attributes)
        {

        }
        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, params Attribute[] attributes)
            : this(defaultValue, options, null, null, attributes)
        {

        }
        public PropertyMetadata(DependencyPropertyOptions options, params Attribute[] attributes)
            : this(null, options, null, null, attributes)
        {

        }
        public PropertyMetadata(params Attribute[] attributes)
            : this(null, default(DependencyPropertyOptions), null, null, attributes)
        {

        }

        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, GetValueOverride getValueOverride, SetValueOverride setValueOverride)
            :
            this(defaultValue, options, getValueOverride, setValueOverride, new Attribute[] { })
        {

        }

        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, GetValueOverride getValueOverride, SetValueOverride setValueOverride, params Attribute[] attributes)
            : this(defaultValue, options, getValueOverride, setValueOverride, false, attributes)
        {

        }


        internal PropertyMetadata(object defaultValue, DependencyPropertyOptions options, GetValueOverride getValueOverride, SetValueOverride setValueOverride, bool shouldAlwaysCallOverride, params Attribute[] attributes)
        {
            this.defaultValue = defaultValue;
            this.options = options;
            this.getValueOverride = getValueOverride;
            this.setValueOverride = setValueOverride;
            this.shouldAlwaysCallOverride = shouldAlwaysCallOverride;
            this.attributes = attributes;
        }

        public Attribute[] GetAttributes()
        {
            return GetAttributes(null);
        }

        public Attribute[] GetAttributes(Type attributeType)
        {
            List<Attribute> attributes = new List<Attribute>();
            if (this.attributes != null)
            {
                foreach (Attribute attribute in this.attributes)
                {
                    if (attribute == null)
                        continue;

                    if (attributeType == null || attribute.GetType() == attributeType)
                        attributes.Add(attribute);
                }
            }
            return attributes.ToArray();
        }

        public object DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                if (this.propertySealed)
                    throw new InvalidOperationException(SR.GetString(SR.Error_SealedPropertyMetadata));

                this.defaultValue = value;
            }
        }
        public DependencyPropertyOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                if (this.propertySealed)
                    throw new InvalidOperationException(SR.GetString(SR.Error_SealedPropertyMetadata));

                this.options = value;
            }
        }

        public bool IsMetaProperty
        {
            get
            {
                return (this.options & DependencyPropertyOptions.Metadata) > 0;
            }
        }
        public bool IsNonSerialized
        {
            get
            {
                return (this.options & DependencyPropertyOptions.NonSerialized) > 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (this.options & DependencyPropertyOptions.ReadOnly) > 0;
            }
        }

        public GetValueOverride GetValueOverride
        {
            get
            {
                return this.getValueOverride;
            }
            set
            {
                if (this.propertySealed)
                    throw new InvalidOperationException(SR.GetString(SR.Error_SealedPropertyMetadata));

                this.getValueOverride = value;
            }
        }
        public SetValueOverride SetValueOverride
        {
            get
            {
                return this.setValueOverride;
            }
            set
            {
                if (this.propertySealed)
                    throw new InvalidOperationException(SR.GetString(SR.Error_SealedPropertyMetadata));

                this.setValueOverride = value;
            }
        }

        protected virtual void OnApply(DependencyProperty dependencyProperty, Type targetType)
        {
        }

        protected bool IsSealed
        {
            get
            {
                return this.propertySealed;
            }
        }

        internal bool ShouldAlwaysCallOverride
        {
            get
            {
                return shouldAlwaysCallOverride;
            }
        }

        internal void Seal(DependencyProperty dependencyProperty, Type targetType)
        {
            OnApply(dependencyProperty, targetType);

            this.propertySealed = true;
        }
    }
}
