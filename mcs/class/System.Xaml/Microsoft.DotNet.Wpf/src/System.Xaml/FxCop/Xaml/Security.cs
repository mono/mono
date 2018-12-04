//
// FxCop Violation Message Suppressions
//  Approved List
//

using System.Diagnostics.CodeAnalysis;

// Begin

[module: SuppressMessage("Microsoft.Security","CA2103:ReviewImperativeSecurity", Scope="member", Target="System.Xaml.Permissions.XamlLoadPermission.#Copy()", Justification="Reviewed by senior CLR security developer.")]

[module: SuppressMessage("Microsoft.Security","CA2103:ReviewImperativeSecurity", Scope="member", Target="MS.Internal.Xaml.Runtime.DynamicMethodRuntime.#.ctor(MS.Internal.Xaml.Runtime.XamlRuntimeSettings,System.Xaml.XamlSchemaContext,System.Xaml.Permissions.XamlAccessLevel)", Justification="Reviewed by Microsoft.")]

[module: SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.Xaml.XamlObjectReader+ObjectMarkupInfo.#GetInstanceDescriptorInfo(System.ComponentModel.Design.Serialization.InstanceDescriptor,System.Reflection.MemberInfo&,System.Collections.ICollection&,System.Boolean&)", Justification="Non-issue since C# 2.0. LinkDemand is FullDemand by default without a SecurityCritical attribute.")]

[module: SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Scope="member", Target="MS.Internal.Utility.PerfServiceProxy.#InitializeGetId()", Justification="Doesn't make sense with security transparency system. Reviewed by Microsoft")]

// End

