//
// ControlParameters.cs
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


using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
	sealed class ControlParameters : IEquatable<ControlParameters>
	{
		Dictionary<string, object> dictionary;
		readonly object[] parameters;

		public ControlParameters (object[] parameters)
		{
			this.parameters = parameters;
		}

		public Dictionary<string, object> Dictionary {
			get {
				return dictionary ?? (dictionary = CreateDictionary ());
			}
		}

		public bool Equals (ControlParameters other)
		{
			if (parameters == null || other.parameters == null)
				return ReferenceEquals (parameters, other.parameters);

			if (parameters.Length != other.parameters.Length)
				return false;

			try {
				return Dictionary.OrderBy (l => l.Key).SequenceEqual (other.Dictionary.OrderBy (l => l.Key));
			} catch (InvalidOperationException) {
				return false;
			}
		}

		Dictionary<string, object> CreateDictionary ()
		{
			if (parameters == null || parameters.Length == 0) {
				return new Dictionary<string, object> (0);
			}

			if (parameters.Length % 2 != 0)
				throw new InvalidOperationException ();

			var dict = new Dictionary<string, object> ();			
			for (int i = 0; i < parameters.Length; ) {
				var key = parameters [i++] as string;
				if (key == null)
					throw new InvalidOperationException ();

				try {
					dict.Add (key, parameters[i++]);
				} catch (System.ArgumentException) {
					throw new InvalidOperationException ();
				}
			}

			return dict;
		}
	}
}
