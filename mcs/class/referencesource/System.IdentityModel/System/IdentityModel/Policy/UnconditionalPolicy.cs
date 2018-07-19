//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.Security.Principal;

    interface IIdentityInfo
    {
        IIdentity Identity { get; }
    }

    class UnconditionalPolicy : IAuthorizationPolicy, IDisposable
    {
        SecurityUniqueId id;
        ClaimSet issuer;
        ClaimSet issuance;
        ReadOnlyCollection<ClaimSet> issuances;
        DateTime expirationTime;
        IIdentity primaryIdentity;
        bool disposable = false;
        bool disposed = false;

        public UnconditionalPolicy(ClaimSet issuance)
            : this(issuance, SecurityUtils.MaxUtcDateTime)
        {
        }

        public UnconditionalPolicy(ClaimSet issuance, DateTime expirationTime)
        {
            if (issuance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuance");

            Initialize(ClaimSet.System, issuance, null, expirationTime);
        }

        public UnconditionalPolicy(ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime)
        {
            if (issuances == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuances");

            Initialize(ClaimSet.System, null, issuances, expirationTime);
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ClaimSet issuance)
            : this(issuance)
        {
            this.primaryIdentity = primaryIdentity;
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ClaimSet issuance, DateTime expirationTime)
            : this(issuance, expirationTime)
        {
            this.primaryIdentity = primaryIdentity;
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime)
            : this(issuances, expirationTime)
        {
            this.primaryIdentity = primaryIdentity;
        }

        UnconditionalPolicy(UnconditionalPolicy from)
        {
            this.disposable = from.disposable;
            this.primaryIdentity = from.disposable ? SecurityUtils.CloneIdentityIfNecessary(from.primaryIdentity) : from.primaryIdentity;
            if (from.issuance != null)
            {
                this.issuance = from.disposable ? SecurityUtils.CloneClaimSetIfNecessary(from.issuance) : from.issuance;
            }
            else
            {
                this.issuances = from.disposable ? SecurityUtils.CloneClaimSetsIfNecessary(from.issuances) : from.issuances;
            }
            this.issuer = from.issuer;
            this.expirationTime = from.expirationTime;
        }

        void Initialize(ClaimSet issuer, ClaimSet issuance, ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime)
        {
            this.issuer = issuer;
            this.issuance = issuance;
            this.issuances = issuances;
            this.expirationTime = expirationTime;
            if (issuance != null)
            {
                this.disposable = issuance is WindowsClaimSet;
            }
            else
            {
                for (int i = 0; i < issuances.Count; ++i)
                {
                    if (issuances[i] is WindowsClaimSet)
                    {
                        this.disposable = true;
                        break;
                    }
                }
            }
        }

        public string Id
        {
            get
            {
                if (this.id == null)
                    this.id = SecurityUniqueId.Create();
                return this.id.Value; 
            }
        }

        public ClaimSet Issuer
        {
            get { return this.issuer; }
        }

        internal IIdentity PrimaryIdentity
        {
            get 
            {
                ThrowIfDisposed();
                if (this.primaryIdentity == null)
                {
                    IIdentity identity = null;
                    if (this.issuance != null)
                    {
                        if (this.issuance is IIdentityInfo)
                        {
                            identity = ((IIdentityInfo)this.issuance).Identity;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.issuances.Count; ++i)
                        {
                            ClaimSet issuance = this.issuances[i];
                            if (issuance is IIdentityInfo)
                            {
                                identity = ((IIdentityInfo)issuance).Identity;
                                // Preferably Non-Anonymous
                                if (identity != null && identity != SecurityUtils.AnonymousIdentity)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    this.primaryIdentity = identity ?? SecurityUtils.AnonymousIdentity;
                }
                return this.primaryIdentity;
            }
        }

        internal ReadOnlyCollection<ClaimSet> Issuances
        {
            get 
            {
                ThrowIfDisposed();
                if (this.issuances == null)
                {
                    List<ClaimSet> issuances = new List<ClaimSet>(1);
                    issuances.Add(issuance);
                    this.issuances = issuances.AsReadOnly();
                }
                return this.issuances; 
            }
        }

        public DateTime ExpirationTime
        {
            get { return this.expirationTime; }
        }

        internal bool IsDisposable
        {
            get { return this.disposable; }
        }

        internal UnconditionalPolicy Clone()
        {
            ThrowIfDisposed();
            return (this.disposable) ? new UnconditionalPolicy(this) : this;
        }

        public virtual void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                SecurityUtils.DisposeIfNecessary(this.primaryIdentity as WindowsIdentity);
                SecurityUtils.DisposeClaimSetIfNecessary(this.issuance);
                SecurityUtils.DisposeClaimSetsIfNecessary(this.issuances);
            }
        }

        void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }

        public virtual bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            ThrowIfDisposed();
            if (this.issuance != null)
            {
                evaluationContext.AddClaimSet(this, this.issuance);
            }
            else
            {
                for (int i = 0; i < this.issuances.Count; ++i)
                {
                    if (this.issuances[i] != null)
                    {
                        evaluationContext.AddClaimSet(this, this.issuances[i]);
                    }
                }
            }

            // Preferably Non-Anonymous
            if (this.PrimaryIdentity != null && this.PrimaryIdentity != SecurityUtils.AnonymousIdentity)
            {
                IList<IIdentity> identities;
                object obj;
                if (!evaluationContext.Properties.TryGetValue(SecurityUtils.Identities, out obj))
                {
                    identities = new List<IIdentity>(1);
                    evaluationContext.Properties.Add(SecurityUtils.Identities, identities);
                }
                else
                {
                    // null if other overrides the property with something else
                    identities = obj as IList<IIdentity>;
                }

                if (identities != null)
                {
                    identities.Add(this.PrimaryIdentity);
                }
            }

            evaluationContext.RecordExpirationTime(this.expirationTime);
            return true;
        }
    }
}
