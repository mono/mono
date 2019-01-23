//
// System.Reflection/MonoEvent.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004, 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace System.Reflection {

	internal struct MonoEventInfo {
		public Type declaring_type;
		public Type reflected_type;
		public String name;
		public MethodInfo add_method;
		public MethodInfo remove_method;
		public MethodInfo raise_method;
		public EventAttributes attrs;
		public MethodInfo[] other_methods;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void get_event_info (MonoEvent ev, out MonoEventInfo info);

		internal static MonoEventInfo GetEventInfo (MonoEvent ev)
		{
			MonoEventInfo mei;
			MonoEventInfo.get_event_info (ev, out mei);
			return mei;
		}
	}

	abstract class RuntimeEventInfo : EventInfo, ISerializable
	{
        internal abstract BindingFlags GetBindingFlags ();

		public override Module Module {
			get {
				return GetRuntimeModule ();
			}
		}

		internal BindingFlags BindingFlags {
			get {
				return GetBindingFlags ();
			}
		}

		internal RuntimeType GetDeclaringTypeInternal ()
		{
			return (RuntimeType) DeclaringType;
		}

		RuntimeType ReflectedTypeInternal {
			get {
				return (RuntimeType) ReflectedType;
			}
		}

		internal RuntimeModule GetRuntimeModule ()
		{
			return GetDeclaringTypeInternal ().GetRuntimeModule ();
		}

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            Contract.EndContractBlock();

            MemberInfoSerializationHolder.GetSerializationInfo(
                info,
                Name,
                ReflectedTypeInternal,
                null,
                MemberTypes.Event);
        }
        #endregion
	}

	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	internal sealed class MonoEvent: RuntimeEventInfo
	{
#pragma warning disable 169
		IntPtr klass;
		IntPtr handle;
#pragma warning restore 169

		internal override BindingFlags GetBindingFlags ()
		{
			MonoEventInfo info = MonoEventInfo.GetEventInfo (this);

			MethodInfo method = info.add_method;
			if (method == null)
				method = info.remove_method;
			if (method == null)
				method = info.raise_method;

			return RuntimeType.FilterPreCalculate (method != null && method.IsPublic, GetDeclaringTypeInternal () != ReflectedType , method != null && method.IsStatic);
		}

        public override EventAttributes Attributes {
			get {
				return MonoEventInfo.GetEventInfo (this).attrs;
			}
		}

		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			MonoEventInfo info = MonoEventInfo.GetEventInfo (this);
			if (nonPublic || (info.add_method != null && info.add_method.IsPublic))
				return info.add_method;
			return null;
		}

		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			MonoEventInfo info = MonoEventInfo.GetEventInfo (this);
			if (nonPublic || (info.raise_method != null && info.raise_method.IsPublic))
				return info.raise_method;
			return null;
		}

		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			MonoEventInfo info = MonoEventInfo.GetEventInfo (this);
			if (nonPublic || (info.remove_method != null && info.remove_method.IsPublic))
				return info.remove_method;
			return null;
		}

		public override MethodInfo[] GetOtherMethods (bool nonPublic)
		{
			MonoEventInfo info = MonoEventInfo.GetEventInfo (this);
			if (nonPublic)
				return info.other_methods;
			int num_public = 0;
			foreach (MethodInfo m in info.other_methods) {
				if (m.IsPublic)
					num_public++;
			}
			if (num_public == info.other_methods.Length)
				return info.other_methods;
			MethodInfo[] res = new MethodInfo [num_public];
			num_public = 0;
			foreach (MethodInfo m in info.other_methods) {
				if (m.IsPublic)
					res [num_public++] = m;
			}
			return res;
		}

		public override Type DeclaringType {
			get {
				return MonoEventInfo.GetEventInfo (this).declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return MonoEventInfo.GetEventInfo (this).reflected_type;
			}
		}

		public override string Name {
			get {
				return MonoEventInfo.GetEventInfo (this).name;
			}
		}

		public override string ToString ()
		{
			return EventHandlerType + " " + Name;
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes( Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}

		public override int MetadataToken {
			get {
				return get_metadata_token (this);
			}
		}

#if !NETCORE
		public sealed override bool HasSameMetadataDefinitionAs (MemberInfo other) => HasSameMetadataDefinitionAsCore<MonoEvent> (other);
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int get_metadata_token (MonoEvent monoEvent);
	}
}
