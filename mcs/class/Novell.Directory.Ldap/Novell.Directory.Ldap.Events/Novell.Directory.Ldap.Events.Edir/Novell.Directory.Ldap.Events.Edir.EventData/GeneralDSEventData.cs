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
// Novell.Directory.Ldap.Events.Edir.EventData.GeneralDSEventData.cs
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
  /// The class represents the data for General DS Events.
  /// </summary>
  public class GeneralDSEventData : BaseEdirEventData
  {
    protected int ds_time;
    public int DSTime
    {
      get
      {
	return ds_time;
      }
    }

    protected int milli_seconds;
    public int MilliSeconds
    {
      get
      {
	return milli_seconds;
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

    protected int current_process;
    public int CurrentProcess
    {
      get
      {
	return current_process;
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

    protected int[] integer_values;
    public int[] IntegerValues
    {
      get
      {
	return integer_values;
      }
    }

    protected string[] string_values;
    public string[] StringValues
    {
      get 
      {
	return string_values;
      }
    }

    public GeneralDSEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      ds_time = getTaggedIntValue(
                (Asn1Tagged) decoder.decode(decodedData, length),
                GeneralEventField.EVT_TAG_GEN_DSTIME);
      milli_seconds = getTaggedIntValue(
                (Asn1Tagged) decoder.decode(decodedData, length),
                GeneralEventField.EVT_TAG_GEN_MILLISEC);

      nVerb = getTaggedIntValue(
                (Asn1Tagged) decoder.decode(decodedData, length),
                GeneralEventField.EVT_TAG_GEN_VERB);
      current_process = getTaggedIntValue(
                (Asn1Tagged) decoder.decode(decodedData, length),
                GeneralEventField.EVT_TAG_GEN_CURRPROC);

      strPerpetratorDN = getTaggedStringValue(
                (Asn1Tagged) decoder.decode(decodedData, length),
                GeneralEventField.EVT_TAG_GEN_PERP);

      Asn1Tagged temptaggedvalue =
            ((Asn1Tagged) decoder.decode(decodedData, length));

      if (temptaggedvalue.getIdentifier().Tag
            == (int) GeneralEventField.EVT_TAG_GEN_INTEGERS) 
      {
	//Integer List.
	Asn1Sequence inteseq = getTaggedSequence(temptaggedvalue, GeneralEventField.EVT_TAG_GEN_INTEGERS);
	Asn1Object[] intobject = inteseq.toArray();
	integer_values = new int[intobject.Length];

	for (int i = 0; i < intobject.Length; i++) 
	{
	  integer_values[i] = ((Asn1Integer) intobject[i]).intValue();
	}

	//second decoding for Strings.
	temptaggedvalue = ((Asn1Tagged) decoder.decode(decodedData, length));
      } 
      else 
      {
	integer_values = null;
      }
      
      if ((temptaggedvalue.getIdentifier().Tag
            == (int) GeneralEventField.EVT_TAG_GEN_STRINGS)
            && (temptaggedvalue.getIdentifier().Constructed)) 
      {
	//String values.
	Asn1Sequence inteseq =
                getTaggedSequence(temptaggedvalue, GeneralEventField.EVT_TAG_GEN_STRINGS);
	Asn1Object[] stringobject = inteseq.toArray();
	string_values = new string[stringobject.Length];

	for (int i = 0; i < stringobject.Length; i++) 
	{
	  string_values[i] =
                    ((Asn1OctetString) stringobject[i]).stringValue();
	}
      } 
      else 
      {
	string_values = null;
      }

      DataInitDone();
    }

    protected int getTaggedIntValue(Asn1Tagged tagvalue, GeneralEventField tagid)
    {
      Asn1Object obj = tagvalue.taggedValue();

      if ((int)tagid != tagvalue.getIdentifier().Tag) 
      {
	throw new IOException("Unknown Tagged Data");
      }

      byte[] dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).byteValue());
      MemoryStream data = new MemoryStream(dbytes);
      
      LBERDecoder dec = new LBERDecoder();
      
      int length = dbytes.Length;
      
      return (int)(dec.decodeNumeric(data, length));
    }

    protected string getTaggedStringValue(Asn1Tagged tagvalue, GeneralEventField tagid)
    {
      Asn1Object obj = tagvalue.taggedValue();

      if ((int)tagid != tagvalue.getIdentifier().Tag) 
      {
	throw new IOException("Unknown Tagged Data");
      }

      byte[] dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).byteValue());
      MemoryStream data = new MemoryStream(dbytes);

      LBERDecoder dec = new LBERDecoder();

      int length = dbytes.Length;

      return (string) dec.decodeCharacterString(data, length);
    }

    protected Asn1Sequence getTaggedSequence(Asn1Tagged tagvalue, GeneralEventField tagid)
    {
      Asn1Object obj = tagvalue.taggedValue();

      if ((int)tagid != tagvalue.getIdentifier().Tag) 
      {
	throw new IOException("Unknown Tagged Data");
      }

      byte[] dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).byteValue());
      MemoryStream data = new MemoryStream(dbytes);

      LBERDecoder dec = new LBERDecoder();
      int length = dbytes.Length;

      return new Asn1Sequence(dec, data, length);
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();

      buf.Append("[GeneralDSEventData");
      buf.AppendFormat("(DSTime={0})", ds_time);
      buf.AppendFormat("(MilliSeconds={0})", milli_seconds);
      buf.AppendFormat("(verb={0})",nVerb);
      buf.AppendFormat("(currentProcess={0})", current_process);
      buf.AppendFormat("(PerpetartorDN={0})", strPerpetratorDN);
      buf.AppendFormat("(Integer Values={0})", integer_values);
      buf.AppendFormat("(String Values={0})", string_values);
      buf.Append("]");

      return buf.ToString();
    }
  }
}
