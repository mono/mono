//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;

    public class ChannelParameterCollection : Collection<object>
    {
        IChannel channel;

        public ChannelParameterCollection()
        {
        }

        public ChannelParameterCollection(IChannel channel)
        {
            this.channel = channel;
        }

        protected virtual IChannel Channel
        {
            get { return this.channel; }
        }

        public void PropagateChannelParameters(IChannel innerChannel)
        {
            if (innerChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerChannel");
            }

            this.ThrowIfMutable();

            ChannelParameterCollection innerCollection = innerChannel.GetProperty<ChannelParameterCollection>();
            if (innerCollection != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    innerCollection.Add(this[i]);
                }
            }
        }

        protected override void ClearItems()
        {
            this.ThrowIfDisposedOrImmutable();
            base.ClearItems();
        }

        protected override void InsertItem(int index, object item)
        {
            this.ThrowIfDisposedOrImmutable();
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.ThrowIfDisposedOrImmutable();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, object item)
        {
            this.ThrowIfDisposedOrImmutable();
            base.SetItem(index, item);
        }

        void ThrowIfDisposedOrImmutable()
        {
            IChannel channel = this.Channel;
            if (channel != null)
            {
                CommunicationState state = channel.State;
                string text = null;

                switch (state)
                {
                    case CommunicationState.Created:
                        break;

                    case CommunicationState.Opening:
                    case CommunicationState.Opened:
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                    case CommunicationState.Faulted:
                        text = SR.GetString(SR.ChannelParametersCannotBeModified,
                                            channel.GetType().ToString(), state.ToString());
                        break;

                    default:
                        text = SR.GetString(SR.CommunicationObjectInInvalidState,
                                            channel.GetType().ToString(), state.ToString());
                        break;
                }

                if (text != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(text));
                }
            }
        }

        void ThrowIfMutable()
        {
            IChannel channel = this.Channel;
            if (channel != null)
            {
                CommunicationState state = channel.State;
                string text = null;

                switch (state)
                {
                    case CommunicationState.Created:
                        text = SR.GetString(SR.ChannelParametersCannotBePropagated,
                                            channel.GetType().ToString(), state.ToString());
                        break;

                    case CommunicationState.Opening:
                    case CommunicationState.Opened:
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                    case CommunicationState.Faulted:
                        break;

                    default:
                        text = SR.GetString(SR.CommunicationObjectInInvalidState,
                                            channel.GetType().ToString(), state.ToString());
                        break;
                }

                if (text != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(text));
                }
            }
        }
    }
}
