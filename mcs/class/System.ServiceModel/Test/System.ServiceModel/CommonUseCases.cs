//
// CommonUseCases.cs
//
// Author:
//	Eyal Alaluf
//
// Copyright (C) 2008 Mainsoft Co. http://www.mainsoft.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	// Use case with one endpoint configuration.
	[ServiceContract]
	public interface ICtorUseCase1
	{
		[OperationContract]
		string Echo (string msg);
	}

	// Use case with multiple endpoint configurations.
	[ServiceContract(ConfigurationName = "CtorUseCase2")]
	public interface ICtorUseCase2
	{
		[OperationContract]
		string Echo (string msg);
	}

	// Use case without endpoint configuration.
	[ServiceContract(ConfigurationName = "CtorUseCase3")]
	public interface ICtorUseCase3
	{
		[OperationContract]
		string Echo (string msg);
	}
}
