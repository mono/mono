#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Newtonsoft.Json
{
	sealed class JavaScriptParameters : ReadOnlyCollection<object>
    {
		public static readonly JavaScriptParameters Empty = new JavaScriptParameters(new List<object>());

		public JavaScriptParameters(IList<object> list)
			: base(list)
		{
		}
    }
}