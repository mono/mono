//
// ExplicitTypeDisplayer.cs: Displays type information as a tree
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ExplicitTypeDisplayer : IndentingTypeDisplayer {

		public ExplicitTypeDisplayer (TextWriter writer)
			: base (writer)
		{
		}

		private void PrintMemberInfo (MemberInfo mi)
		{
			WriteLine ("DeclaringType={0}", mi.DeclaringType);
			WriteLine ("MemberType={0}", mi.MemberType);
			WriteLine ("Name={0}", mi.Name);
			WriteLine ("ReflectedType={0}", mi.ReflectedType);
			/*
			 * Not liked by Constructors: on type MethodsEventHandler
			WriteLine ("GetCustomAttributes():");
			using (Indenter n1 = GetIndenter()) {
				object[] attrs = mi.GetCustomAttributes (true);
				foreach (object a in attrs)
					WriteLine (a);
			}
			 */
		}

		protected override void OnIndentedType (TypeEventArgs e)
		{
			WriteLine (GetTypeHeader (e.Type));
			if (ShowTypeProperties) {
				using (Indenter n1 = GetIndenter()) {
					WriteLine ("System.Type Properties:");
					using (Indenter n2 = GetIndenter())
						PrintType (e.Type);
				}
			}
		}

		protected override void OnIndentedBaseType (BaseTypeEventArgs e)
		{
			WriteLine ("Base:");
			using (Indenter n1 = GetIndenter()) {
				WriteLine (e.BaseType);
			}
		}

		protected void PrintType (Type i)
		{
			PrintMemberInfo (i);
			WriteLine ("Delimiter={0}", Type.Delimiter);
			if (ShowMonoBroken)
				WriteLine ("EmptyTypes={0}", Type.EmptyTypes.ToString());
			WriteLine ("FilterAttribute={0}", Type.FilterAttribute);
			WriteLine ("FilterName={0}", Type.FilterName);
			WriteLine ("FilterNameIgnoreCase={0}", Type.FilterNameIgnoreCase);
			if (ShowMonoBroken)
				WriteLine ("Missing={0}", Type.Missing);

			if (ShowMonoBroken)
				WriteLine ("Assembly={0}", i.Assembly);
			using (Indenter n1 = GetIndenter()) {
				if (ShowMonoBroken)
					WriteLine ("CodeBase={0}", i.Assembly.CodeBase);
				WriteLine ("EntryPoint={0}", i.Assembly.EntryPoint);
#if MONO_BROKEN
				if (ShowMonoBroken)
					WriteLine ("EscapedCodeBase={0}", i.Assembly.EscapedCodeBase);
#endif
				WriteLine ("Evidence={0}", i.Assembly.Evidence);
				if (ShowMonoBroken)
					WriteLine ("FullName={0}", i.Assembly.FullName);
#if MONO_BROKEN
				if (ShowMonoBroken)
					WriteLine ("GlobalAssemblyCache={0}", i.Assembly.GlobalAssemblyCache);
#endif
				WriteLine ("Location={0}", i.Assembly.Location);
			}
			if (ShowMonoBroken)
				WriteLine ("AssemblyQualifiedName={0}", i.AssemblyQualifiedName);
			WriteLine ("Attributes={0}", 
				GetEnumDescription (typeof(TypeAttributes), i.Attributes));
			WriteLine ("BaseType={0}", i.BaseType);
			WriteLine ("DeclaringType={0}", i.DeclaringType);
			WriteLine ("DefaultBinder={0}", Type.DefaultBinder);
			WriteLine ("FullName={0}", i.FullName);
			WriteLine ("GUID={0}", i.GUID);
			WriteLine ("HasElementType={0}", i.HasElementType);
			WriteLine ("IsAbstract={0}", i.IsAbstract);
			WriteLine ("IsAnsiClass={0}", i.IsAnsiClass);
			WriteLine ("IsArray={0}", i.IsArray);
			WriteLine ("IsAutoClass={0}", i.IsAutoClass);
			WriteLine ("IsAutoLayout={0}", i.IsAutoLayout);
			WriteLine ("IsByRef={0}", i.IsByRef);
			WriteLine ("IsClass={0}", i.IsClass);
			WriteLine ("IsCOMObject={0}", i.IsCOMObject);
			WriteLine ("IsContextful={0}", i.IsContextful);
			WriteLine ("IsEnum={0}", i.IsEnum);
			WriteLine ("IsExplicitLayout={0}", i.IsExplicitLayout);
			WriteLine ("IsImport={0}", i.IsImport);
			WriteLine ("IsInterface={0}", i.IsInterface);
			WriteLine ("IsLayoutSequential={0}", i.IsLayoutSequential);
			WriteLine ("IsMarshalByRef={0}", i.IsMarshalByRef);
			WriteLine ("IsNestedAssembly={0}", i.IsNestedAssembly);
			WriteLine ("IsNestedFamORAssem={0}", i.IsNestedFamORAssem);
			WriteLine ("IsNestedPrivate={0}", i.IsNestedPrivate);
			WriteLine ("IsNotPublic={0}", i.IsNotPublic);
			WriteLine ("IsPointer={0}", i.IsPointer);
			WriteLine ("IsPrimitive={0}", i.IsPrimitive);
			WriteLine ("IsPublic={0}", i.IsPublic);
			WriteLine ("IsSealed={0}", i.IsSealed);
			WriteLine ("IsSerializable={0}", i.IsSerializable);
			WriteLine ("IsSpecialName={0}", i.IsSpecialName);
			WriteLine ("IsUnicodeClass={0}", i.IsUnicodeClass);
			WriteLine ("IsValueType={0}", i.IsValueType);
			WriteLine ("Module={0}", i.Module);
			WriteLine ("Namespace={0}", i.Namespace);
			WriteLine ("TypeHandle={0}", i.TypeHandle);
			if (ShowMonoBroken)
				WriteLine ("TypeInitializer={0}", i.TypeInitializer);
			WriteLine ("UnderlyingSystemType={0}", i.UnderlyingSystemType);
		}

		protected override void OnIndentedInterfaces (InterfacesEventArgs e)
		{ 
			WriteLine ("Interfaces:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Interfaces.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (Type i in e.Interfaces) {
					WriteLine (i);
					if (VerboseOutput) {
						using (Indenter n3 = GetIndenter()) {
							PrintType (i);
						}
					}
				}
			}
		}

		protected void PrintFieldInfo (FieldInfo f)
		{
			if (VerboseOutput) {
				PrintMemberInfo (f);
				WriteLine ("Attributes={0}", 
					GetEnumDescription (typeof(FieldAttributes), f.Attributes));
				WriteLine ("FieldHandle={0}", f.FieldHandle);
				WriteLine ("FieldType={0}", f.FieldType);
				WriteLine ("IsAssembly={0}", f.IsAssembly);
				WriteLine ("IsFamily={0}", f.IsFamily);
				WriteLine ("IsFamilyAndAssembly={0}", f.IsFamilyAndAssembly);
				WriteLine ("IsFamilyOrAssembly={0}", f.IsFamilyOrAssembly);
				WriteLine ("IsInitOnly={0}", f.IsInitOnly);
				WriteLine ("IsLiteral={0}", f.IsLiteral);
				WriteLine ("IsNotSerialized={0}", f.IsNotSerialized);
				WriteLine ("IsPinvokeImpl={0}", f.IsPinvokeImpl);
				WriteLine ("IsPrivate={0}", f.IsPrivate);
				WriteLine ("IsPublic={0}", f.IsPublic);
				WriteLine ("IsSpecialName={0}", f.IsSpecialName);
				WriteLine ("IsStatic={0}", f.IsStatic);
				if (f.IsStatic && ((f.Attributes & FieldAttributes.HasDefault) != 0)) {
					WriteLine ("Default Value: {0}", FieldValue (f));
				}
			}
		}

		protected override void OnIndentedFields (FieldsEventArgs e)
		{
			WriteLine ("Fields:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Fields.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (FieldInfo f in e.Fields) {
					WriteLine (f);
					using (Indenter n3 = GetIndenter()) {
						PrintFieldInfo (f);
					}
				}
			}
		}

		protected void PrintPropertyInfo (PropertyInfo p)
		{
			if (VerboseOutput) {
				PrintMemberInfo (p);
				WriteLine ("Attributes={0}", 
					GetEnumDescription (typeof(PropertyAttributes), p.Attributes));
				WriteLine ("CanRead={0}", p.CanRead);
				WriteLine ("CanWrite={0}", p.CanWrite);
				WriteLine ("IsSpecialName={0}", p.IsSpecialName);
				WriteLine ("PropertyType={0}", p.PropertyType);
			}
		}

		protected override void OnIndentedProperties (PropertiesEventArgs e)
		{
			WriteLine ("Properties:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Properties.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (PropertyInfo p in e.Properties) {
					WriteLine (p);
					using (Indenter n3 = GetIndenter()) {
						PrintPropertyInfo (p);
					}
				}
			}
		}

		protected void PrintEventInfo (EventInfo i)
		{
			if (VerboseOutput) {
				PrintMemberInfo (i);
				WriteLine ("Attributes={0}", 
					GetEnumDescription (typeof(EventAttributes), i.Attributes));
				WriteLine ("EventHandlerType={0}", i.EventHandlerType);
				WriteLine ("IsMulticast={0}", i.IsMulticast);
				WriteLine ("IsSpecialName={0}", i.IsSpecialName);
			}
		}

		protected override void OnIndentedEvents (EventsEventArgs e)
		{
			WriteLine ("Events:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Events.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (EventInfo i in e.Events) {
					WriteLine (i);
					using (Indenter n3 = GetIndenter()) {
						PrintEventInfo (i);
					}
				}
			}
		}

		private void PrintMethodBase (MethodBase mb)
		{
			PrintMemberInfo (mb);
			WriteLine ("Attributes={0}", 
				GetEnumDescription (typeof(MethodAttributes), mb.Attributes));
			WriteLine ("CallingConvention={0}", mb.CallingConvention);
			WriteLine ("IsAbstract={0}", mb.IsAbstract);
			WriteLine ("IsAssembly={0}", mb.IsAssembly);
			WriteLine ("IsConstructor={0}", mb.IsConstructor);
			WriteLine ("IsFamily={0}", mb.IsFamily);
			WriteLine ("IsFamilyAndAssembly={0}", mb.IsFamilyAndAssembly);
			WriteLine ("IsFamilyOrAssembly={0}", mb.IsFamilyOrAssembly);
			WriteLine ("IsFinal={0}", mb.IsFinal);
			WriteLine ("IsHideBySig={0}", mb.IsHideBySig);
			WriteLine ("IsPrivate={0}", mb.IsPrivate);
			WriteLine ("IsPublic={0}", mb.IsPublic);
			WriteLine ("IsSpecialName={0}", mb.IsSpecialName);
			WriteLine ("IsStatic={0}", mb.IsStatic);
			WriteLine ("IsVirtual={0}", mb.IsVirtual);
			WriteLine ("MethodHandle={0}", mb.MethodHandle);
		}

		protected void PrintConstructorInfo (ConstructorInfo c)
		{
			if (VerboseOutput) {
				PrintMethodBase (c);
			}
		}

		protected override void OnIndentedConstructors (ConstructorsEventArgs e)
		{
			WriteLine ("Constructors:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Constructors.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (ConstructorInfo c in e.Constructors) {
					WriteLine (c);
					using (Indenter n3 = GetIndenter()) {
						PrintConstructorInfo (c);
					}
				}
			}
		}

		protected void PrintMethodInfo (MethodInfo m)
		{
			if (VerboseOutput) {
				PrintMethodBase (m);
				WriteLine ("ReturnType={0}", m.ReturnType);
				WriteLine ("ReturnTypeCustomAttributes={0}", 
					/* GetEnumDescription (typeof(m.ReturnTypeCustomAttributes), m.ReturnTypeCustomAttributes) */
					m.ReturnTypeCustomAttributes);
			}
		}

		protected override void OnIndentedMethods (MethodsEventArgs e)
		{
			WriteLine ("Methods:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Methods.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (MethodInfo m in e.Methods) {
					WriteLine (m);
					using (Indenter n3 = GetIndenter()) {
						PrintMethodInfo (m);
					}
				}
			}
		}
	}
}

