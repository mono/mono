//
// System.Web.UI.TemplateInstanceAttribute.cs
//
// Noam Lampert  (noaml@mainsoft.com)
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
#if NET_2_0

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI
{
	// attributes
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class TemplateInstanceAttribute : Attribute
	{
		#region Fields

		readonly TemplateInstance _instance;

		public static readonly TemplateInstanceAttribute Single;
		public static readonly TemplateInstanceAttribute Multiple;
		public static readonly TemplateInstanceAttribute Default;

		#endregion

		#region Constructors

		static TemplateInstanceAttribute () {
			Single = new TemplateInstanceAttribute (TemplateInstance.Single);
			Multiple = new TemplateInstanceAttribute (TemplateInstance.Multiple);
			Default = Multiple;
		}

		#endregion

		#region Properties

		public TemplateInstance Instances { get { return _instance; } }

		#endregion

		#region Methods

		public TemplateInstanceAttribute (TemplateInstance instance) {
			_instance = instance;
		}

		public override bool IsDefaultAttribute () {
			return Equals (Default);
		}

		public override bool Equals (object obj) {
			if (this == obj)
				return true;

			TemplateInstanceAttribute other = obj as TemplateInstanceAttribute;
			if (obj == null)
				return false;

			return Instances == other.Instances;
		}

		public override int GetHashCode () {
			return (int) Instances;
		}

		#endregion
	}
}

#endif
