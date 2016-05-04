//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.PropertyEditing
{
    using System.Runtime;
    using System.Collections;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class EditorOptionAttribute : Attribute
    {
        public string Name { get; set; }
        public object Value { get; set; }
        //TypeId is needed so that multiple EditorOptionsAttribute could be added to the same property
        public override object TypeId
        {
            get
            {
                return new EditorOptionsAttributeTypeId
                {
                    BaseTypeId = base.TypeId,
                    Name = this.Name,
                    Value = this.Value
                };
            }
        }

        public static bool TryGetOptionValue(IEnumerable attributes, string optionName, out object optionValue)
        {            
            foreach (Attribute attribute in attributes)
            {
                EditorOptionAttribute optionAttribute = attribute as EditorOptionAttribute;
                if (optionAttribute != null && optionAttribute.Name.Equals(optionName))
                {
                    optionValue = optionAttribute.Value;
                    return true;
                }
            }
            optionValue = null;
            return false;
        }

        //A class to uniquely identify a name-value pair
        class EditorOptionsAttributeTypeId
        {
            public object BaseTypeId { get; set; }
            public string Name { get; set; }
            public object Value { get; set; }

            public override bool Equals(object obj)
            {
                EditorOptionsAttributeTypeId that = obj as EditorOptionsAttributeTypeId;
                if (that == null) return false;
                return this.BaseTypeId == that.BaseTypeId &&
                    string.Equals(this.Name, that.Name) &&
                    object.Equals(this.Value, that.Value);
            }

            public override int GetHashCode()
            {
                return
                    (BaseTypeId == null ? 0 : BaseTypeId.GetHashCode()) ^
                    (Name == null ? 0 : Name.GetHashCode()) ^
                    (Value == null ? 0 : Value.GetHashCode());
            }
        }
    }
}
