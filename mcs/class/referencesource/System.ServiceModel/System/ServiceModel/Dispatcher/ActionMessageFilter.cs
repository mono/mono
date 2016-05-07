//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    public class ActionMessageFilter : MessageFilter
    {
        Dictionary<string, int> actions;
        ReadOnlyCollection<string> actionSet;

        [DataMember(IsRequired = true)]
        internal string[] DCActions
        {
            get
            {
                string[] act = new string[this.actions.Count];
                actions.Keys.CopyTo(act, 0);
                return act;
            }
            set
            {
                Init(value);
            }
        }

        public ActionMessageFilter(params string[] actions)
        {
            if (actions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actions");
            }

            Init(actions);
        }

        void Init(string[] actions)
        {
            if (actions.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ActionFilterEmptyList), "actions"));
            }

            this.actions = new Dictionary<string, int>();
            for (int i = 0; i < actions.Length; ++i)
            {
                // Duplicates are removed
                if (!this.actions.ContainsKey(actions[i]))
                {
                    this.actions.Add(actions[i], 0);
                }
            }
        }

        public ReadOnlyCollection<string> Actions
        {
            get
            {
                if (this.actionSet == null)
                {
                    this.actionSet = new ReadOnlyCollection<string>(new List<string>(this.actions.Keys));
                }
                return this.actionSet;
            }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new ActionMessageFilterTable<FilterData>();
        }

        bool InnerMatch(Message message)
        {
            string act = message.Headers.Action;
            if (act == null)
            {
                act = string.Empty;
            }

            return this.actions.ContainsKey(act);
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return InnerMatch(message);
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            Message msg = messageBuffer.CreateMessage();
            try
            {
                return InnerMatch(msg);
            }
            finally
            {
                msg.Close();
            }
        }
    }
}
