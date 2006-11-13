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
// Novell.Directory.Ldap.Events.Edir.EdirEventIntermediateResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Events.Edir.EventData;

namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary>
  /// This class represents the intermediate response corresponding to edir events.
  /// </summary>
  public class EdirEventIntermediateResponse : LdapIntermediateResponse
  {
    protected EdirEventType event_type;

    /// <summary>
    /// Type of Edir event.
    /// </summary>
    public EdirEventType EventType
    {
      get
      {
	return event_type;
      }
    }

    protected EdirEventResultType event_result_type;
    
    /// <summary>
    /// Type of Edir event result.
    /// </summary>
    public EdirEventResultType EventResultType
    {
      get
      {
	return event_result_type;
      }
    }
    
    protected BaseEdirEventData event_response_data;

    /// <summary>
    /// The response data object associated with Edir event.
    /// </summary>
    public BaseEdirEventData EventResponseDataObject
    {
      get
      {
	return event_response_data;
      }
    }
    
    public EdirEventIntermediateResponse(RfcLdapMessage message)
      : base(message)
    {
      ProcessMessage(getValue());
    }

    public EdirEventIntermediateResponse(byte[] message)
      : base( new RfcLdapMessage( new Asn1Sequence() ) )
    {
      ProcessMessage(SupportClass.ToSByteArray(message));
    }

    [CLSCompliantAttribute(false)]
    protected void ProcessMessage(sbyte[] returnedValue)
    {
      LBERDecoder decoder = new LBERDecoder();
      Asn1Sequence sequence = (Asn1Sequence) decoder.decode(returnedValue);

      event_type = (EdirEventType)(((Asn1Integer) sequence.get_Renamed(0)).intValue());
      event_result_type = (EdirEventResultType)(((Asn1Integer) sequence.get_Renamed(1)).intValue());

      if (sequence.size() > 2)
      {
	Asn1Tagged objTagged = (Asn1Tagged) sequence.get_Renamed(2);
	
	switch((EdirEventDataType)(objTagged.getIdentifier().Tag))
	{
	case EdirEventDataType.EDIR_TAG_ENTRY_EVENT_DATA:
	  event_response_data = new EntryEventData(EdirEventDataType.EDIR_TAG_ENTRY_EVENT_DATA, objTagged.taggedValue());
	  break;

	case EdirEventDataType.EDIR_TAG_VALUE_EVENT_DATA:
	  event_response_data = new ValueEventData(EdirEventDataType.EDIR_TAG_VALUE_EVENT_DATA, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_DEBUG_EVENT_DATA:
	  event_response_data = new DebugEventData(EdirEventDataType.EDIR_TAG_DEBUG_EVENT_DATA, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_GENERAL_EVENT_DATA:
	  event_response_data = new GeneralDSEventData(EdirEventDataType.EDIR_TAG_GENERAL_EVENT_DATA, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_SKULK_DATA:
	  event_response_data = null;
	  break;
	  
	case EdirEventDataType.EDIR_TAG_BINDERY_EVENT_DATA:
	  event_response_data = new BinderyObjectEventData(EdirEventDataType.EDIR_TAG_BINDERY_EVENT_DATA, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_DSESEV_INFO:
	  event_response_data = new SecurityEquivalenceEventData(EdirEventDataType.EDIR_TAG_DSESEV_INFO, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_MODULE_STATE_DATA:
	  event_response_data = new ModuleStateEventData(EdirEventDataType.EDIR_TAG_MODULE_STATE_DATA, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_NETWORK_ADDRESS:
	  event_response_data = new NetworkAddressEventData(EdirEventDataType.EDIR_TAG_NETWORK_ADDRESS, objTagged.taggedValue());
	  break;
	  
	case EdirEventDataType.EDIR_TAG_CONNECTION_STATE:
	  event_response_data = new ConnectionStateEventData(EdirEventDataType.EDIR_TAG_CONNECTION_STATE, objTagged.taggedValue());
	  break;

	case EdirEventDataType.EDIR_TAG_CHANGE_SERVER_ADDRESS:
	  event_response_data = new ChangeAddressEventData(EdirEventDataType.EDIR_TAG_CHANGE_SERVER_ADDRESS, objTagged.taggedValue());
	  break;

	  /*
            case EdirEventDataType.EDIR_TAG_CHANGE_CONFIG_PARAM :
                responsedata =
                    new ChangeConfigEventData(
                        taggedobject.taggedValue());

                break;

            case EdirEventDataType.EDIR_TAG_STATUS_LOG :
                responsedata =
                    new StatusLogEventData(taggedobject.taggedValue());

                break;
	  */
	case EdirEventDataType.EDIR_TAG_NO_DATA:
	  event_response_data = null;
	  break;

	default:
	  //unhandled data.
	  throw new IOException();
	  }
      } else
      {
	//NO DATA
	event_response_data = null;
      }
    }
  }
}
