using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Diagnostics.CodeAnalysis;

[assembly: XmlnsDefinition(NameSpaces.Toolbox, "System.Activities.Presentation.Toolbox")]
[assembly: XmlnsDefinition(NameSpaces.Design, "System.Activities.Presentation")]
[assembly: XmlnsDefinition(NameSpaces.Design, "System.Activities.Presentation.View")]
[assembly: XmlnsDefinition(NameSpaces.Design2010, "System.Activities.Presentation.Annotations")]
[assembly: XmlnsDefinition(NameSpaces.Design2010, "System.Activities.Presentation.Expressions")]
[assembly: XmlnsDefinition(NameSpaces.Design2010, "System.Activities.Presentation.ViewState")]
[assembly: XmlnsPrefix(NameSpaces.Design, NameSpaces.DesignPrefix)]
[assembly: XmlnsPrefix(NameSpaces.Design2010, NameSpaces.Design2010Prefix)]
[assembly: XmlnsPrefix(NameSpaces.Mc, NameSpaces.McPrefix)]

[assembly: InternalsVisibleTo("CIT.System.Activities.Core.Design, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.Activities.Core.Presentation, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.Activities.Presentation.UnitTest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

[assembly: ThemeInfo(ResourceDictionaryLocation.SourceAssembly, ResourceDictionaryLocation.SourceAssembly)]
[assembly: SuppressMessage("Microsoft.MSInternal", "CA904:DeclareTypesInMicrosoftOrSystemNamespace", Scope = "namespace", Target = "XamlGeneratedNamespace", Justification = "Xaml Generated")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "namespace", Target = "System.Activities.Presentation.Sqm", Justification = "False positive for SQM")]
