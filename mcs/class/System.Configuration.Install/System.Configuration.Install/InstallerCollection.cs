// InstallerCollection.cs
//   System.Configuration.Install.InstallerCollection class implementation
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
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

using System.Collections;

namespace System.Configuration.Install
{

public class InstallerCollection : CollectionBase {
	private Installer parent;

	// Constructors
	internal InstallerCollection (Installer parent)
	{
		this.parent = parent;
	}

	// Properties
	public Installer this [int index] {
		get {
			return (Installer)List [index];
		}
		set {
			List [index] = value;
		}
	}

	// Methods
	public int Add (Installer installer)
	{
		return List.Add (installer);
	}

	public void AddRange (Installer [] installers)
	{
		foreach (Installer ins in installers)
			Add (ins);
	}

	public void AddRange (InstallerCollection installers)
	{
		foreach (Installer ins in installers)
			Add (ins);
	}

	public bool Contains (Installer installer)
	{
		return List.Contains (installer);
	}

	public void CopyTo (Installer [] array, int index)
	{
		List.CopyTo (array, index);
	}

	public int IndexOf (Installer installer)
	{
		return List.IndexOf (installer);
	}

	public void Insert (int index, Installer installer)
	{
		List.Insert (index, installer);
	}

	public void Remove (Installer installer)
	{
		List.Remove (installer);
	}

	protected override void OnInsert (int index, object val)
	{
		((Installer)val).Parent = parent;
	}

	protected override void OnRemove (int index, object val)
	{
		((Installer)val).Parent = null;
	}

	protected override void OnSet (int index, object oldVal, object newVal)
	{
		((Installer)oldVal).Parent = null;
		((Installer)newVal).Parent = parent;
	}
}

}
