// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Validation;
    using System.Activities.Statements;
    using System.Collections.Generic;

    internal class PickValidationErrorSourceLocator : IValidationErrorSourceLocator
    {
        public List<object> FindSourceDetailFromActivity(Activity errorSource, object errorSourceDetail)
        {
            if (errorSourceDetail == null)
            {
                return new List<object> { errorSource };
            }
            else
            {
                return FindRelativePath((Pick)errorSource, errorSourceDetail);
            }
        }

        public void ReplaceParentChainWithSource(Activity parentActivity, List<object> parentChain)
        {
            Pick pick = (Pick)parentActivity;

            if (parentChain.Count > 1)
            {
                // assume last object in parentChain is PickBranchBody

                // remove PickBranchBody
                parentChain.RemoveAt(parentChain.Count - 1);

                Activity targetActivity = (Activity)parentChain[parentChain.Count - 1];

                // the targetActivity will be available in the path
                parentChain.RemoveAt(parentChain.Count - 1);

                List<object> path = FindRelativePath(pick, targetActivity);

                foreach (object pathObject in path)
                {
                    parentChain.Add(pathObject);
                }
            }
        }

        private static List<object> FindRelativePath(Pick pickActivity, object descendent)
        {
            List<object> path = FindDescendentFromPick(pickActivity, descendent);
            path.Reverse();
            return path;
        }

        private static List<object> FindDescendentFromPick(Pick pickActivity, object descendent)
        {
            List<object> path = new List<object>();
            path.Add(pickActivity);
            foreach (PickBranch branch in pickActivity.Branches)
            {
                if (branch == descendent)
                {
                    break;
                }
                else if (branch.Trigger == descendent)
                {
                    path.Add(branch);
                    break;
                }
                else if (branch.Action == descendent)
                {
                    path.Add(branch);
                    break;
                }
                else
                {
                    bool isVariableError = false;
                    foreach (Variable variable in branch.Variables)
                    {
                        if (variable.Default == descendent)
                        {
                            isVariableError = true;
                            break;
                        }
                    }

                    if (isVariableError)
                    {
                        path.Add(branch);
                        break;
                    }
                }
            }

            path.Add(descendent);
            return path;
        }       
    }
}
