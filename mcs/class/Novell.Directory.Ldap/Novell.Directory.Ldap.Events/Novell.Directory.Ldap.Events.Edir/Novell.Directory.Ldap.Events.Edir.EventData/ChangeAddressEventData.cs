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
// Novell.Directory.Ldap.Events.Edir.EventData.ChangeAddressEventData.cs
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
  /// This class represents the data for Change Address.
  /// </summary>
  public class ChangeAddressEventData : BaseEdirEventData
  {
    protected int nFlags;
    public int Flags
    {
      get
      {
	return nFlags;
      }
    }

    protected int nProto;
    public int Proto
    {
      get
      {
	return nProto;
      }
    }

    protected int address_family;
    public int AddressFamily
    {
      get
      {
	return address_family;
      }
    }

    protected string strAddress;
    public string Address
    {
      get
      {
	return strAddress;
      }
    }

    protected string pstk_name;
    public string PstkName
    {
      get
      {
	return pstk_name;
      }
    }

    protected string source_module;
    public string SourceModule
    {
      get
      {
	return source_module;
      }
    }

    public ChangeAddressEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      nFlags = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      nProto = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      address_family = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      strAddress = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      pstk_name = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      source_module = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

      DataInitDone();
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[ChangeAddresssEvent");
      buf.AppendFormat("(flags={0})", + nFlags);
      buf.AppendFormat("(proto={0})", nProto);
      buf.AppendFormat("(addrFamily={0})", address_family);
      buf.AppendFormat("(address={0})", strAddress);
      buf.AppendFormat("(pstkName={0})", pstk_name);
      buf.AppendFormat("(source={0})", source_module);
      buf.Append("]");

      return buf.ToString();
    }
  }
}
