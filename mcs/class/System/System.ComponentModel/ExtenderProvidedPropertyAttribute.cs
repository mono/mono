//
// System.ComponentModel.ExtenderProvidedPropertyAttribute
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
    [MonoTODO ("Some method needed to actually SET some values")]
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ExtenderProvidedPropertyAttribute : Attribute
	{

        private PropertyDescriptor extender;
        private IExtenderProvider extenderProvider;
        private Type receiver;

        public ExtenderProvidedPropertyAttribute()
        {
        }

        public PropertyDescriptor ExtenderProperty
        {
            get
            {
                return extender;
            }
        }

        public IExtenderProvider Provider
        {
            get
            {
                return extenderProvider;
            }
        }

        public Type ReceiverType
        {
            get
            {
                return receiver;
            }
        }

        public override bool IsDefaultAttribute()
        {
            // FIXME correct implementation??
            return (extender == null) &&
                (extenderProvider == null) &&
                (receiver == null);
        }

		public override bool Equals (object obj)
		{
            if (!(obj is ExtenderProvidedPropertyAttribute))
                return false;
            if (obj == this)
                return true;
            return ((ExtenderProvidedPropertyAttribute) obj).ExtenderProperty.Equals (extender) &&
                ((ExtenderProvidedPropertyAttribute) obj).Provider.Equals (extenderProvider) &&
                ((ExtenderProvidedPropertyAttribute) obj).ReceiverType.Equals (receiver);
		}

		public override int GetHashCode()
		{
            // FIXME correct implementation??
            return base.GetHashCode();
		}
	}
}
