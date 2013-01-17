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
// (C) 2005 Iain McCoy
// (C) 2007 Novell, Inc.
//
// Authors:
//   Iain McCoy (iain@mccoy.id.au)
//   Chris Toshok (toshok@ximian.com)
//
//

using System.Collections.Generic;

namespace System.Windows {
	public class DependencyObjectType {

		private static Dictionary<Type,DependencyObjectType> typeMap = new Dictionary<Type,DependencyObjectType>();
		private static int current_id;

		private int id;
		private Type systemType;
		
		private DependencyObjectType (int id, Type systemType)
		{
			this.id = id;
			this.systemType = systemType;
		}

		public DependencyObjectType BaseType { 
			get { return DependencyObjectType.FromSystemType (systemType.BaseType); }
		}

		public int Id { 
			get { return id; }
		}

		public string Name { 
			get { return systemType.Name; }
		}

		public Type SystemType {
			get { return systemType; }
		}

		public static DependencyObjectType FromSystemType(Type systemType)
		{
			if (typeMap.ContainsKey (systemType))
				return typeMap[systemType];

			DependencyObjectType dot;

			typeMap[systemType] = dot = new DependencyObjectType (current_id++, systemType);

			return dot;
		}

		public bool IsInstanceOfType(DependencyObject dependencyObject)
		{
			return systemType.IsInstanceOfType (dependencyObject);
		}

		public bool IsSubclassOf(DependencyObjectType dependencyObjectType)
		{
			return systemType.IsSubclassOf (dependencyObjectType.SystemType);
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
	}
}
