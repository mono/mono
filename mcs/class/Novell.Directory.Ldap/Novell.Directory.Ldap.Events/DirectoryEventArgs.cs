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
// Novell.Directory.Ldap.Events.DirectoryEventArgs.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


namespace Novell.Directory.Ldap.Events
{
  /// <summary> 
  /// This is the base class for other EventArgs corresponding to 
  /// Ldap and Edir events.
  /// </summary>
  /// <seealso cref='Novell.Directory.Ldap.Events.LdapEventArgs'/>
  /// <seealso cref='Novell.Directory.Ldap.Events.Edir.EdirEventArgs'/>
  public class DirectoryEventArgs : BaseEventArgs
  {
    protected EventClassifiers eClassification;
    public EventClassifiers EventClassification
    {
      get 
      {
	return eClassification;
      }
      set
      {
	eClassification = value;
      }
    }

    public DirectoryEventArgs(LdapMessage sourceMessage,
			      EventClassifiers aClassification)
      : base(sourceMessage)
    {
      eClassification = aClassification;
    }
  }
}
