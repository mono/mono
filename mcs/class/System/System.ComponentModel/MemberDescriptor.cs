//
// System.ComponentModel.MemberDescriptor.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{

    [ComVisible (true)]
    public abstract class MemberDescriptor 
    {

        private string name;
        private string displayName;
        private Attribute [] attrs;
        private AttributeCollection attrCollection;
		
        protected MemberDescriptor (string name, Attribute [] attrs)
        {
            this.name = name;
            this.displayName = name;
            this.attrs = attrs;
        }

        protected MemberDescriptor (MemberDescriptor reference, Attribute [] attrs)
        {
            name = reference.name;
            this.displayName = name;
            this.attrs = attrs;
        }

        protected MemberDescriptor (string name)
        {
            this.name = name;
            this.displayName = name;
        }

        protected MemberDescriptor (MemberDescriptor reference)
        {
            name = reference.name;
            this.displayName = name;
            attrs = reference.attrs;
        }

        protected virtual Attribute [] AttributeArray 
        {
            get 
            {
                return attrs;
            }

            set 
            {
                attrs = value;
            }
        }

        [MonoTODO]
        protected virtual void FillAttributes(System.Collections.IList attributeList)
        {
            // LAMESPEC/FIXME - I don't think this is correct, but didn't really understand
            // what this sub is good for
            attributeList = this.attrs;
            return;
        }

        public virtual AttributeCollection Attributes
        {
            get 
            {
                if (attrCollection == null)
                    attrCollection = CreateAttributeCollection ();
                return attrCollection;
            }
        }

        protected virtual AttributeCollection CreateAttributeCollection()
        {
            return new AttributeCollection (attrs);
        }
			
        public virtual string Category 
        {
            get 
            {
                return ((CategoryAttribute) Attributes [typeof (CategoryAttribute)]).Category;
            }
        }

        public virtual string Description 
        {
            get 
            {
                foreach (Attribute attr in attrs)
                {
                    if (attr is DescriptionAttribute)
                        return ((DescriptionAttribute) attr).Description;
                }
                return "";
            }
        }

        public virtual bool DesignTimeOnly 
        {
            get 
            {
                foreach (Attribute attr in attrs)
                {
                    if (attr is DesignOnlyAttribute)
                        return ((DesignOnlyAttribute) attr).IsDesignOnly;
                }

                return false;
            }
        }

        public virtual string DisplayName 
        {
            get 
            {
                return displayName;
            }
        }

        public virtual string Name 
        {
            get 
            {
                return name;
            }
        }

        public virtual bool IsBrowsable 
        {
            get 
            {
                foreach (Attribute attr in attrs)
                {
                    if (attr is BrowsableAttribute)
                        return ((BrowsableAttribute) attr).Browsable;
                }

                return false;
            }
        }

        protected virtual int NameHashCode 
        {
            get 
            {
                return name.GetHashCode ();
            }
        }

        public override int GetHashCode() 
        {
            return name.GetHashCode ();
        }

        [MonoTODO ("Probably not correctly implemented (too harsh?)")]
        public override bool Equals(object obj)
        {
            if (!(obj is MemberDescriptor))
                return false;
            if (obj == this)
                return true;
            return (((MemberDescriptor) obj).AttributeArray == attrs) &&
                (((MemberDescriptor) obj).Attributes == attrCollection) &&
                (((MemberDescriptor) obj).DisplayName == displayName) &&
                (((MemberDescriptor) obj).Name == name);
        }

        protected static ISite GetSite(object component)
        {
            if (component is Component)
                return ((Component) component).Site;
            else
                return null;
        }

        [MonoTODO]
        protected static object GetInvokee(Type componentClass, object component)
        {
            // FIXME WHAT should that do???
            throw new NotImplementedException ();
        }

        protected static MethodInfo FindMethod(Type componentClass, string name, 
            Type[ ] args, Type returnType)
        {
            return FindMethod (componentClass, name, args, returnType, true);
        }

        protected static MethodInfo FindMethod(Type componentClass, string name, 
            Type[ ] args, Type returnType, bool publicOnly)
        {
            BindingFlags bf;
            if (publicOnly == true)
                bf = BindingFlags.Public;
            else
                bf = BindingFlags.NonPublic | BindingFlags.Public;
            // FIXME returnType is not taken into account. AFAIK methods are not allowed to only
            // differ by return type anyway
            return componentClass.GetMethod (name, bf, null, CallingConventions.Any, args, null);
        }
    }
}
