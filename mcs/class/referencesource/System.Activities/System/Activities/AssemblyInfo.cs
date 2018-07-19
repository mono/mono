using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("97bdccfe-43bf-4c17-991d-c797c2ef2243")]

// Define XAML namespace mappings
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/activities", "System.Activities")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/activities", "System.Activities.Statements")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/activities", "System.Activities.Expressions")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/activities", "System.Activities.Validation")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/activities", "System.Activities.XamlIntegration")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger", "System.Activities.Debugger.Symbol")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger", "sads")]

// Friends Assembly
// TODO, 33059, Make common shim to access System.Activities's internal classes.
[assembly: InternalsVisibleTo("CDF.CIT.Scenarios.WorkflowModel.Debugger, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("CDF.CIT.Scenarios.WorkflowModel.Compensation, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("CDF.CIT.Scenarios.WorkflowModel.DataModel, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("CDF.CIT.Scenarios.Activities.Statements.StateMachine, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.Compensation, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
// Need to provide InternalsVisibleTo System.Runtime.Serialization to allow serialization of internal DataContracts/DataMembers in partial trust.
[assembly: InternalsVisibleTo("System.Runtime.Serialization, PublicKey=00000000000000000400000000000000")]
