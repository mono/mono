//
// ModuleBuilder.pns.cs
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
    public partial class ModuleBuilder : System.Reflection.Module
    {
        internal ModuleBuilder() => throw new PlatformNotSupportedException();
        public override System.Reflection.Assembly Assembly { get { throw new PlatformNotSupportedException(); } }
        public override string FullyQualifiedName { get { throw new PlatformNotSupportedException(); } }
        public override int MetadataToken { get { throw new PlatformNotSupportedException(); } }
        public override System.Guid ModuleVersionId { get { throw new PlatformNotSupportedException(); } }
        public override string Name { get { throw new PlatformNotSupportedException(); } }
        public override string ScopeName { get { throw new PlatformNotSupportedException(); } }
        public void CreateGlobalFunctions() => throw new PlatformNotSupportedException();
        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter DefineDocument (string url, System.Guid language, System.Guid languageVendor, System.Guid documentType) => throw new PlatformNotSupportedException ();
        public System.Reflection.Emit.EnumBuilder DefineEnum(string name, System.Reflection.TypeAttributes visibility, System.Type underlyingType) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodBuilder DefineGlobalMethod(string name, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodBuilder DefineGlobalMethod(string name, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] requiredReturnTypeCustomModifiers, System.Type[] optionalReturnTypeCustomModifiers, System.Type[] parameterTypes, System.Type[][] requiredParameterTypeCustomModifiers, System.Type[][] optionalParameterTypeCustomModifiers) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodBuilder DefineGlobalMethod(string name, System.Reflection.MethodAttributes attributes, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.FieldBuilder DefineInitializedData(string name, byte[] data, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException(); }
        public void DefineManifestResource (string name, System.IO.Stream stream, System.Reflection.ResourceAttributes attribute) => throw new PlatformNotSupportedException ();
        public System.Reflection.Emit.MethodBuilder DefinePInvokeMethod(string name, string dllName, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes, System.Runtime.InteropServices.CallingConvention nativeCallConv, System.Runtime.InteropServices.CharSet nativeCharSet) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes, System.Runtime.InteropServices.CallingConvention nativeCallConv, System.Runtime.InteropServices.CharSet nativeCharSet) { throw new PlatformNotSupportedException(); }
        public System.Resources.IResourceWriter DefineResource (string name, string description) => throw new PlatformNotSupportedException ();
        public System.Resources.IResourceWriter DefineResource (string name, string description, System.Reflection.ResourceAttributes attribute) => throw new PlatformNotSupportedException ();
        public void DefineUnmanagedResource (byte[] resource) => throw new PlatformNotSupportedException ();
        public void DefineUnmanagedResource (string resourceFileName) => throw new PlatformNotSupportedException ();
        public System.Reflection.Emit.TypeBuilder DefineType(string name) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent, int typesize) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Reflection.Emit.PackingSize packsize) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Reflection.Emit.PackingSize packingSize, int typesize) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeBuilder DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Type[] interfaces) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.FieldBuilder DefineUninitializedData(string name, int size, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public System.Reflection.MethodInfo GetArrayMethod(System.Type arrayClass, string methodName, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodToken GetArrayMethodToken(System.Type arrayClass, string methodName, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodToken GetConstructorToken(System.Reflection.ConstructorInfo con) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodToken GetConstructorToken(System.Reflection.ConstructorInfo constructor, System.Collections.Generic.IEnumerable<System.Type> optionalParameterTypes) { throw new PlatformNotSupportedException(); }
        public override object[] GetCustomAttributes (bool inherit) { throw new PlatformNotSupportedException(); }
        public override object[] GetCustomAttributes (System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException(); }
        public override System.Reflection.FieldInfo GetField (string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException(); }
        public override System.Reflection.FieldInfo[] GetFields (System.Reflection.BindingFlags bindingFlags) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.FieldToken GetFieldToken(System.Reflection.FieldInfo field) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        protected override System.Reflection.MethodInfo GetMethodImpl (string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException(); }
        public override System.Reflection.MethodInfo[] GetMethods (System.Reflection.BindingFlags bindingFlags) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodToken GetMethodToken(System.Reflection.MethodInfo method) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.MethodToken GetMethodToken(System.Reflection.MethodInfo method, System.Collections.Generic.IEnumerable<System.Type> optionalParameterTypes) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.SignatureToken GetSignatureToken(System.Reflection.Emit.SignatureHelper sigHelper) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.StringToken GetStringConstant(string str) { throw new PlatformNotSupportedException(); }
        public System.Diagnostics.SymbolStore.ISymbolWriter GetSymWriter () => throw new PlatformNotSupportedException ();
        public override System.Type GetType (string className) { throw new PlatformNotSupportedException(); }
        public override System.Type GetType (string className, bool ignoreCase) { throw new PlatformNotSupportedException(); }
        public override System.Type GetType (string className, bool throwOnError, bool ignoreCase) { throw new PlatformNotSupportedException(); }
        public override System.Type[] GetTypes () { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeToken GetTypeToken(string name) { throw new PlatformNotSupportedException(); }
        public System.Reflection.Emit.TypeToken GetTypeToken(System.Type type) { throw new PlatformNotSupportedException(); }
        public override bool IsDefined (System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException(); }
        public override bool IsResource () { throw new PlatformNotSupportedException(); }
        public bool IsTransient() { throw new PlatformNotSupportedException(); }
        public override System.Reflection.FieldInfo ResolveField (int metadataToken, System.Type[] genericTypeArguments, System.Type[] genericMethodArguments) { throw new PlatformNotSupportedException(); }
        public override System.Reflection.MemberInfo ResolveMember (int metadataToken, System.Type[] genericTypeArguments, System.Type[] genericMethodArguments) { throw new PlatformNotSupportedException(); }
        public override System.Reflection.MethodBase ResolveMethod (int metadataToken, System.Type[] genericTypeArguments, System.Type[] genericMethodArguments) { throw new PlatformNotSupportedException(); }
        public override byte[] ResolveSignature (int metadataToken) { throw new PlatformNotSupportedException(); }
        public override string ResolveString (int metadataToken) { throw new PlatformNotSupportedException(); }
        public override System.Type ResolveType (int metadataToken, System.Type[] genericTypeArguments, System.Type[] genericMethodArguments) { throw new PlatformNotSupportedException(); }
        public void SetCustomAttribute(System.Reflection.ConstructorInfo con, byte[] binaryAttribute) => throw new PlatformNotSupportedException();
        public void SetCustomAttribute(System.Reflection.Emit.CustomAttributeBuilder customBuilder) => throw new PlatformNotSupportedException();
        public void SetSymCustomAttribute (string name, byte[] data) => throw new PlatformNotSupportedException ();
        public void SetUserEntryPoint(System.Reflection.MethodInfo entryPoint) => throw new PlatformNotSupportedException();
    }

    public readonly partial struct MethodToken : System.IEquatable<MethodToken>
    {
        public static readonly System.Reflection.Emit.MethodToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.MethodToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.MethodToken a, System.Reflection.Emit.MethodToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.MethodToken a, System.Reflection.Emit.MethodToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct SignatureToken : System.IEquatable<SignatureToken>
    {
        public static readonly System.Reflection.Emit.SignatureToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.SignatureToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.SignatureToken a, System.Reflection.Emit.SignatureToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.SignatureToken a, System.Reflection.Emit.SignatureToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct FieldToken : System.IEquatable<FieldToken>
    {
        public static readonly System.Reflection.Emit.FieldToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.FieldToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.FieldToken a, System.Reflection.Emit.FieldToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.FieldToken a, System.Reflection.Emit.FieldToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct StringToken : System.IEquatable<StringToken>
    {
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.StringToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.StringToken a, System.Reflection.Emit.StringToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.StringToken a, System.Reflection.Emit.StringToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct TypeToken : System.IEquatable<TypeToken>
    {
        public static readonly System.Reflection.Emit.TypeToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.TypeToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.TypeToken a, System.Reflection.Emit.TypeToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.TypeToken a, System.Reflection.Emit.TypeToken b) { throw new PlatformNotSupportedException(); }
    }

    public partial class DynamicILInfo
    {
        internal DynamicILInfo() => throw new PlatformNotSupportedException();
        public System.Reflection.Emit.DynamicMethod DynamicMethod { get { throw new PlatformNotSupportedException(); } }
        public int GetTokenFor(byte[] signature) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.Reflection.Emit.DynamicMethod method) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.RuntimeFieldHandle field) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.RuntimeFieldHandle field, System.RuntimeTypeHandle contextType) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.RuntimeMethodHandle method) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.RuntimeMethodHandle method, System.RuntimeTypeHandle contextType) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(System.RuntimeTypeHandle type) { throw new PlatformNotSupportedException(); }
        public int GetTokenFor(string literal) { throw new PlatformNotSupportedException(); }
        [System.CLSCompliantAttribute(false)]
        public unsafe void SetCode(byte* code, int codeSize, int maxStackSize) => throw new PlatformNotSupportedException();
        public void SetCode(byte[] code, int maxStackSize) => throw new PlatformNotSupportedException();
        [System.CLSCompliantAttribute(false)]
        public unsafe void SetExceptions(byte* exceptions, int exceptionsSize) => throw new PlatformNotSupportedException();
        public void SetExceptions(byte[] exceptions) => throw new PlatformNotSupportedException();
        [System.CLSCompliantAttribute(false)]
        public unsafe void SetLocalSignature(byte* localSignature, int signatureSize) => throw new PlatformNotSupportedException();
        public void SetLocalSignature(byte[] localSignature) => throw new PlatformNotSupportedException();
    }

    public readonly partial struct EventToken : System.IEquatable<EventToken>
    {
        public static readonly System.Reflection.Emit.EventToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.EventToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.EventToken a, System.Reflection.Emit.EventToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.EventToken a, System.Reflection.Emit.EventToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct ParameterToken : System.IEquatable<ParameterToken>
    {
        public static readonly System.Reflection.Emit.ParameterToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.ParameterToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.ParameterToken a, System.Reflection.Emit.ParameterToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.ParameterToken a, System.Reflection.Emit.ParameterToken b) { throw new PlatformNotSupportedException(); }
    }

    public readonly partial struct PropertyToken : System.IEquatable<PropertyToken>
    {
        public static readonly System.Reflection.Emit.PropertyToken Empty;
        public int Token { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Reflection.Emit.PropertyToken obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Reflection.Emit.PropertyToken a, System.Reflection.Emit.PropertyToken b) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Reflection.Emit.PropertyToken a, System.Reflection.Emit.PropertyToken b) { throw new PlatformNotSupportedException(); }
    }
}

#endif
