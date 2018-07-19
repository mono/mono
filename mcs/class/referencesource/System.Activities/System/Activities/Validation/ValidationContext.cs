//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class ValidationContext
    {
        ActivityUtilities.ChildActivity owner;
        ActivityUtilities.ActivityCallStack parentChain;
        LocationReferenceEnvironment environment;
        IList<ValidationError> getChildrenErrors;
        ProcessActivityTreeOptions options;

        internal ValidationContext(ActivityUtilities.ChildActivity owner, ActivityUtilities.ActivityCallStack parentChain, ProcessActivityTreeOptions options, LocationReferenceEnvironment environment)
        {
            this.owner = owner;
            this.parentChain = parentChain;
            this.options = options;
            this.environment = environment;
        }

        internal LocationReferenceEnvironment Environment
        {
            get { return this.environment; }
        }

        internal IEnumerable<Activity> GetParents()
        {
            List<Activity> parentsList = new List<Activity>();

            for (int i = 0; i < parentChain.Count; i++)
            {
                parentsList.Add(parentChain[i].Activity);
            }

            return parentsList;
        }

        internal IEnumerable<Activity> GetWorkflowTree()
        {
            // It is okay to just walk the declared parent chain here
            Activity currentNode = this.owner.Activity;
            if (currentNode != null)
            {
                while (currentNode.Parent != null)
                {
                    currentNode = currentNode.Parent;
                }
                List<Activity> nodes = ActivityValidationServices.GetChildren(new ActivityUtilities.ChildActivity(currentNode, true), new ActivityUtilities.ActivityCallStack(), this.options);
                nodes.Add(currentNode);
                return nodes;
            }
            else
            {
                return ActivityValidationServices.EmptyChildren;
            }            
        }

        internal IEnumerable<Activity> GetChildren()
        {
            if (!this.owner.Equals(ActivityUtilities.ChildActivity.Empty))
            {
                return ActivityValidationServices.GetChildren(this.owner, this.parentChain, this.options);
            }
            else
            {
                return ActivityValidationServices.EmptyChildren;
            }
        }

        internal void AddGetChildrenErrors(ref IList<ValidationError> validationErrors)
        {
            if (this.getChildrenErrors != null && this.getChildrenErrors.Count > 0)
            {
                if (validationErrors == null)
                {
                    validationErrors = new List<ValidationError>();
                }

                for (int i = 0; i < this.getChildrenErrors.Count; i++)
                {
                    validationErrors.Add(this.getChildrenErrors[i]);
                }

                this.getChildrenErrors = null;
            }
        }
    }
}
