//
// PropertyBuilder.pns.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if !MONO_FEATURE_SRE

namespace System.Reflection.Emit
{
	public sealed partial class PropertyBuilder : System.Reflection.PropertyInfo
    {
        internal PropertyBuilder() { throw new PlatformNotSupportedException (); } 
        public override System.Reflection.PropertyAttributes Attributes { get { throw new PlatformNotSupportedException (); } }
        public override bool CanRead { get { throw new PlatformNotSupportedException (); } }
        public override bool CanWrite { get { throw new PlatformNotSupportedException (); } }
        public override System.Type DeclaringType { get { throw new PlatformNotSupportedException (); } }
        public override System.Reflection.Module Module { get { throw new PlatformNotSupportedException (); } }
        public override string Name { get { throw new PlatformNotSupportedException (); } }
        public System.Reflection.Emit.PropertyToken PropertyToken { get { throw new PlatformNotSupportedException (); } }
        public override System.Type PropertyType { get { throw new PlatformNotSupportedException (); } }
        public override System.Type ReflectedType { get { throw new PlatformNotSupportedException (); } }
        public void AddOtherMethod(System.Reflection.Emit.MethodBuilder mdBuilder) { throw new PlatformNotSupportedException (); } 
        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic) { throw new PlatformNotSupportedException (); }
        public override object[] GetCustomAttributes(bool inherit) { throw new PlatformNotSupportedException (); }
        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
        public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic) { throw new PlatformNotSupportedException (); }
        public override System.Reflection.ParameterInfo[] GetIndexParameters() { throw new PlatformNotSupportedException (); }
        public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic) { throw new PlatformNotSupportedException (); }
        public override object GetValue(object obj, object[] index) { throw new PlatformNotSupportedException (); }
        public override object GetValue(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, System.Globalization.CultureInfo culture) { throw new PlatformNotSupportedException (); }
        public override bool IsDefined(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
        public void SetConstant(object defaultValue) { throw new PlatformNotSupportedException (); } 
        public void SetCustomAttribute(System.Reflection.ConstructorInfo con, byte[] binaryAttribute) { throw new PlatformNotSupportedException (); } 
        public void SetCustomAttribute(System.Reflection.Emit.CustomAttributeBuilder customBuilder) { throw new PlatformNotSupportedException (); } 
        public void SetGetMethod(System.Reflection.Emit.MethodBuilder mdBuilder) { throw new PlatformNotSupportedException (); } 
        public void SetSetMethod(System.Reflection.Emit.MethodBuilder mdBuilder) { throw new PlatformNotSupportedException (); } 
        public override void SetValue(object obj, object value, object[] index) { throw new PlatformNotSupportedException (); } 
        public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, System.Globalization.CultureInfo culture) { throw new PlatformNotSupportedException (); } 
    }
}

#endif
