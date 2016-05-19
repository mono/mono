//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.IdentityModel.Claims;

    // Issues claimsets whose conditions (if any) have been evaluated.
    public interface IAuthorizationPolicy : IAuthorizationComponent
    {
        ClaimSet Issuer { get; }

        // Evaluates conditions (if any) against the context, may add grants to the context
        // Return 'false' if for this evaluation, should be called again if claims change. (eg. not done)
        // Return 'true' if no more claims will be added regardless of changes for this evaluation (eg. done).
        // 'state' is good for this evaluation only. Will be null if starting again.
        // Implementations should expect to be called multiple times on different threads.
        bool Evaluate(EvaluationContext evaluationContext, ref object state);
    }
}
