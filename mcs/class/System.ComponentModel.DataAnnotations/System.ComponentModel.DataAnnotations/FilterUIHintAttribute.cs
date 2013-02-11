//
// FilterUIHintAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
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

#if NET_4_0

using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class FilterUIHintAttribute : Attribute
	{
		readonly ControlParameters controlParameters;

		public FilterUIHintAttribute (string filterUIHint)
			: this (filterUIHint, null, null)
		{
		}

		public FilterUIHintAttribute (string filterUIHint, string presentationLayer)
			: this (filterUIHint, presentationLayer, null)
		{
		}

		public FilterUIHintAttribute (string filterUIHint, string presentationLayer, params object[] controlParameters)
		{
			FilterUIHint = filterUIHint;
			PresentationLayer = presentationLayer;	
			this.controlParameters = new ControlParameters (controlParameters);
		}

		public IDictionary<string, object> ControlParameters {
			get {
				return controlParameters.Dictionary;
			}
		}
		
		public string FilterUIHint { get; private set; }

		public string PresentationLayer { get; private set; }

		public override object TypeId {
			get {
				return this;
			}
		}

		public override int GetHashCode ()
		{
			return RuntimeHelpers.GetHashCode (FilterUIHint) ^
				RuntimeHelpers.GetHashCode (PresentationLayer);
		}

		public override bool Equals (object obj)
		{
			var fha = obj as FilterUIHintAttribute;
			if (fha == null)
				return false;

			return fha.FilterUIHint == FilterUIHint && 
				fha.PresentationLayer == PresentationLayer &&
				fha.controlParameters.Equals (controlParameters);
		}
	}
}

#endif