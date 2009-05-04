//
// System.Web.UI.FileLevelControlBuilderAttribute.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//     Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
// (C) 2009 Novell, Inc (http://novell.com/)
//
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
namespace System.Web.UI
{
	[AttributeUsageAttribute (AttributeTargets.Class)]
	public sealed class FileLevelControlBuilderAttribute : Attribute
	{
		public static readonly FileLevelControlBuilderAttribute Default = new FileLevelControlBuilderAttribute (null);
		
		public FileLevelControlBuilderAttribute (Type builderType)
		{
			this.BuilderType = builderType;
		}
		
		public Type BuilderType {
			get;
			private set;
		}
		
		public override bool Equals (Object obj)
		{
			var attr = obj as FileLevelControlBuilderAttribute;
			return ((attr != null) && this.BuilderType == attr.BuilderType);
		}
		
		public new static bool Equals (Object objA, Object objB)
		{
			var attrA = objA as FileLevelControlBuilderAttribute;
			if (attrA == null)
				return false;

			var attrB = objB as FileLevelControlBuilderAttribute;
			if (attrB == null)
				return false;

			return (attrA.BuilderType == attrB.BuilderType);
		}
			
		public override int GetHashCode ()
		{
			Type type = BuilderType;
			if (type == null)
				return base.GetHashCode ();

			return type.GetHashCode ();
		}
		
		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
	}
}
#endif
