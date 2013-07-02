//
// UIHintAttribute.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = true)]
	public class UIHintAttribute : Attribute
	{
		readonly ControlParameters controlParameters;

		public UIHintAttribute (string uiHint)
			: this (uiHint, null)
		{
		}

		public UIHintAttribute (string uiHint, string presentationLayer)
			: this (uiHint, presentationLayer, null)
		{
		}

		public UIHintAttribute (string uiHint, string presentationLayer, params object [] controlParameters)
		{
			UIHint = uiHint;
			PresentationLayer = presentationLayer;
			this.controlParameters = new ControlParameters (controlParameters);
		}

		public IDictionary<string, object> ControlParameters {
			get {
				return controlParameters.Dictionary;
			}
		}

		public string PresentationLayer { get; private set; }

		public string UIHint { get; private set; }

#if NET_4_0
		public override object TypeId {
			get {
				return this;
			}
		}
#endif

		public override int GetHashCode ()
		{
			return RuntimeHelpers.GetHashCode (UIHint) ^
				RuntimeHelpers.GetHashCode (PresentationLayer);
		}

		public override bool Equals (object obj)
		{
			var ha = obj as UIHintAttribute;
			if (ha == null)
				return false;

			return ha.UIHint == UIHint && 
				ha.PresentationLayer == PresentationLayer &&
				ha.controlParameters.Equals (controlParameters);
		}
	}
}
