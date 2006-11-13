/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Events.Edir.EdirEventArgs.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary> 
  /// This class represents the EventArgs for Edir events in general.
  /// </summary>
  public class EdirEventArgs : DirectoryEventArgs
  {
    /// <summary> 
    /// This property gives the contained event information in the form
    /// of an IntermediateResponse if the contained information is of correct
    /// type. In case the type of contained information is incorrect, null is returned.
    /// </summary>
    public EdirEventIntermediateResponse IntermediateResponse
    {
      get
      {
	if (ldap_message is EdirEventIntermediateResponse)
	  return (EdirEventIntermediateResponse)ldap_message;
	else
	  return null;
      }
    }

    public EdirEventArgs(LdapMessage sourceMessage,
			 EventClassifiers aClassification)
      : base(sourceMessage, aClassification)
    {
    }

  }
}
