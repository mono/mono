using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Data.Linq {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;

    internal abstract class ChangeTracker {
        /// <summary>
        /// Starts tracking an object as 'unchanged'
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal abstract TrackedObject Track(object obj);
        /// <summary>
        /// Starts tracking an object as 'unchanged', and optionally
        /// 'weakly' tracks all other referenced objects recursively.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recurse">True if all untracked objects in the graph
        /// should be tracked recursively.</param>
        /// <returns></returns>
        internal abstract TrackedObject Track(object obj, bool recurse);
        /// <summary>
        /// Fast-tracks an object that is already in identity cache
        /// </summary>
        /// <param name="obj"></param>
        internal abstract void FastTrack(object obj);
        internal abstract bool IsTracked(object obj);
        internal abstract TrackedObject GetTrackedObject(object obj);
        internal abstract void StopTracking(object obj);
        internal abstract void AcceptChanges();
        internal abstract IEnumerable<TrackedObject> GetInterestingObjects();

        internal static ChangeTracker CreateChangeTracker(CommonDataServices dataServices, bool asReadOnly) {
            if (asReadOnly) {
                return new ReadOnlyChangeTracker();
            }
            else {
                return new StandardChangeTracker(dataServices);
            }
        }

        class StandardChangeTracker : ChangeTracker {
            Dictionary<object, StandardTrackedObject> items;
            PropertyChangingEventHandler onPropertyChanging;
            CommonDataServices services;

            internal StandardChangeTracker(CommonDataServices services) {
                this.services = services;
                this.items = new Dictionary<object, StandardTrackedObject>();
                this.onPropertyChanging = new PropertyChangingEventHandler(this.OnPropertyChanging);
            }

            /// <summary>
            /// Given a type root and a discriminator, return the type that would be instantiated.
            /// </summary>
            private static MetaType TypeFromDiscriminator(MetaType root, object discriminator) {
                foreach (MetaType type in root.InheritanceTypes) {
                    if (IsSameDiscriminator(discriminator, type.InheritanceCode))
                        return type;
                }
                return root.InheritanceDefault;
            }

            private static bool IsSameDiscriminator(object discriminator1, object discriminator2) {
                if (discriminator1 == discriminator2) {
                    return true;
                }
                if (discriminator1 == null || discriminator2 == null) {
                    return false;
                }
                return discriminator1.Equals(discriminator2);
            }

            internal override TrackedObject Track(object obj) {
                return Track(obj, false);
            }

            internal override TrackedObject Track(object obj, bool recurse) {
                MetaType type = this.services.Model.GetMetaType(obj.GetType());
                Dictionary<object, object> visited = new Dictionary<object, object>();
                return Track(type, obj, visited, recurse, 1);
            }

            private TrackedObject Track(MetaType mt, object obj, Dictionary<object, object> visited, bool recurse, int level) {
                StandardTrackedObject tracked = (StandardTrackedObject)this.GetTrackedObject(obj);
                if (tracked != null || visited.ContainsKey(obj)) {
                    return tracked;
                }

                // The root object tracked is tracked normally - all other objects
                // in the reference graph are weakly tracked.
                bool weaklyTrack = level > 1;
                tracked = new StandardTrackedObject(this, mt, obj, obj, weaklyTrack);
                if (tracked.HasDeferredLoaders) {
                    throw Error.CannotAttachAddNonNewEntities();
                }
                this.items.Add(obj, tracked);
                this.Attach(obj);
                visited.Add(obj, obj);

                if (recurse) {
                    // track parents (objects we are dependent on)
                    foreach (RelatedItem parent in this.services.GetParents(mt, obj)) {
                        this.Track(parent.Type, parent.Item, visited, recurse, level + 1);
                    }

                    // track children (objects that are dependent on us)
                    foreach (RelatedItem child in this.services.GetChildren(mt, obj)) {
                        this.Track(child.Type, child.Item, visited, recurse, level + 1);
                    }
                }

                return tracked;
            }

            internal override void FastTrack(object obj) {
                // assumes object is already in identity cache
                this.Attach(obj);
            }

            internal override void StopTracking(object obj) {
                this.Detach(obj);
                this.items.Remove(obj);
            }

            internal override bool IsTracked(object obj) {
                return this.items.ContainsKey(obj) || this.IsFastTracked(obj);
            }

            private bool IsFastTracked(object obj) {
                MetaType type = this.services.Model.GetTable(obj.GetType()).RowType;
                return this.services.IsCachedObject(type, obj);
            }

            internal override TrackedObject GetTrackedObject(object obj) {
                StandardTrackedObject ti;
                if (!this.items.TryGetValue(obj, out ti)) {
                    if (this.IsFastTracked(obj)) {
                        return this.PromoteFastTrackedObject(obj);
                    }
                }
                return ti;
            }

            private StandardTrackedObject PromoteFastTrackedObject(object obj) {
                Type type = obj.GetType();
                MetaType metaType = this.services.Model.GetTable(type).RowType.GetInheritanceType(type);
                return this.PromoteFastTrackedObject(metaType, obj);
            }

            private StandardTrackedObject PromoteFastTrackedObject(MetaType type, object obj) {
                StandardTrackedObject ti = new StandardTrackedObject(this, type, obj, obj);
                this.items.Add(obj, ti);
                return ti;
            }

            private void Attach(object obj) {
                INotifyPropertyChanging notifier = obj as INotifyPropertyChanging;
                if (notifier != null) {
                    notifier.PropertyChanging += this.onPropertyChanging;
                }
                else {
                    // if has no notifier, consider it modified already
                    this.OnPropertyChanging(obj, null);
                }
            }

            private void Detach(object obj) {
                INotifyPropertyChanging notifier = obj as INotifyPropertyChanging;
                if (notifier != null) {
                    notifier.PropertyChanging -= this.onPropertyChanging;
                }
            }

            private void OnPropertyChanging(object sender, PropertyChangingEventArgs args) {
                StandardTrackedObject ti;
                if (this.items.TryGetValue(sender, out ti)) {
                    ti.StartTracking();
                }
                else if (this.IsFastTracked(sender)) {
                    ti = this.PromoteFastTrackedObject(sender);
                    ti.StartTracking();
                }
            }

            internal override void AcceptChanges() {
                List<StandardTrackedObject> list = new List<StandardTrackedObject>((IEnumerable<StandardTrackedObject>)this.items.Values);
                foreach (TrackedObject item in list) {
                    item.AcceptChanges();
                }
            }

            internal override IEnumerable<TrackedObject> GetInterestingObjects() {
                foreach (StandardTrackedObject ti in this.items.Values) {
                    if (ti.IsInteresting) {
                        yield return ti;
                    }
                }
            }

            class StandardTrackedObject : TrackedObject {
                private StandardChangeTracker tracker;
                private MetaType type;
                private object current;
                private object original;
                private State state;
                private BitArray dirtyMemberCache;
                private bool haveInitializedDeferredLoaders;
                private bool isWeaklyTracked;                

                enum State {
                    New,
                    Deleted,
                    PossiblyModified,
                    Modified,
                    Removed,
                    Dead
                }

                public override string ToString() {
                    return type.Name + ":" + GetState();
                }

                private string GetState() {
                    switch (this.state) {
                        case State.New:
                        case State.Deleted:
                        case State.Dead:
                        case State.Removed:
                            return this.state.ToString();
                        default:
                            if (this.IsModified) {
                                return "Modified";
                            }
                            else {
                                return "Unmodified";
                            }
                    }
                }

                internal StandardTrackedObject(StandardChangeTracker tracker, MetaType type, object current, object original) {
                    if (current == null) {
                        throw Error.ArgumentNull("current");
                    }
                    this.tracker = tracker;
                    this.type = type.GetInheritanceType(current.GetType());
                    this.current = current;
                    this.original = original;
                    this.state = State.PossiblyModified;
                    dirtyMemberCache = new BitArray(this.type.DataMembers.Count);
                }

                internal StandardTrackedObject(StandardChangeTracker tracker, MetaType type, object current, object original, bool isWeaklyTracked)
                    : this(tracker, type, current, original) {
                    this.isWeaklyTracked = isWeaklyTracked;
                }

                internal override bool IsWeaklyTracked {
                    get { return isWeaklyTracked; }
                }

                internal override MetaType Type {
                    get { return this.type; }
                }

                internal override object Current {
                    get { return this.current; }
                }

                internal override object Original {
                    get { return this.original; }
                }

                internal override bool IsNew {
                    get { return this.state == State.New; }
                }

                internal override bool IsDeleted {
                    get { return this.state == State.Deleted; }
                }

                internal override bool IsRemoved {
                    get { return this.state == State.Removed; }
                }

                internal override bool IsDead {
                    get { return this.state == State.Dead; }
                }

                internal override bool IsModified {
                    get { return this.state == State.Modified || (this.state == State.PossiblyModified && this.current != this.original && this.HasChangedValues()); }
                }

                internal override bool IsUnmodified {
                    get { return this.state == State.PossiblyModified && (this.current == this.original || !this.HasChangedValues()); }
                }

                internal override bool IsPossiblyModified {
                    get { return this.state == State.Modified || this.state == State.PossiblyModified; }
                }

                internal override bool CanInferDelete() {
                    // A delete can be inferred iff there is a non-nullable singleton association that has 
                    // been set to null, and the association has DeleteOnNull = true.
                    if (this.state == State.Modified || this.state == State.PossiblyModified) {
                        foreach (MetaAssociation assoc in Type.Associations) {
                            if (assoc.DeleteOnNull && assoc.IsForeignKey && !assoc.IsNullable && !assoc.IsMany &&
                                assoc.ThisMember.StorageAccessor.HasAssignedValue(Current) &&
                                assoc.ThisMember.StorageAccessor.GetBoxedValue(Current) == null) {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                internal override bool IsInteresting {
                    get {
                        return this.state == State.New ||
                               this.state == State.Deleted ||
                               this.state == State.Modified ||
                               (this.state == State.PossiblyModified && this.current != this.original) ||
                               CanInferDelete();
                    }
                }

                internal override void ConvertToNew() {
                    // must be new or unmodified or removed to convert to new
                    System.Diagnostics.Debug.Assert(this.IsNew || this.IsRemoved || this.IsUnmodified);
                    this.original = null;
                    this.state = State.New;
                }

                internal override void ConvertToPossiblyModified() {
                    System.Diagnostics.Debug.Assert(this.IsPossiblyModified || this.IsDeleted);
                    this.state = State.PossiblyModified;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToModified() {
                    System.Diagnostics.Debug.Assert(this.IsPossiblyModified);
                    System.Diagnostics.Debug.Assert(this.type.VersionMember != null || !this.type.HasUpdateCheck);
                    this.state = State.Modified;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToPossiblyModified(object originalState) {
                    // must be modified or unmodified to convert to modified
                    System.Diagnostics.Debug.Assert(this.IsNew || this.IsPossiblyModified);
                    System.Diagnostics.Debug.Assert(originalState != null);
                    System.Diagnostics.Debug.Assert(originalState.GetType() == this.type.Type);
                    this.state = State.PossiblyModified;
                    this.original = this.CreateDataCopy(originalState);
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToDeleted() {
                    // must be modified or unmodified to be deleted
                    System.Diagnostics.Debug.Assert(this.IsDeleted || this.IsPossiblyModified);
                    this.state = State.Deleted;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToDead() {
                    System.Diagnostics.Debug.Assert(this.IsDead || this.IsDeleted);
                    this.state = State.Dead;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToRemoved() {
                    System.Diagnostics.Debug.Assert(this.IsRemoved || this.IsNew);
                    this.state = State.Removed;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToUnmodified() {
                    System.Diagnostics.Debug.Assert(this.IsNew || this.IsPossiblyModified);
                    // reset to unmodified
                    this.state = State.PossiblyModified;
                    if (this.current is INotifyPropertyChanging) {
                        this.original = this.current;
                    }
                    else {
                        this.original = this.CreateDataCopy(this.current);
                    }
                    this.ResetDirtyMemberTracking();
                    this.isWeaklyTracked = false;
                }

                internal override void AcceptChanges() {
                    if (IsWeaklyTracked) {
                        InitializeDeferredLoaders();
                        isWeaklyTracked = false;
                    }
                    if (this.IsDeleted) {
                        this.ConvertToDead();
                    }
                    else if (this.IsNew) {
                        this.InitializeDeferredLoaders();
                        this.ConvertToUnmodified();
                    }
                    else if (this.IsPossiblyModified) {
                        this.ConvertToUnmodified();
                    }
                }

                private void AssignMember(object instance, MetaDataMember mm, object value) {
                    // In the unnotified case, directly use the storage accessor
                    // for everything because there are not events to be fired.
                    if (!(this.current is INotifyPropertyChanging)) {
                        mm.StorageAccessor.SetBoxedValue(ref instance, value);
                    }
                    else {
                        // Go through the member accessor to fire events.
                        mm.MemberAccessor.SetBoxedValue(ref instance, value);
                    }
                }

                /// <summary>
                /// Certain state is saved during change tracking to enable modifications
                /// to be detected taking refresh operations into account.  When changes
                /// are reverted or accepted, this state must be reset.
                /// </summary>
                private void ResetDirtyMemberTracking() {
                    this.dirtyMemberCache.SetAll(false);
                }

                /// <summary>
                /// Refresh internal tracking state using the original value and mode
                /// specified.
                /// </summary>        
                internal override void Refresh(RefreshMode mode, object freshInstance) {
                    this.SynchDependentData();

                    // This must be done prior to updating original values
                    this.UpdateDirtyMemberCache();

                    // Apply the refresh strategy to each data member
                    Type instanceType = freshInstance.GetType();
                    foreach (MetaDataMember mm in type.PersistentDataMembers) {
                        var memberMode = mm.IsDbGenerated ? RefreshMode.OverwriteCurrentValues : mode;
                        if (memberMode != RefreshMode.KeepCurrentValues) {
                            if (!mm.IsAssociation && (this.Type.Type == instanceType || mm.DeclaringType.Type.IsAssignableFrom(instanceType))) {
                                object freshValue = mm.StorageAccessor.GetBoxedValue(freshInstance);
                                this.RefreshMember(mm, memberMode, freshValue);
                            }
                        }
                    }

                    // Make the new data the current original value
                    this.original = this.CreateDataCopy(freshInstance);

                    if (mode == RefreshMode.OverwriteCurrentValues) {
                        this.ResetDirtyMemberTracking();
                    }
                }

                /// <summary>
                /// Using the last saved comparison baseline, figure out which members have
                /// changed since the last refresh, and save that information.  This must be
                /// done BEFORE any merge operations modify the current values.
                /// </summary>
                private void UpdateDirtyMemberCache() {
                    // iterate over all members, and if they differ from 
                    // last read values, mark as dirty           
                    foreach (MetaDataMember mm in type.PersistentDataMembers) {
                        if (mm.IsAssociation && mm.Association.IsMany) {
                            continue;
                        }
                        if (!this.dirtyMemberCache.Get(mm.Ordinal) && this.HasChangedValue(mm)) {
                            this.dirtyMemberCache.Set(mm.Ordinal, true);
                        }
                    }
                }

                internal override void RefreshMember(MetaDataMember mm, RefreshMode mode, object freshValue) {
                    System.Diagnostics.Debug.Assert(!mm.IsAssociation);

                    if (mode == RefreshMode.KeepCurrentValues) {
                        return;
                    }

                    bool hasUserChange = this.HasChangedValue(mm);

                    // we don't want to overwrite any modified values, unless
                    // the mode is original wins                
                    if (hasUserChange && mode != RefreshMode.OverwriteCurrentValues)
                        return;

                    object currentValue = mm.StorageAccessor.GetBoxedValue(this.current);
                    if (!object.Equals(freshValue, currentValue)) {
                        mm.StorageAccessor.SetBoxedValue(ref this.current, freshValue);

                        // update all singleton associations that are affected by a change to this member
                        foreach (MetaDataMember am in this.GetAssociationsForKey(mm)) {
                            if (!am.Association.IsMany) {
                                IEnumerable ds = this.tracker.services.GetDeferredSourceFactory(am).CreateDeferredSource(this.current);
                                if (am.StorageAccessor.HasValue(this.current)) {
                                    this.AssignMember(this.current, am, ds.Cast<Object>().SingleOrDefault());
                                }
                            }
                        }
                    }
                }

                private IEnumerable<MetaDataMember> GetAssociationsForKey(MetaDataMember key) {
                    foreach (MetaDataMember mm in this.type.PersistentDataMembers) {
                        if (mm.IsAssociation && mm.Association.ThisKey.Contains(key)) {
                            yield return mm;
                        }
                    }
                }

                internal override object CreateDataCopy(object instance) {
                    System.Diagnostics.Debug.Assert(instance != null);
                    Type instanceType = instance.GetType();
                    System.Diagnostics.Debug.Assert(instance.GetType() == this.type.Type);

                    object copy = Activator.CreateInstance(this.Type.Type);

                    MetaType rootMetaType = this.tracker.services.Model.GetTable(instanceType).RowType.InheritanceRoot;
                    foreach (MetaDataMember mm in rootMetaType.GetInheritanceType(instanceType).PersistentDataMembers) {
                        if (this.Type.Type != instanceType && !mm.DeclaringType.Type.IsAssignableFrom(instanceType)) {
                            continue;
                        }
                        if (mm.IsDeferred) {
                            // do not copy associations
                            if (!mm.IsAssociation) {
                                if (mm.StorageAccessor.HasValue(instance)) {
                                    object value = mm.DeferredValueAccessor.GetBoxedValue(instance);
                                    mm.DeferredValueAccessor.SetBoxedValue(ref copy, value);
                                }
                                else {
                                    IEnumerable ds = this.tracker.services.GetDeferredSourceFactory(mm).CreateDeferredSource(copy);
                                    mm.DeferredSourceAccessor.SetBoxedValue(ref copy, ds);
                                }
                            }
                        }
                        else {
                            // otherwise assign the value as-is to the backup instance
                            object value = mm.StorageAccessor.GetBoxedValue(instance);
                            // assumes member values are immutable or will communicate changes to entity
                            // note: byte[] and char[] don't do this. 
                            mm.StorageAccessor.SetBoxedValue(ref copy, value);
                        }
                    }
                    return copy;
                }

                internal void StartTracking() {
                    if (this.original == this.current) {
                        this.original = this.CreateDataCopy(this.current);
                    }
                }

                // Return value indicates whether or not any data was actually [....]'d
                internal override bool SynchDependentData() {                    
                    bool valueWasSet = false;

                    // set foreign key fields
                    foreach (MetaAssociation assoc in this.Type.Associations) {
                        MetaDataMember mm = assoc.ThisMember;
                        if (assoc.IsForeignKey) {
                            bool hasAssigned = mm.StorageAccessor.HasAssignedValue(this.current);
                            bool hasLoaded = mm.StorageAccessor.HasLoadedValue(this.current);
                            if (hasAssigned || hasLoaded) {
                                object parent = mm.StorageAccessor.GetBoxedValue(this.current);
                                if (parent != null) {
                                    // copy parent's current primary key into this instance's foreign key fields
                                    for (int i = 0, n = assoc.ThisKey.Count; i < n; i++) {
                                        MetaDataMember accThis = assoc.ThisKey[i];
                                        MetaDataMember accParent = assoc.OtherKey[i];
                                        object parentValue = accParent.StorageAccessor.GetBoxedValue(parent);
                                        accThis.StorageAccessor.SetBoxedValue(ref this.current, parentValue);
                                        valueWasSet = true;
                                    }
                                }
                                else if (assoc.IsNullable) {
                                    if (mm.IsDeferred || (this.original != null && mm.MemberAccessor.GetBoxedValue(this.original) != null)) {
                                        // no known parent? set to null
                                        for (int i = 0, n = assoc.ThisKey.Count; i < n; i++) {
                                            MetaDataMember accThis = assoc.ThisKey[i];
                                            if (accThis.CanBeNull) {
                                                if (this.original != null && this.HasChangedValue(accThis)) {
                                                    if (accThis.StorageAccessor.GetBoxedValue(this.current) != null) {
                                                        throw Error.InconsistentAssociationAndKeyChange(accThis.Member.Name, mm.Member.Name);
                                                    }
                                                }
                                                else {
                                                    accThis.StorageAccessor.SetBoxedValue(ref this.current, null);
                                                    valueWasSet = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (!hasLoaded) {
                                    //Else the parent association has been set to null; but the ID is not nullable so
                                    //the value can not be set
                                    StringBuilder keys = new StringBuilder();
                                    foreach (MetaDataMember key in assoc.ThisKey) {
                                        if (keys.Length > 0) {
                                            keys.Append(", ");
                                        }
                                        keys.AppendFormat("{0}.{1}", this.Type.Name.ToString(), key.Name);
                                    }
                                    throw Error.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(assoc.OtherType.Name, this.Type.Name, keys);
                                }
                            }
                        }
                    }

                    /// Explicitly set any inheritance discriminator for item.
                    if (this.type.HasInheritance) {
                        if (this.original != null) {
                            object currentDiscriminator = type.Discriminator.MemberAccessor.GetBoxedValue(this.current);
                            MetaType currentTypeFromDiscriminator = TypeFromDiscriminator(this.type, currentDiscriminator);
                            object dbDiscriminator = type.Discriminator.MemberAccessor.GetBoxedValue(this.original);
                            MetaType dbTypeFromDiscriminator = TypeFromDiscriminator(this.type, dbDiscriminator);

                            // Would the discriminator change also change the type? If so, its not allowed.
                            if (currentTypeFromDiscriminator != dbTypeFromDiscriminator) {
                                throw Error.CannotChangeInheritanceType(dbDiscriminator,
                                    currentDiscriminator, original.GetType().Name, currentTypeFromDiscriminator);
                            }
                        }
                        else {
                            // No db value means this is an 'Add'. Set the discriminator.
                            MetaType currentType = type.GetInheritanceType(this.current.GetType());
                            if (currentType.HasInheritanceCode) {
                                object code = currentType.InheritanceCode;
                                this.type.Discriminator.MemberAccessor.SetBoxedValue(ref current, code);
                                valueWasSet = true;
                            }
                        }
                    }
                    return valueWasSet;
                }

                internal override bool HasChangedValue(MetaDataMember mm) {
                    if (this.current == this.original) {
                        return false;
                    }
                    if (mm.IsAssociation && mm.Association.IsMany) {
                        return mm.StorageAccessor.HasAssignedValue(this.original);
                    }
                    if (mm.StorageAccessor.HasValue(this.current)) {
                        if (this.original != null && mm.StorageAccessor.HasValue(this.original)) {
                            // If the member has ever been in a modified state
                            // in the past, it is considered modified
                            if (dirtyMemberCache.Get(mm.Ordinal)) {
                                return true;
                            }
                            object baseline = mm.MemberAccessor.GetBoxedValue(this.original);
                            object currentValue = mm.MemberAccessor.GetBoxedValue(this.current);
                            if (!object.Equals(currentValue, baseline)) {
                                return true;
                            }
                            return false;
                        }
                        else if (mm.IsDeferred && mm.StorageAccessor.HasAssignedValue(this.current)) {
                            return true;
                        }
                    }
                    return false;
                }

                internal override bool HasChangedValues() {
                    if (this.current == this.original) {
                        return false;
                    }
                    if (this.IsNew) {
                        return true;
                    }
                    foreach (MetaDataMember mm in this.type.PersistentDataMembers) {
                        if (!mm.IsAssociation && this.HasChangedValue(mm)) {
                            return true;
                        }
                    }
                    return false;
                }

                internal override IEnumerable<ModifiedMemberInfo> GetModifiedMembers() {
                    foreach (MetaDataMember mm in this.type.PersistentDataMembers) {
                        if (this.IsModifiedMember(mm)) {
                            object currentValue = mm.MemberAccessor.GetBoxedValue(this.current);
                            if (this.original != null && mm.StorageAccessor.HasValue(this.original)) {
                                object originalValue = mm.MemberAccessor.GetBoxedValue(this.original);
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, originalValue);
                            }
                            else if (this.original == null || (mm.IsDeferred && !mm.StorageAccessor.HasLoadedValue(this.current))) {
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, null);
                            }
                        }
                    }
                }

                private bool IsModifiedMember(MetaDataMember member) {
                    return !member.IsAssociation &&
                           !member.IsPrimaryKey &&
                           !member.IsVersion &&
                           !member.IsDbGenerated &&
                            member.StorageAccessor.HasAssignedValue(this.current) &&
                           (this.state == State.Modified ||
                           (this.state == State.PossiblyModified && this.HasChangedValue(member)));
                }

                internal override bool HasDeferredLoaders {
                    get {
                        foreach (MetaAssociation assoc in this.Type.Associations) {
                            if (HasDeferredLoader(assoc.ThisMember)) {
                                return true;
                            }
                        }
                        IEnumerable<MetaDataMember> deferredMembers = this.Type.PersistentDataMembers.Where(p => p.IsDeferred && !p.IsAssociation);
                        foreach (MetaDataMember deferredMember in deferredMembers) {
                            if (HasDeferredLoader(deferredMember)) {
                                return true;
                            }
                        }
                        return false;
                    }
                }

                private bool HasDeferredLoader(MetaDataMember deferredMember) {
                    if (!deferredMember.IsDeferred) {
                        return false;
                    }

                    MetaAccessor acc = deferredMember.StorageAccessor;
                    if (acc.HasAssignedValue(this.current) || acc.HasLoadedValue(this.current)) {
                        return false;
                    }
                    MetaAccessor dsacc = deferredMember.DeferredSourceAccessor;
                    IEnumerable loader = (IEnumerable)dsacc.GetBoxedValue(this.current);

                    return loader != null;
                }

                /// <summary>
                /// Called to initialize deferred loaders for New or Attached entities.
                /// </summary>
                internal override void InitializeDeferredLoaders() {
                    if (this.tracker.services.Context.DeferredLoadingEnabled) {
                        foreach (MetaAssociation assoc in this.Type.Associations) {
                            // don't set loader on association that is dependent on unrealized generated values
                            if (!this.IsPendingGeneration(assoc.ThisKey)) {
                                InitializeDeferredLoader(assoc.ThisMember);
                            }
                        }
                        IEnumerable<MetaDataMember> deferredMembers = this.Type.PersistentDataMembers.Where(p => p.IsDeferred && !p.IsAssociation);
                        foreach (MetaDataMember deferredMember in deferredMembers) {
                            // don't set loader on member that is dependent on unrealized generated values
                            if (!this.IsPendingGeneration(Type.IdentityMembers)) {
                                InitializeDeferredLoader(deferredMember);
                            }
                        }
                        haveInitializedDeferredLoaders = true;
                    }
                }

                private void InitializeDeferredLoader(MetaDataMember deferredMember) {
                    MetaAccessor acc = deferredMember.StorageAccessor;
                    if (!acc.HasAssignedValue(this.current) && !acc.HasLoadedValue(this.current)) {
                        MetaAccessor dsacc = deferredMember.DeferredSourceAccessor;
                        IEnumerable loader = (IEnumerable)dsacc.GetBoxedValue(this.current);
                        // don't reset loader on any deferred member that already has one
                        if (loader == null) {
                            IDeferredSourceFactory factory = this.tracker.services.GetDeferredSourceFactory(deferredMember);
                            loader = factory.CreateDeferredSource(this.current);
                            dsacc.SetBoxedValue(ref this.current, loader);

                        }
                        else if (loader != null && !haveInitializedDeferredLoaders) {
                            // If loader is present but wasn't generated by us, then
                            // an attempt to Attach or Add an entity from another context
                            // has been made, which is not supported.
                            throw Error.CannotAttachAddNonNewEntities();
                        }
                    }
                }

                internal override bool IsPendingGeneration(IEnumerable<MetaDataMember> key) {
                    if (this.IsNew) {
                        foreach (MetaDataMember member in key) {
                            if (IsMemberPendingGeneration(member)) {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                internal override bool IsMemberPendingGeneration(MetaDataMember keyMember) {
                    if (this.IsNew && keyMember.IsDbGenerated) {
                        return true;
                    }
                    // look for any FK association that has this key member (should only be one)
                    foreach (MetaAssociation assoc in type.Associations) {
                        if (assoc.IsForeignKey) {
                            int index = assoc.ThisKey.IndexOf(keyMember);
                            if (index > -1) {
                                // we must have a reference to this other object to know if its side of 
                                // the association is generated or not
                                object otherItem = null;
                                if (assoc.ThisMember.IsDeferred) {
                                    otherItem = assoc.ThisMember.DeferredValueAccessor.GetBoxedValue(this.current);
                                }
                                else {
                                    otherItem = assoc.ThisMember.StorageAccessor.GetBoxedValue(this.current);
                                }
                                if (otherItem != null) {
                                    if (assoc.IsMany) {
                                        // Can't be pending generation for a value that would have to be the same
                                        // across many rows.
                                        continue;
                                    }
                                    else {
                                        StandardTrackedObject trackedOther = (StandardTrackedObject)this.tracker.GetTrackedObject(otherItem);
                                        if (trackedOther != null) {
                                            MetaDataMember otherMember = assoc.OtherKey[index];
                                            return trackedOther.IsMemberPendingGeneration(otherMember);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// This is the implementation used when change tracking is disabled.
        /// </summary>
        class ReadOnlyChangeTracker : ChangeTracker {
            internal override TrackedObject Track(object obj) { return null; }
            internal override TrackedObject Track(object obj, bool recurse) { return null; }
            internal override void FastTrack(object obj) { }
            internal override bool IsTracked(object obj) { return false; }
            internal override TrackedObject GetTrackedObject(object obj) { return null; }
            internal override void StopTracking(object obj) { }
            internal override void AcceptChanges() { }
            internal override IEnumerable<TrackedObject> GetInterestingObjects() { return new TrackedObject[0]; }
        }
    }

    internal abstract class TrackedObject {
        internal abstract MetaType Type { get; }
        /// <summary>
        /// The current client value.
        /// </summary>
        internal abstract object Current { get; }
        /// <summary>
        /// The last read database value.  This is updated whenever the
        /// item is refreshed.
        /// </summary>
        internal abstract object Original { get; }
        internal abstract bool IsInteresting { get; } // new, deleted or possibly changed
        internal abstract bool IsNew { get; }
        internal abstract bool IsDeleted { get; }
        internal abstract bool IsModified { get; }
        internal abstract bool IsUnmodified { get; }
        internal abstract bool IsPossiblyModified { get; }
        internal abstract bool IsRemoved { get; }
        internal abstract bool IsDead { get; }
        /// <summary>
        /// True if the object is being tracked (perhaps during a recursive
        /// attach operation) but can be transitioned to other states.
        /// </summary>
        internal abstract bool IsWeaklyTracked { get; }
        internal abstract bool HasDeferredLoaders { get; }        
        internal abstract bool HasChangedValues();
        internal abstract IEnumerable<ModifiedMemberInfo> GetModifiedMembers();
        internal abstract bool HasChangedValue(MetaDataMember mm);
        internal abstract bool CanInferDelete();
        internal abstract void AcceptChanges();
        internal abstract void ConvertToNew();
        internal abstract void ConvertToPossiblyModified();
        internal abstract void ConvertToPossiblyModified(object original);
        internal abstract void ConvertToUnmodified();
        internal abstract void ConvertToModified();
        internal abstract void ConvertToDeleted();
        internal abstract void ConvertToRemoved();
        internal abstract void ConvertToDead();
        /// <summary>
        /// Refresh the item by making the value passed in the current 
        /// Database value, and refreshing the current values using the
        /// mode specified.
        /// </summary>       
        internal abstract void Refresh(RefreshMode mode, object freshInstance);
        /// <summary>
        /// Does the refresh operation for a single member.  This method does not 
        /// update the baseline 'original' value.  You must call 
        /// Refresh(RefreshMode.KeepCurrentValues, freshInstance) to finish the refresh 
        /// after refreshing individual members.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="mode"></param>
        /// <param name="freshValue"></param>
        internal abstract void RefreshMember(MetaDataMember member, RefreshMode mode, object freshValue);
        /// <summary>
        /// Create a data-member only copy of the instance (no associations)
        /// </summary>
        /// <returns></returns>
        internal abstract object CreateDataCopy(object instance);

        internal abstract bool SynchDependentData();

        internal abstract bool IsPendingGeneration(IEnumerable<MetaDataMember> keyMembers);
        internal abstract bool IsMemberPendingGeneration(MetaDataMember keyMember);

        internal abstract void InitializeDeferredLoaders();
    }
}
