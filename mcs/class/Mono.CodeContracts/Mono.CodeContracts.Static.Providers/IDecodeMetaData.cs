using System;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Providers
{
	public interface IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
	{
		Type System_Array { get; }

		Type System_Boolean { get; }

		Type System_Char { get; }

		Type System_DynamicallyTypedReference { get; }

		Type System_Double { get; }

		Type System_Int8 { get; }

		Type System_Int16 { get; }

		Type System_Int32 { get; }

		Type System_Int64 { get; }

		Type System_IntPtr { get; }

		Type System_Object { get; }

		Type System_RuntimeArgumentHandle { get; }

		Type System_RuntimeTypeHandle { get; }

		Type System_RuntimeMethodHandle { get; }

		Type System_RuntimeFieldHandle { get; }

		Type System_Single { get; }

		Type System_String { get; }

		Type System_Type { get; }

		Type System_UInt8 { get; }

		Type System_UInt16 { get; }

		Type System_Uint32 { get; }

		Type System_Uint64 { get; }

		Type System_UIntPtr { get; }

		Type System_Void { get; }

		string SharedContractClassAssembly { get; set; }

    	Type ReturnType(Method method);

    	IIndexable<Parameter> Parameters(Method method);

    	Parameter This(Method method);

   	 	string Name(Method method);

		string FullName(Method method);

    	string DeclaringMemberCanonicalName(Method method);

    	bool IsMain(Method method);

    	bool IsStatic(Method method);

    	bool IsPrivate(Method method);

    	bool IsProtected(Method method);

    	bool IsPublic(Method method);

    	bool IsVirtual(Method method);

    	bool IsNewSlot(Method method);

    	bool IsOverride(Method method);

    	bool IsSealed(Method method);

    	bool IsVoidMethod(Method method);

    	bool IsConstructor(Method method);

    	bool IsFinalizer(Method method);

    	bool IsDispose(Method method);

    	bool IsInternal(Method method);

    	bool IsVisibleOutsideAssembly(Method method);

    	bool IsVisibleOutsideAssembly(Property property);

    	bool IsVisibleOutsideAssembly(Event @event);

    	bool IsVisibleOutsideAssembly(Field method);

    	bool IsAbstract(Method method);

    	bool IsExtern(Method method);

    	bool IsPropertySetter(Method method);

    	bool IsPropertyGetter(Method method);

    	Property GetPropertyFromAccessor(Method method);

    	bool IsCompilerGenerated(Method method);

    	bool IsDebuggerNonUserCode(Method method);

    	bool IsDebuggerNonUserCode(Type t);

    	bool IsAutoPropertyMember(Method method);

    	bool IsAutoPropertySetter(Method method, out Field backingField);

    	bool IsCompilerGenerated(Type type);

    	bool IsNativeCpp(Type type);

    	Type DeclaringType(Method method);

    	Assembly DeclaringAssembly(Method method);

    	int MethodToken(Method method);

    	bool IsSpecialized(Method method);

    	bool IsSpecialized(Method method, ref IImmutableMap<Type, Type> specialization);

    	bool IsSpecialized(Method method, out IIndexable<Type> methodTypeArguments, out IIndexable<Type> typeTypeArguments);

    	bool IsGeneric(Method method, out IIndexable<Type> formals);

    	Method Unspecialized(Method method);

    	Method Specialize(Method method, Type[] methodTypeArguments, Type[] typeTypeArguments);

    	IEnumerable<Method> OverriddenMethods(Method method);

    	IEnumerable<Method> ImplementedMethods(Method method);

    	IEnumerable<Method> OverriddenAndImplementedMethods(Method method);

    	bool IsImplicitImplementation(Method method);

    	bool HasBody(Method method);

    	Result AccessMethodBody<Data, Result>(Method method, IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Data, Result> consumer, Data data);

    	bool Equal(Method m1, Method m2);

    	bool Equal(Type type1, Type type2);

    	bool IsAsVisibleAs(Method m1, Method m2);

    	bool IsAsVisibleAs(Type t, Method m);

    	bool IsVisibleFrom(Method m, Type t);

    	bool IsVisibleFrom(Field f, Type t);

    	bool IsVisibleFrom(Type t, Type tfrom);

    	bool TryGetImplementingMethod(Type type, Method baseMethod, out Method implementingMethod);

    	IIndexable<Local> Locals(Method method);

    	bool TryGetRootMethod(Method method, out Method rootMethod);

    	Type FieldType(Field field);

    	string FullName(Field field);

    	string Name(Field field);

    	bool IsStatic(Field field);

    	bool IsPrivate(Field field);

    	bool IsProtected(Field field);

    	bool IsPublic(Field field);

    	bool IsInternal(Field field);

    	bool IsVolatile(Field field);

    	bool IsNewSlot(Field field);

    	Type DeclaringType(Field field);

    	bool IsReadonly(Field field);

    	bool IsAsVisibleAs(Field field, Method method);

    	bool IsSpecialized(Field field);

    	Field Unspecialized(Field field);

    	bool TryInitialValue(Field field, out object value);

    	bool Equal(Field f1, Field f2);

    	bool IsCompilerGenerated(Field f);

    	string Name(Property property);

    	bool HasGetter(Property property, out Method getter);

    	bool HasSetter(Property property, out Method setter);

    	IEnumerable<Property> Properties(Type type);

    	bool IsStatic(Property p);

    	bool IsOverride(Property p);

    	bool IsNewSlot(Property p);

    	bool IsSealed(Property p);

    	Type DeclaringType(Property p);

    	Type PropertyType(Property property);

    	bool Equal(Property p1, Property p2);

    	string Name(Event @event);

    	bool HasAdder(Event @event, out Method adder);

    	bool HasRemover(Event @event, out Method remover);

    	IEnumerable<Event> Events(Type type);

    	bool IsStatic(Event e);

    	bool IsOverride(Event e);

    	bool IsNewSlot(Event e);

    	bool IsSealed(Event e);

    	Type DeclaringType(Event e);

    	Type HandlerType(Event e);

    	bool Equal(Event e1, Event e2);

    	bool IsEventAdder(Method method, out Event @event);

    	bool IsEventRemover(Method method, out Event @event);

    	bool IsVoid(Type type);

    	bool IsUnmanagedPointer(Type type);

    	bool IsManagedPointer(Type type);

    	bool IsPrimitive(Type type);

    	bool IsStruct(Type type);

    	bool IsArray(Type type);

    	int Rank(Type type);

    	bool IsInterface(Type type);

    	bool IsStatic(Type type);

    	bool IsClass(Type type);

    	bool IsAbstract(Type type);

    	bool IsPublic(Type type);

    	bool IsInternal(Type type);

    	bool IsProtected(Type type);

    	bool IsPrivate(Type type);

    	bool IsSealed(Type type);

    	bool IsNested(Type type, out Type parentType);

    	bool IsReferenceConstrained(Type type);

    	bool IsValueConstrained(Type type);

    	bool IsConstructorConstrained(Type type);

    	bool IsFormalTypeParameter(Type type);

    	bool IsMethodFormalTypeParameter(Type type);

    	bool IsModified(Type type, out Type modified, out IIndexable<Pair<bool, Type>> modifiers);

    	bool IsGeneric(Type type, out IIndexable<Type> formals, bool normalized);

    	bool IsDelegate(Type type);

    	int NormalizedFormalTypeParameterIndex(Type type);

    	Type FormalTypeParameterDefiningType(Type type);

    	IIndexable<Type> NormalizedActualTypeArguments(Type type);

    	IIndexable<Type> ActualTypeArguments(Method method);

    	int MethodFormalTypeParameterIndex(Type type);

    	Method MethodFormalTypeDefiningMethod(Type type);

    	bool IsEnum(Type type);

    	bool HasFlagsAttribute(Type type);

    	Type TypeEnum(Type type);

    	Guid DeclaringModule(Type type);

    	string DeclaringModuleName(Type type);

    	IEnumerable<Field> Fields(Type type);

    	IEnumerable<Method> Methods(Type type);

    	IEnumerable<Type> NestedTypes(Type type);

    	string Name(Type type);

    	string FullName(Type type);

    	string Namespace(Type type);

    	Type ArrayType(Type type, int rank);

    	Type ManagedPointer(Type type);

    	Type UnmanagedPointer(Type type);

    	Type ElementType(Type type);

    	int TypeSize(Type type);

    	bool HasBaseClass(Type type);

    	Type BaseClass(Type type);

    	IEnumerable<Type> Interfaces(Type type);

    	IEnumerable<Type> TypeParameterConstraints(Type type);

    	bool IsSpecialized(Type type, out IIndexable<Type> typeArguments);

    	bool NormalizedIsSpecialized(Type type, out IIndexable<Type> typeArguments);

    	Type Unspecialized(Type type);

    	bool IsReferenceType(Type type);

    	bool IsNativePointerType(Type declaringType);

    	bool DerivesFrom(Type sub, Type super);

    	bool DerivesFromIgnoringTypeArguments(Type sub, Type super);

    	int ConstructorsCount(Type type);

    	bool IsVisibleOutsideAssembly(Type type);

    	Type Specialize(Type type, Type[] typeArguments);

    	bool TryLoadAssembly(string fileName, IDictionary assemblyCache, Action<CompilerError> errorHandler, out Assembly assembly, bool legacyContractMode, List<string> referencedAssemblies);

    	IEnumerable<Type> GetTypes(Assembly assembly);

    	IEnumerable<Assembly> AssemblyReferences(Assembly assembly);

    	string Name(Assembly assembly);

    	Version Version(Assembly assembly);

    	Guid AssemblyGuid(Assembly assembly);

    	bool TryGetSystemType(string fullName, out Type type);

	    string Name(Parameter param);

    	Type ParameterType(Parameter p);

    	bool IsOut(Parameter p);

    	int ArgumentIndex(Parameter p);

    	int ArgumentStackIndex(Parameter p);

    	Method DeclaringMethod(Parameter p);

    	string Name(Local local);

    	Type LocalType(Local local);

    	IEnumerable<Attribute> GetAttributes(Type type);

    	IEnumerable<Attribute> GetAttributes(Method method);

    	IEnumerable<Attribute> GetAttributes(Field field);

    	IEnumerable<Attribute> GetAttributes(Property field);

    	IEnumerable<Attribute> GetAttributes(Event @event);

    	IEnumerable<Attribute> GetAttributes(Assembly assembly);

    	IEnumerable<Attribute> GetAttributes(Parameter parameter);

    	Type AttributeType(Attribute attribute);

    	Method AttributeConstructor(Attribute attribute);

    	IIndexable<object> PositionalArguments(Attribute attribute);

    	object NamedArgument(string name, Attribute attribute);

    	void AddLibPath(string path);

    	void AddResolvedPath(string path);
	}
}

