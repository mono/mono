// InstallerAssembly.cs
//   This file contains an installable assembly's source code.
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

using System;
using System.Collections;
using System.Configuration.Install;
using System.ComponentModel;

namespace MonoTests.System.Configuration.Install
{

[RunInstaller (true)]
public class InstallerAssembly : Installer {
	public override void Install (IDictionary state)
	{
		base.Install (state);
		Context.LogMessage (">>InstallerAssembly.Install called");
	}

	public override void Commit (IDictionary state)
	{
		base.Commit (state);
		Context.LogMessage (">>InstallerAssembly.Commit called");
	}

	public override void Rollback (IDictionary state)
	{
		base.Rollback (state);
		Context.LogMessage (">>InstallerAssembly.Rollback called");
	}

	public override void Uninstall (IDictionary state)
	{
		base.Uninstall (state);
		Context.LogMessage (">>InstallerAssembly.Uninstall called");
	}

	public override string HelpText {
		get {
			return "This is help text from the installer.";
		}
	}
}

[RunInstaller (true)]
class PrivateInstaller : Installer {
	public override void Install (IDictionary state)
	{
		base.Install (state);
		Context.LogMessage (">>This should not be called");
	}
}

[RunInstaller (false)]
public class NotToBeRun : Installer {
	public override void Install (IDictionary state)
	{
		base.Install (state);
		Context.LogMessage (">>This should not be called");
	}
}

[RunInstaller (true)]
public class NotAnInstaller {
	public void Install (IDictionary state)
	{
	}
}

}
