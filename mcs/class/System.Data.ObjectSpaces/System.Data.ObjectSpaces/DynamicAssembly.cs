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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

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
