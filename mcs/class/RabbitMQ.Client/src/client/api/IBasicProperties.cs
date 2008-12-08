// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;

namespace RabbitMQ.Client
{
    ///<summary>Common AMQP Basic content-class headers interface,
    ///spanning the union of the functionality offered by versions
    ///0-8, 0-8qpid and 0-9 (without WIP) of AMQP.</summary>
    ///<remarks>
    ///<para>
    ///The specification code generator provides
    ///protocol-version-specific implementations of this interface. To
    ///obtain an implementation of this interface in a
    ///protocol-version-neutral way, use
    ///IModel.CreateBasicProperties().
    ///</para>
    ///<para>
    ///Each property is readable, writable and clearable: a cleared
    ///property will not be transmitted over the wire. Properties on a
    ///fresh instance are clear by default.
    ///</para>
    ///</remarks>
    public interface IBasicProperties : IContentHeader
    {
        ///<summary> MIME content type </summary>
        string ContentType { get; set; }

        ///<summary> MIME content encoding </summary>
        string ContentEncoding { get; set; }

        ///<summary> message header field table </summary>
        IDictionary Headers { get; set; }

        ///<summary> non-persistent (1) or persistent (2) </summary>
        byte DeliveryMode { get; set; }

        ///<summary> message priority, 0 to 9 </summary>
        byte Priority { get; set; }

        ///<summary> application correlation identifier </summary>
        string CorrelationId { get; set; }

        ///<summary> destination to reply to </summary>
        string ReplyTo { get; set; }

        ///<summary> message expiration specification </summary>
        string Expiration { get; set; }

        ///<summary> application message identifier </summary>
        string MessageId { get; set; }

        ///<summary> message timestamp </summary>
        AmqpTimestamp Timestamp { get; set; }

        ///<summary> message type name </summary>
        string Type { get; set; }

        ///<summary> creating user id </summary>
        string UserId { get; set; }

        ///<summary> creating application id </summary>
        string AppId { get; set; }

        ///<summary> intra-cluster routing identifier </summary>
        string ClusterId { get; set; }

        ///<summary> Clear the ContentType property. </summary>
        void ClearContentType();

        ///<summary> Clear the ContentEncoding property. </summary>
        void ClearContentEncoding();

        ///<summary> Clear the Headers property. </summary>
        void ClearHeaders();

        ///<summary> Clear the DeliveryMode property. </summary>
        void ClearDeliveryMode();

        ///<summary> Clear the Priority property. </summary>
        void ClearPriority();

        ///<summary> Clear the CorrelationId property. </summary>
        void ClearCorrelationId();

        ///<summary> Clear the ReplyTo property. </summary>
        void ClearReplyTo();

        ///<summary> Clear the Expiration property. </summary>
        void ClearExpiration();

        ///<summary> Clear the MessageId property. </summary>
        void ClearMessageId();

        ///<summary> Clear the Timestamp property. </summary>
        void ClearTimestamp();

        ///<summary> Clear the Type property. </summary>
        void ClearType();

        ///<summary> Clear the UserId property. </summary>
        void ClearUserId();

        ///<summary> Clear the AppId property. </summary>
        void ClearAppId();

        ///<summary> Clear the ClusterId property. </summary>
        void ClearClusterId();

        ///<summary>Convenience property; parses ReplyTo property
        ///using PublicationAddress.Parse, and serializes it using
        ///PublicationAddress.ToString. Returns null if ReplyTo property
        ///cannot be parsed by PublicationAddress.Parse.</summary>
        PublicationAddress ReplyToAddress { get; set; }

        ///<summary>Sets DeliveryMode to either persistent (2) or non-persistent (1).</summary>
        ///<remarks>
        ///<para>
        ///The numbers 1 and 2 for delivery mode are "magic" in that
        ///they appear in the AMQP 0-8 and 0-9 specifications as part
        ///of the definition of the DeliveryMode Basic-class property,
        ///without being defined as named constants.
        ///</para>
        ///<para>
        ///Calling this method causes DeliveryMode to take on a
        ///value. In order to reset DeliveryMode to the default empty
        ///condition, call ClearDeliveryMode.
        ///</para>
        ///</remarks>
        void SetPersistent(bool persistent);
    }
}
