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
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{

    [ComVisible (true)]
    public abstract class MemberDescriptor 
    {

        private string name;
        private Attribute [] attrs;
        private AttributeCollection attrCollection;
		
        protected MemberDescriptor (string name, Attribute [] attrs)
        {
            this.name = name;
            this.attrs = attrs;
        }

        protected MemberDescriptor (MemberDescriptor reference, Attribute [] attrs)
        {
            name = reference.name;
            this.attrs = attrs;
        }

        protected MemberDescriptor (string name)
        {
            this.name = name;
        }

        protected MemberDescriptor (MemberDescriptor reference)
        {
            name = reference.name;
            attrs = reference.AttributeArray;
        }

        protected virtual Attribute [] AttributeArray 
        {
            get 
            {
				if (attrs == null) 
				{
					ArrayList list = new ArrayList ();
					FillAttributes (list);
					
					ArrayList filtered = new ArrayList ();
					foreach (Attribute at in list) {
						bool found = false;
						for (int n=0; n<filtered.Count && !found; n++)
							found = (filtered[n].GetType() == at.GetType ());
						if (!found)
							filtered.Add (at);
					}
					attrs = (Attribute[]) filtered.ToArray (typeof(Attribute));
				}
				
                return attrs;
            }

            set 
            {
                attrs = value;
            }
        }

        protected virtual void FillAttributes(System.Collections.IList attributeList)
        {
			// to be overriden
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
            return new AttributeCollection (AttributeArray);
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
                foreach (Attribute attr in AttributeArray)
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
                foreach (Attribute attr in AttributeArray)
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
                return name;
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
                foreach (Attribute attr in AttributeArray)
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

        public override bool Equals(object obj)
        {
			MemberDescriptor other = obj as MemberDescriptor;
            if (obj == null) return false;
			
            return other.name == name;
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
			
			// Lluis: Checked with VS.NET and it always return the component, even if
			// it has its own designer set with DesignerAttribute. So, no idea
			// what this should do.
            return component;
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
