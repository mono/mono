//
// HelperClasses_ITRS.cs : Various ITypeResolutionService implementations 
// for use during testing.
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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

using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace MonoTests.System.Resources {
	public class DummyITRS : ITypeResolutionService {
		public Assembly GetAssembly (AssemblyName name, bool throwOnError)
		{
			return null;
		}

		public Assembly GetAssembly (AssemblyName name)
		{
			return null;
		}

		public string GetPathOfAssembly (AssemblyName name)
		{
			return null;
		}

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			return null;
		}

		public Type GetType (string name, bool throwOnError)
		{
			return null;
		}

		public Type GetType (string name)
		{
			return null;
		}

		public void ReferenceAssembly (AssemblyName name)
		{

		}
	}

	public class ReturnSerializableSubClassITRS : ITypeResolutionService {
		public Assembly GetAssembly (AssemblyName name, bool throwOnError)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Assembly GetAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public string GetPathOfAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			return typeof (serializableSubClass);
		}

		public Type GetType (string name, bool throwOnError)
		{
			return typeof (serializableSubClass);
		}

		public Type GetType (string name)
		{
			return typeof (serializableSubClass);
		}

		public void ReferenceAssembly (AssemblyName name)
		{

		}

	}

	public class ReturnIntITRS : ITypeResolutionService {
		public Assembly GetAssembly (AssemblyName name, bool throwOnError)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Assembly GetAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public string GetPathOfAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			return typeof (Int32);
		}

		public Type GetType (string name, bool throwOnError)
		{
			return typeof (Int32);
		}

		public Type GetType (string name)
		{
			return typeof (Int32);
		}

		public void ReferenceAssembly (AssemblyName name)
		{

		}
	}

	public class ExceptionalITRS : ITypeResolutionService {
		public Assembly GetAssembly (AssemblyName name, bool throwOnError)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Assembly GetAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public string GetPathOfAssembly (AssemblyName name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Type GetType (string name, bool throwOnError)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public Type GetType (string name)
		{
			throw new NotImplementedException ("I was accessed");
		}

		public void ReferenceAssembly (AssemblyName name)
		{

		}
	}

}
