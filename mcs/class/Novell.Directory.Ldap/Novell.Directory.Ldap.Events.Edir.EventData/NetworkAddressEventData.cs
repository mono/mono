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
// Novell.Directory.Ldap.Events.Edir.EventData.NetworkAddressEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This class represents the data for Network Address Events.
  /// </summary>
  public class NetworkAddressEventData : BaseEdirEventData
  {
    protected int nType;
    public int ValueType
    {
      get
      {
	return nType;
      }
    }

    protected string strData;
    public string Data
    {
      get
      {
	return strData;
      }
    }

    public NetworkAddressEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      nType = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      strData = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

      DataInitDone();
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[NetworkAddress");
      buf.AppendFormat("(type={0})", nType);
      buf.AppendFormat("(Data={0})", strData);
      buf.Append("]");

      return buf.ToString();
    }
  }
}
