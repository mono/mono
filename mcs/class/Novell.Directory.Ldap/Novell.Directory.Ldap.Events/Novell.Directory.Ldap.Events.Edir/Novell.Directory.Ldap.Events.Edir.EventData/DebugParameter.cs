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
// Novell.Directory.Ldap.Events.Edir.EventData.DebugParameter.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Text;

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This class represents the Debug Paramenter that is part of
  /// the DebugEventData.
  /// </summary>
  public class DebugParameter
  {
    protected DebugParameterType debug_type;
    public DebugParameterType DebugType
    {
      get
      {
	return debug_type;
      }
    }

    protected object objData;
    public object Data
    {
      get
      {
	return objData;
      }
    }

    public DebugParameter(Asn1Tagged dseObject)
    {
      switch ((DebugParameterType)(dseObject.getIdentifier().Tag))
      {
      case DebugParameterType.ENTRYID:
      case DebugParameterType.INTEGER:
	objData = getTaggedIntValue(dseObject);
	break;

      case DebugParameterType.BINARY:
	objData = ((Asn1OctetString) dseObject.taggedValue()).byteValue();
	break;

      case DebugParameterType.STRING:
	objData = ((Asn1OctetString) dseObject.taggedValue()).stringValue();
	break;

      case DebugParameterType.TIMESTAMP:
	objData = new DSETimeStamp(getTaggedSequence(dseObject));
	break;

      case DebugParameterType.TIMEVECTOR:
	ArrayList timeVector = new ArrayList();
	Asn1Sequence seq = getTaggedSequence(dseObject);
	int count = ((Asn1Integer) seq.get_Renamed(0)).intValue();
	if (count > 0)
	{
	  Asn1Sequence timeSeq = (Asn1Sequence) seq.get_Renamed(1);
	
	  for (int i = 0; i < count; i++)
	  {
	    timeVector.Add(new DSETimeStamp((Asn1Sequence) timeSeq.get_Renamed(i)));
	  }
	}

	objData = timeVector;
	break;

      case DebugParameterType.ADDRESS:
	objData = new ReferralAddress(getTaggedSequence(dseObject));
	break;

      default:
	throw new IOException("Unknown Tag in DebugParameter..");
      }

      debug_type = (DebugParameterType)(dseObject.getIdentifier().Tag);
    }

    protected int getTaggedIntValue(Asn1Tagged tagVal)
    {
      Asn1Object obj = tagVal.taggedValue();
      byte[] dataBytes = SupportClass.ToByteArray(((Asn1OctetString) obj).byteValue());
      
      MemoryStream decodedData = new MemoryStream(dataBytes);
      LBERDecoder decoder = new LBERDecoder();

      return ((int) decoder.decodeNumeric(
				decodedData, 
				dataBytes.Length));
    }

    protected Asn1Sequence getTaggedSequence(Asn1Tagged tagVal)
    {
       Asn1Object obj = tagVal.taggedValue();
      byte[] dataBytes = SupportClass.ToByteArray(((Asn1OctetString) obj).byteValue());
      
      MemoryStream decodedData = new MemoryStream(dataBytes);
      LBERDecoder decoder = new LBERDecoder();

      return new Asn1Sequence(decoder, decodedData, dataBytes.Length);
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString()
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[DebugParameter");
      if (Enum.IsDefined(debug_type.GetType(), debug_type))
      {
	  buf.AppendFormat("(type={0},", debug_type);
	  buf.AppendFormat("value={0})", objData);
      }
      else
      {
	  buf.Append("(type=Unknown)");
      }
      buf.Append("]");

      return buf.ToString();
    }
  }
}
