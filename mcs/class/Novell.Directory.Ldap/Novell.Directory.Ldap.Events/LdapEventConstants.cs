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
// Novell.Directory.Ldap.Events.LdapEventConstants.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap.Events
{

  /// <summary>
  /// Event Classifiers
  /// </summary>
  public enum EventClassifiers
  {
    CLASSIFICATION_UNKNOWN = -1,
    CLASSIFICATION_LDAP_PSEARCH = 0,
    CLASSIFICATION_EDIR_EVENT = 1
  }

  /// <summary>
  /// Types of Ldap Events
  /// </summary>
  public enum LdapEventType
  {
    TYPE_UNKNOWN = LdapEventSource.EVENT_TYPE_UNKNOWN,
    LDAP_PSEARCH_ADD = LdapPersistSearchControl.ADD,
    LDAP_PSEARCH_DELETE = LdapPersistSearchControl.DELETE,
    LDAP_PSEARCH_MODIFY = LdapPersistSearchControl.MODIFY,
    LDAP_PSEARCH_MODDN = LdapPersistSearchControl.MODDN,
    LDAP_PSEARCH_ANY = LDAP_PSEARCH_ADD | LDAP_PSEARCH_DELETE | LDAP_PSEARCH_MODIFY | LDAP_PSEARCH_MODDN
  }
}
