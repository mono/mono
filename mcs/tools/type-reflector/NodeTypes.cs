//
// NodeTypes.cs: The types of nodes that can be displayed.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

namespace Mono.TypeReflector
{
	/// <summary>
	/// The types of Nodes that can be displayed.
	/// </summary>
	public enum NodeTypes {
		// Meta-nodes
		Assembly,
		Library,
		Namespace,
		Module,
		// Type information
		CustomAttributeProvider,
		Type,
		BaseType,
		Interface,
		Field,
		Constructor,
		Method,
		Parameter,
		Property,
		Event,
		// Misc
		ReturnValue,	// needed?  could be simulated
		Alias,		// Refer's to a previous node.  Description is the prior object
		Other
	}
}

