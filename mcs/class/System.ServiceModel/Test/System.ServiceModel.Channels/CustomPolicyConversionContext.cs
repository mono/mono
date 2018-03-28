//
// CustomPolicyConversionContext.cs
//
// Author:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	class CustomPolicyConversionContext : PolicyConversionContext
	{
		PolicyAssertionCollection binding_assertions = new PolicyAssertionCollection ();
		BindingElementCollection binding_elements = new BindingElementCollection ();

		public CustomPolicyConversionContext ()
			: base (new ServiceEndpoint (new ContractDescription ("FakeContract")))
		{
		}

		public override PolicyAssertionCollection GetBindingAssertions ()
		{
			return binding_assertions;
		}

		public override PolicyAssertionCollection GetFaultBindingAssertions (FaultDescription fault)
		{
			return binding_assertions;
		}

		public override PolicyAssertionCollection GetMessageBindingAssertions (MessageDescription message)
		{
			return binding_assertions;
		}

		public override PolicyAssertionCollection GetOperationBindingAssertions (OperationDescription operation)
		{
			return binding_assertions;
		}

		public override BindingElementCollection BindingElements {
			get {
				return binding_elements;
			}
		}

	}
}
#endif
