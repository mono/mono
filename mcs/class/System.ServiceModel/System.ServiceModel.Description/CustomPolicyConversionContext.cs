//
// CustomPolicyConversionContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Xml;
using System.ServiceModel.Channels;
using WS = System.Web.Services.Description;

namespace System.ServiceModel.Description {

	internal class CustomPolicyConversionContext : PolicyConversionContext {
		WS.Binding binding;
		PolicyAssertionCollection assertions;
		BindingElementCollection binding_elements;

		internal WS.Binding WsdlBinding {
			get { return binding; }
		}

		#region implemented abstract members of PolicyConversionContext

		public override PolicyAssertionCollection GetBindingAssertions ()
		{
			return assertions;
		}

		public override PolicyAssertionCollection GetFaultBindingAssertions (FaultDescription fault)
		{
			throw new NotImplementedException ();
		}

		public override PolicyAssertionCollection GetMessageBindingAssertions (MessageDescription message)
		{
			throw new NotImplementedException ();
		}

		public override PolicyAssertionCollection GetOperationBindingAssertions (OperationDescription operation)
		{
			throw new NotImplementedException ();
		}

		public override BindingElementCollection BindingElements {
			get { return binding_elements; }
		}

		#endregion

		public CustomPolicyConversionContext (WS.Binding binding, ServiceEndpoint endpoint)
			: base (endpoint)
		{
			this.binding = binding;
			assertions = new PolicyAssertionCollection ();
			binding_elements = ((CustomBinding)endpoint.Binding).Elements;
		}

		public CustomPolicyConversionContext (ServiceEndpoint endpoint)
			: base (endpoint)
		{
			assertions = new PolicyAssertionCollection ();
			binding_elements = endpoint.Binding.CreateBindingElements ();
		}

		public void AddPolicyAssertion (XmlElement element)
		{
			/*
			 * http://www.w3.org/Submission/WS-Policy/#Policy_Assertion:
			 *
			 * <wsp:Policy … >
			 *   <wsp:ExactlyOne>
			 *     ( <wsp:All> ( <Assertion …> … </Assertion> )* </wsp:All> )*
			 *   </wsp:ExactlyOne>
			 * </wsp:Policy> 
			 * 
			 */

			var exactlyOne = element.FirstChild as XmlElement;
			if (exactlyOne == null) {
				// OOPS
				return;
			}

			if (!exactlyOne.NamespaceURI.Equals (Constants.WspNamespace) ||
				!exactlyOne.LocalName.Equals ("ExactlyOne")) {
				// FIXME: What to do with this ... ?
				return;
			}

			foreach (var node in exactlyOne.ChildNodes) {
				var child = node as XmlElement;
				if (child == null)
					continue;

				if (!child.NamespaceURI.Equals (Constants.WspNamespace) ||
				    !child.LocalName.Equals ("All")) {
					// FIXME: Can assertions go here ... ?
					continue;
				}

				foreach (var node2 in child.ChildNodes) {
					var assertion = node2 as XmlElement;
					if (assertion == null)
						continue;

					assertions.Add (assertion);
				}
			}
		}
	}
}

