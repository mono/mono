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
// Novell.Directory.Ldap.Events.Edir.EventData.ValueEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using System.Text;

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This class represents the data for Value Events.
  /// </summary>
  public class ValueEventData : BaseEdirEventData
  {
    protected string strAttribute;
    public string Attribute
    {
      get
      {
	return strAttribute;
      }
    }

    protected string strClassId;
    public string ClassId
    {
      get
      {
	return strClassId;
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

    protected byte[] binData;
    public byte[] BinaryData
    {
      get
      {
	return binData;
      }
    }
    protected string strEntry;
    public string Entry
    {
      get
      {
	return strEntry;
      }
    }

    protected string strPerpetratorDN;
    public string PerpetratorDN
    {
      get 
      {
	return strPerpetratorDN;
      }
    }

    // syntax
    protected string strSyntax;
    public string Syntax
    {
      get
      {
	return strSyntax;
      }
    }

    protected DSETimeStamp timeStampObj;
    public DSETimeStamp TimeStamp
    {
      get
      {
	return timeStampObj;
      }
    }

    protected int nVerb;
    public int Verb
    {
      get
      {
	return nVerb;
      }
    }

    public ValueEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
        int[] length = new int[1];
	Asn1OctetString octData;

        strPerpetratorDN =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
        strEntry =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
        strAttribute =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
        strSyntax =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

        strClassId =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

        timeStampObj =
            new DSETimeStamp((Asn1Sequence) decoder.decode(decodedData, length));

        octData = ((Asn1OctetString) decoder.decode(decodedData, length));
	strData = octData.stringValue();
	binData = SupportClass.ToByteArray(octData.byteValue());

        nVerb = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();

	DataInitDone();
    }    

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();

      buf.Append("[ValueEventData");
      buf.AppendFormat("(Attribute={0})", strAttribute);
      buf.AppendFormat("(Classid={0})", strClassId);
      buf.AppendFormat("(Data={0})", strData);
      buf.AppendFormat("(Data={0})", binData);
      buf.AppendFormat("(Entry={0})", strEntry);
      buf.AppendFormat("(Perpetrator={0})", strPerpetratorDN);
      buf.AppendFormat("(Syntax={0})", strSyntax);
      buf.AppendFormat("(TimeStamp={0})", timeStampObj);
      buf.AppendFormat("(Verb={0})", nVerb);
      buf.Append("]");
      
      return buf.ToString();
    }
  }
}
