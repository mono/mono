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
// Novell.Directory.Ldap.Events.Edir.MonitorEventResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary> 
  /// This object represents the ExtendedResponse returned when Event
  /// Registeration fails. This Extended Response structure is generated for
  /// requests send as MonitorEventRequest.
  /// </summary>
  public class MonitorEventResponse : LdapExtendedResponse
  {
    protected EdirEventSpecifier[] specifier_list;
    public EdirEventSpecifier[] SpecifierList
    {
      get
      {
	return specifier_list;
      }
    }

    public MonitorEventResponse(RfcLdapMessage message)
      : base(message)
    {
      sbyte[] returnedValue = Value;
      
      if (null == returnedValue)
      {
	throw new LdapException(LdapException.resultCodeToString(ResultCode),
				ResultCode, 
				null);
      }

      LBERDecoder decoder = new LBERDecoder();

      Asn1Sequence sequence = (Asn1Sequence) decoder.decode(returnedValue);
      
      int length = ((Asn1Integer) sequence.get_Renamed(0)).intValue();
      Asn1Set sequenceSet = (Asn1Set) sequence.get_Renamed(1);
      specifier_list = new EdirEventSpecifier[length];

      for (int i = 0; i < length; i++) 
      {
	Asn1Sequence eventspecifiersequence =
                (Asn1Sequence) sequenceSet.get_Renamed(i);
	int classfication =
                ((Asn1Integer) eventspecifiersequence.get_Renamed(0)).intValue();
	int enumtype =
	  ((Asn1Enumerated) eventspecifiersequence.get_Renamed(1)).intValue();
	specifier_list[i] =
	  new EdirEventSpecifier((EdirEventType)classfication, (EdirEventResultType)enumtype);
        }
    }
  }
}
