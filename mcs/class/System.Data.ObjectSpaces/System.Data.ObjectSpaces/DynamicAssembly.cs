//
// System.Data.ObjectSpaces.DynamicAssembly.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003-2004
//

#if NET_1_2

using System.Data.Mapping;
using System.Reflection;

namespace System.Data.ObjectSpaces
{
        [MonoTODO]
        public class DynamicAssembly
        {
                private Type type;              //The underlying type
                protected FieldInfo fInfo;
                protected PropertyInfo pInfo;
               
                [MonoTODO]
                protected DynamicAssembly (Type type) 
                {
			ConstructorInfo ci = type.GetConstructor (Type.EmptyTypes);
			if (ci == null)
				throw new ObjectException (String.Format ("Cannot find suitable constructor in type '{0}'", type.FullName));
			this.type = type;
                }

                public Type UnderlyingType {
                        get { return this.type; }
                }
                
                
                [MonoTODO]
                public virtual object CreateObject (ObjectEngine engine,
                                                    ObjectContext context,
                                                    MappingSchema map,
                                                    ObjectSources sources) 
                {
                        return null;        
                }

		internal static DynamicAssembly GetDynamicAssembly (Type type)
		{
			return new DynamicAssembly (type);
		}

                [MonoTODO]
                public FieldInfo[] GetFields() 
                {
                        return null;        
                }

                [MonoTODO]
                public PropertyInfo[] GetProperties () 
                {
                        return null;        
                }



                [MonoTODO]
                public virtual object GetValue (object obj, PropertyInfo propertyInfo) 
                {
                        return null;        
                }
                
                [MonoTODO]
                public virtual object GetValue (object obj, FieldInfo fieldInfo)
                {
                        return null;        
                }
                
                [MonoTODO]
                public virtual object GetValue (object obj, MemberInfo memberInfo, string memberPath)
                {
                        return null;        
                }



                [MonoTODO]
                public virtual void SetValue (object obj, PropertyInfo propertyInfo, object value) {}
                
                [MonoTODO]
                public virtual void SetValue (object obj, FieldInfo fieldInfo, object value) {}
                
                [MonoTODO]
                public virtual void SetValue (object obj, MemberInfo memberInfo, string memberPath, object value) {}  
        }
}

#endif
