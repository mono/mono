// TransactedInstaller.cs
//   System.Configuration.Install.TransactedInstaller class implementation
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

namespace System.Configuration.Install
{

public class TransactedInstaller : Installer {
	// Constructors
	public TransactedInstaller ()
	{
	}

	// Methods
	public override void Install (IDictionary state)
	{
		if (state == null)
			throw new ArgumentException ("State saver cannot be null");

		try {
			Context.LogMessage ("Starting transacted installation.");
			base.Install (state);
		} catch (Exception e) {
			try {
				Context.LogMessage ("Errors occurred during installation -- starting rollback.");
				Context.addToLog (e);
				Rollback (state);
			} catch (Exception rbke) {
				Context.LogMessage ("Rollback failed.");
				Context.addToLog (rbke);
			}
			Context.LogMessage ("Rollback phase completed.");
			throw new Exception ("Transacted installation failed.", e);
		}

		try {
			Context.LogMessage ("Installation phasing completed successfully -- starting commit.");
			Commit (state);
		} catch (Exception e) {
			Context.LogMessage ("Commit failed.");
			Context.addToLog (e);
		}
		Context.LogMessage ("Commit phase completed.");
	}

	public override void Uninstall (IDictionary state)
	{
		Context.LogMessage ("Uninstallation started.");
		try {
			base.Uninstall (state);
		} catch (Exception e) {
			if (e is ArgumentException)
				throw e;
			Context.LogMessage ("Errors occurred during uninstallation.");
			Context.addToLog (e);
		} finally {
			Context.LogMessage ("Uninstallation completed.");
		}
	}
}

}
