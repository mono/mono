//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Model;

    internal class TransitionReorderChange : Change
    {
        public override string Description
        {
            get 
            { 
                return null; 
            }
        }

        public override bool Apply()
        {
            return false;
        }

        public override Change GetInverse()
        {
            return new TransitionReorderChange();
        }
    }
}
