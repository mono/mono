
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Security;

using Mono;

namespace System.Reflection {

	abstract class RuntimeAssembly : Assembly
	{

	}


	class MonoAssembly : RuntimeAssembly
	{

	}
}


