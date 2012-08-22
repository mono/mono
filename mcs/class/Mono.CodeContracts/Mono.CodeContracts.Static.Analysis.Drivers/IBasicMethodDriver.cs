// 
// IBasicMethodDriver.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
	
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Drivers 
{
	interface IBasicMethodDriverMultiParam<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, LogOptions> where Type : IEquatable<Type> where LogOptions : IFrameworkLogOptions
	{
	    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Unit, Unit, IMethodContext<Field, Method>, Unit> RawLayer { get; }

	    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, int, int, IStackContext<Field, Method>, Unit> StackLayer { get; }

	    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Unit, Unit, IMethodContext<Field, Method>, Unit> ContractFreeRawLayer { get; }

	    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, int, int, IStackContext<Field, Method>, Unit> ContractFreeStackLayer { get; }

	    LogOptions Options { get; }

	    ICFG ContractFreeCFG { get; }

	    SyntacticComplexity SyntacticComplexity { get; set; }

	    IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, LogOptions> AnalysisDriver { get; }
	}
}