//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    internal class DirectionalAction : IComparable<DirectionalAction>
    {
        MessageDirection direction;
        string action;
        bool isNullAction;

        internal DirectionalAction(MessageDirection direction, string action)
        {
            if (!MessageDirectionHelper.IsDefined(direction))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("direction"));

            this.direction = direction;
            if (action == null)
            {
                this.action = MessageHeaders.WildcardAction;
                this.isNullAction = true;
            }
            else
            {
                this.action = action;
                this.isNullAction = false;
            }
        }

        public MessageDirection Direction
        { get { return this.direction; } }

        public string Action
        { get { return this.isNullAction ? null : this.action; } }

        public override bool Equals(Object other)
        {
            DirectionalAction tmp = other as DirectionalAction;
            if (tmp == null)
                return false;
            return this.Equals(tmp);
        }

        public bool Equals(DirectionalAction other)
        {
            if (other == null)
                return false;

            return (this.direction == other.direction)
                && (this.action == other.action);
        }

        public int CompareTo(DirectionalAction other)
        {
            if (other == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");

            if ((this.direction == MessageDirection.Input) && (other.direction == MessageDirection.Output))
                return -1;
            if ((this.direction == MessageDirection.Output) && (other.direction == MessageDirection.Input))
                return 1;

            return this.action.CompareTo(other.action);
        }

        public override int GetHashCode()
        {
            return this.action.GetHashCode();
        }
    }
}
