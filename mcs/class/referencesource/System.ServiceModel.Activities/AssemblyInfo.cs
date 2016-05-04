using System.Runtime.CompilerServices;
using System.Windows.Markup;

// Define XAML namespace mappings
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/servicemodel", "System.ServiceModel")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/servicemodel", "System.ServiceModel.Activities")]

// Friends Assembly
// Need to provide InternalsVisibleTo System.Runtime.Serialization to allow serialization of internal DataContracts/DataMembers in partial trust.
[assembly: InternalsVisibleTo("System.Runtime.Serialization, PublicKey=00000000000000000400000000000000")]

// Partial Trust :
