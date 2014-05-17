//
// FileExtensionAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
// Copyright (C) 2013 Pablo Ruiz García
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

#if NET_4_5

using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations
{
	// See: http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.fileextensionsattribute.aspx

	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FileExtensionsAttribute : DataTypeAttribute
	{
		private const string DefaultErrorMessage = "The {0} field only accepts files with the following extensions: {1}.";
		private const string DefaultExtensions = "png,jpg,jpeg,gif";

		public FileExtensionsAttribute ()
			: base (DataType.Upload)
		{
			// XXX: There is no .ctor accepting Func<string> on DataTypeAttribute.. :?
			base.ErrorMessage = DefaultErrorMessage;
			this.Extensions = DefaultExtensions;
		}

		public string Extensions { get; set; }

		private string[] GetExtensionList ()
		{
			return (Extensions ?? "").Split (',');
		}

		private string GetExtension (string filename)
		{
			var parts = filename.Split ('.');
			return parts.Length > 0 ? parts [parts.Length - 1] : "";
		}

		public override string FormatErrorMessage (string name)
		{
			var extensions = GetExtensionList().Aggregate ((cur, next) => cur + ", " + next);
			return string.Format (ErrorMessageString, name, extensions);
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			if (value is string)
			{
				var str = value as string;
				var ext = GetExtension (str);
				return GetExtensionList ().Any (x => string.Equals (x, ext, StringComparison.InvariantCultureIgnoreCase));
			}

			return false;
		}
	}
}

#endif
