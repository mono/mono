//
// System.Data.ObjectSpaces.DynamicAssembly.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

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
                        this.type = type;        
                }
                
                public Type UnderLyingType {
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