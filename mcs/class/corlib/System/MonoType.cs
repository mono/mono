//
// System.MonoType
//
// Authors: 
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
// 	Patrik Torstensson (patrik.torstensson@labs2.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//  Marek Safar (marek.safar@gmail.com)
//
// (c) 2001-2003 Ximian, Inc.
// Copyright (C) 2003-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.Remoting.Activation;
using System.Runtime;

namespace System
{
	// Contains information about the type which is expensive to compute
	[StructLayout (LayoutKind.Sequential)]
	internal class MonoTypeInfo {
		public string full_name;
		public MonoCMethod default_ctor;
	}

	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	class MonoType : RuntimeType, ISerializable
	{
		[NonSerialized]
		MonoTypeInfo type_info;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		internal override MonoCMethod GetDefaultConstructor ()
		{
			MonoCMethod ctor = null;
			
			if (type_info == null)
				type_info = new MonoTypeInfo ();
			else
				ctor = type_info.default_ctor;

			if (ctor == null) {
				var ctors = GetConstructors (BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

				for (int i = 0; i < ctors.Length; ++i) {
					if (ctors [i].GetParametersCount () == 0) {
						type_info.default_ctor = ctor = (MonoCMethod) ctors [i];
						break;
					}
				}
			}

			return ctor;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern ConstructorInfo GetCorrespondingInflatedConstructor (ConstructorInfo generic);

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
                {
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedMethod (fromNoninstanciated);
                }

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedConstructor (fromNoninstanciated);
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			/* create sensible flags from given FieldInfo */
			BindingFlags flags = fromNoninstanciated.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
			flags |= fromNoninstanciated.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			return GetField (fromNoninstanciated.Name, flags);
		}

		public override int GetHashCode()
		{
			Type t = UnderlyingSystemType;
			if (t != null && t != this)
				return t.GetHashCode ();
			return (int)_impl.Value;
		}

		public override string FullName {
			get {
				string fullName;
				// This doesn't need locking
				if (type_info == null)
					type_info = new MonoTypeInfo ();
				if ((fullName = type_info.full_name) == null)
					fullName = type_info.full_name = getFullName (true, false);

				return fullName;
			}
		}

		internal override bool IsUserType {
			get {
				return false;
			}
		}
	}
}
