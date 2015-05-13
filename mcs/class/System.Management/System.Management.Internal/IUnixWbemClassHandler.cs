//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Management
{
	/// <summary>
	/// Unix wbem class handler interface
	/// </summary>
	internal interface IUnixWbemClassHandler
	{
		string PathField { get; }

		IUnixWbemClassHandler New();

		/// <summary>
		/// Get this instance.
		/// </summary>
		IEnumerable<object> Get(string strQuery);
	
		/// <summary>
		/// Get the specified nativeObj.
		/// </summary>
		/// <param name='nativeObj'>
		/// Native object.
		/// </param>
		object Get(object nativeObj);

		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
		void AddProperty (string key, object obj);

		/// <summary>
		/// Gets the property.
		/// </summary>
		/// <returns>
		/// The property.
		/// </returns>
		/// <param name='key'>
		/// Key.
		/// </param>
		object GetProperty(string key);

		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <returns>
		/// The method.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		IUnixWbemClassHandler InvokeMethod(string methodName, IUnixWbemClassHandler obj);

		IUnixWbemClassHandler WithProperty(string key, object obj);

		IUnixWbemClassHandler WithMethod(string key, UnixCimMethodInfo methodInfo);

		/// <summary>
		/// Adds the method.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='method'>
		/// Method.
		/// </param>
		void AddMethod (string key, UnixCimMethodInfo method);

		
		UnixWbemQualiferInfo GetQualifier(string name);

		UnixWbemQualiferInfo GetQualifier(int index);

		IEnumerable<string> QualifierNames { get; }

		IDictionary<string, object> Properties { get; }

		IEnumerable<string> PropertyNames { get; }

		IEnumerable<string> SystemPropertyNames { get; }

		IEnumerable<UnixWbemPropertyInfo> PropertyInfos { get; }

		IEnumerable<UnixWbemPropertyInfo> SystemPropertyInfos { get; }

		IEnumerable<string> MethodNames { get; }
		
		IEnumerable<UnixCimMethodInfo> Methods { get; }

		UnixCimMethodInfo NextMethod();
	}
}
