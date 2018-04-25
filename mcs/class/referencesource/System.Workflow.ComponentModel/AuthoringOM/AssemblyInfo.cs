using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Workflow.ComponentModel.Serialization;

[assembly: InternalsVisibleTo("System.Workflow.Runtime, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.WorkflowServices, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("Microsoft.Workflow.Compiler, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.ServiceModel.Activities, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

[assembly: XmlnsDefinition(StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel")]
[assembly: XmlnsDefinition(StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel.Compiler")]
[assembly: XmlnsDefinition(StandardXomlKeys.Definitions_XmlNs, "System.Workflow.ComponentModel.Serialization")]
[assembly: XmlnsDefinition(StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel.Design")]

[assembly: XmlnsPrefix(StandardXomlKeys.WorkflowXmlNs, StandardXomlKeys.WorkflowPrefix)]
[assembly: XmlnsPrefix(StandardXomlKeys.Definitions_XmlNs, StandardXomlKeys.Definitions_XmlNs_Prefix)]

[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#ActivityContextGuidProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#CancelingEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#ClosedEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#CompensatingEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#ExecutingEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#FaultingEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Activity.#StatusChangedEvent")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.ActivityExecutionContext.#CurrentExceptionProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.CompensateActivity.#TargetActivityNameProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.FaultHandlerActivity.#FaultTypeProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.SuspendActivity.#ErrorProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.TerminateActivity.#ErrorProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.ThrowActivity.#FaultProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.ThrowActivity.#FaultTypeProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.WorkflowChanges.#ConditionProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.WorkflowParameterBinding.#ParameterNameProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.WorkflowParameterBinding.#ValueProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.WorkflowTransactionOptions.#IsolationLevelProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.WorkflowTransactionOptions.#TimeoutDurationProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.ActivityCodeDomSerializer.#MarkupFileNameProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.ActivityMarkupSerializer.#EndColumnProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.ActivityMarkupSerializer.#EndLineProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.ActivityMarkupSerializer.#StartColumnProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.ActivityMarkupSerializer.#StartLineProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.WorkflowMarkupSerializer.#ClrNamespacesProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.WorkflowMarkupSerializer.#EventsProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.WorkflowMarkupSerializer.#XClassProperty")]
[module: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Serialization.WorkflowMarkupSerializer.#XCodeProperty")]
