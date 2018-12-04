//
// FxCop Violation Message Suppressions
//  Approved List
//

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Reliability","CA2001:AvoidCallingProblematicMethods", Scope="member", Target="System.Xaml.Schema.ClrNamespace.#ParseClrNamespaceUri(System.String)", MessageId="System.Reflection.Assembly.LoadWithPartialName", Justification="Back compat.")]

[module: SuppressMessage("Microsoft.Reliability","CA2001:AvoidCallingProblematicMethods", MessageId="System.Reflection.Assembly.LoadFile", Scope="member", Target="System.Xaml.ReflectionHelper.#LoadAssemblyHelper(System.String,System.String)")]

[module: SuppressMessage("Microsoft.Reliability","CA2001:AvoidCallingProblematicMethods", MessageId="System.Reflection.Assembly.LoadWithPartialName", Scope="member", Target="System.Xaml.XamlSchemaContext.#ResolveAssembly(System.String)", Justification="Need to support load of assemblies from GAC by short name.")]
