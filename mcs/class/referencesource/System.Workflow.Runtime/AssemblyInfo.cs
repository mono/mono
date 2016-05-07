using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Workflow.ComponentModel.Serialization;

#pragma warning disable 618
[assembly: SecurityPermissionAttribute(SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.Execution | SecurityPermissionFlag.SerializationFormatter)]
#pragma warning restore 618

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Configuration")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Hosting")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Tracking")]

[assembly: XmlnsPrefix("http://schemas.microsoft.com/winfx/2006/xaml/workflow", "wf")]
[assembly: InternalsVisibleTo("System.WorkflowServices, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.ServiceModel.Activities, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

