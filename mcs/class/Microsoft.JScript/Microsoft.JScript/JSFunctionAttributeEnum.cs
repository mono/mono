//
// JSFunctionAttributeEnum.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
		public enum JSFunctionAttributeEnum {
			HasArguments    = 0x01,
			HasThisObject   = 0x02,
			IsNested        = 0x04,
			HasStackFrame   = 0x08,
			HasVarArgs      = 0x10,
			HasEngine       = 0x20,
			IsExpandoMethod = 0x40,
			IsInstanceNestedClassConstructor = 0x80,
			ClassicFunction = HasArguments | HasThisObject | HasEngine,
			NestedFunction  = HasStackFrame | IsNested | HasEngine,      
			ClassicNestedFunction = ClassicFunction | NestedFunction,
		}
}