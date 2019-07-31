//
// Copyright (c) 2018 Microsoft Corp
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
using System.Text;

namespace Mono.WebAssembly.Build
{
	// custom base for template to strip out CodeDom dependency
	public abstract class TemplateBase
	{
		public abstract string TransformText ();
		public virtual void Initialize () { }

		StringBuilder builder = new StringBuilder ();

		public StringBuilder GenerationEnvironment {
			get => builder;
			set {
				if (value == null) {
					builder.Length = 0;
				} else {
					builder = value;
				}
			}
		}

		public ToStringInstanceHelper ToStringHelper { get; } = new ToStringInstanceHelper ();

		public void Write (string textToAppend) => this.GenerationEnvironment.Append (textToAppend);

		public void Write (string format, params object[] args) => GenerationEnvironment.AppendFormat (format, args);

		public void WriteLine (string textToAppend) => GenerationEnvironment.AppendLine (textToAppend);

		public void WriteLine (string format, params object[] args)
		{
			GenerationEnvironment.AppendFormat (format, args);
			GenerationEnvironment.AppendLine ();
		}

		public class ToStringInstanceHelper
		{
			public IFormatProvider FormatProvider { get; } = System.Globalization.CultureInfo.InvariantCulture;

			public string ToStringWithCulture (object objectToConvert)
			{
				if (objectToConvert == null) {
					throw new ArgumentNullException (nameof (objectToConvert));
				}
				var type = objectToConvert.GetType ();
				var iConvertibleType = typeof (IConvertible);
				if (iConvertibleType.IsAssignableFrom (type)) {
					return ((IConvertible)objectToConvert).ToString (FormatProvider);
				}
				var methInfo = type.GetMethod ("ToString", new Type[] { iConvertibleType });
				if ((methInfo != null)) {
					return (string)(methInfo.Invoke (objectToConvert, new object[] { FormatProvider}));
				}
				return objectToConvert.ToString ();
			}
		}
	}
}
