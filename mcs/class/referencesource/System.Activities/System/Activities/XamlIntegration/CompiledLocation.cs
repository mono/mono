//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Collections.Generic;

    [DataContract(Name = XD.CompiledLocation.Name, Namespace = XD.Runtime.Namespace)]
    internal class CompiledLocation<T> : Location<T>
    {
        Func<T> getMethod;
        Action<T> setMethod;

        int expressionId;

        IList<LocationReference> locationReferences;

        IList<Location> locations;

        ActivityInstance rootInstance;

        Activity compiledRootActivity;
        byte[] compiledRootActivityQualifiedId;

        Activity expressionActivity;
        byte[] expressionActivityQualifiedId;

        string expressionText;

        bool forImplementation;

        public CompiledLocation(Func<T> getMethod, Action<T> setMethod, IList<LocationReference> locationReferences, IList<Location> locations, int expressionId, Activity compiledRootActivity, ActivityContext currentActivityContext)
        {
            this.getMethod = getMethod;
            this.setMethod = setMethod;

            this.forImplementation = currentActivityContext.Activity.MemberOf != currentActivityContext.Activity.RootActivity.MemberOf;
            this.locationReferences = locationReferences;
            this.locations = locations;
            this.expressionId = expressionId;

            this.compiledRootActivity = compiledRootActivity;
            this.expressionActivity = currentActivityContext.Activity;
            //
            // Save the root activity instance to get the root activity post persistence
            // The root will always be alive as long as the location is valid, which is not
            // true for the activity instance of the expression that is executing
            this.rootInstance = currentActivityContext.CurrentInstance;
            while (this.rootInstance.Parent != null)
            {
                this.rootInstance = this.rootInstance.Parent;
            }
            //
            // Save the text of the expression for exception message
            ITextExpression textExpression = currentActivityContext.Activity as ITextExpression;
            if (textExpression != null)
            {
                this.expressionText = textExpression.ExpressionText;
            }
        }

        public CompiledLocation(Func<T> getMethod, Action<T> setMethod)
        {
            //
            // This is the constructor that is used to refresh the get/set methods during rehydration
            // An instance of this class created with the constructor cannot be invoked.
            this.getMethod = getMethod;
            this.setMethod = setMethod;
        }

        public override T Value
        {
            get
            {
                if (this.getMethod == null)
                {
                    RefreshAccessors();
                }
                return getMethod();
            }
            set
            {
                if (this.setMethod == null)
                {
                    RefreshAccessors();
                }
                setMethod(value);
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public byte[] CompiledRootActivityQualifiedId
        {
            get
            {
                if (this.compiledRootActivityQualifiedId == null)
                {
                    return this.compiledRootActivity.QualifiedId.AsByteArray();
                }

                return this.compiledRootActivityQualifiedId;
            }
            set
            {
                this.compiledRootActivityQualifiedId = value;
            }
        }
        
        [DataMember(EmitDefaultValue = false)]
        public byte[] ExpressionActivityQualifiedId
        {
            get
            {
                if (this.expressionActivityQualifiedId == null)
                {
                    return this.expressionActivity.QualifiedId.AsByteArray();
                }

                return this.expressionActivityQualifiedId;
            }
            set
            {
                this.expressionActivityQualifiedId = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public IList<Tuple<string, Type>> locationReferenceCache
        {
            get
            {
                if (this.locationReferences == null || this.locationReferences.Count == 0)
                {
                    return null;
                }

                List<Tuple<string, Type>> durableCache = new List<Tuple<string, Type>>(this.locationReferences.Count);

                foreach (LocationReference reference in locationReferences)
                {
                    durableCache.Add(new Tuple<string, Type>(reference.Name, reference.Type));
                }

                return durableCache;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    this.locationReferences = new List<LocationReference>();
                    return;
                }

                this.locationReferences = new List<LocationReference>(value.Count);
                foreach (Tuple<string, Type> reference in value)
                {
                    this.locationReferences.Add(new CompiledLocationReference(reference.Item1, reference.Item2));
                }
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "expressionId")]
        internal int SerializedExpressionId
        {
            get { return this.expressionId; }
            set { this.expressionId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "locations")]
        internal IList<Location> SerializedLocations
        {
            get { return this.locations; }
            set { this.locations = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "rootInstance")]
        internal ActivityInstance SerializedRootInstance
        {
            get { return this.rootInstance; }
            set { this.rootInstance = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "expressionText")]
        internal string SerializedExpressionText
        {
            get { return this.expressionText; }
            set { this.expressionText = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "forImplementation")]
        internal bool SerializedForImplementation
        {
            get { return this.forImplementation; }
            set { this.forImplementation = value; }
        }

        void RefreshAccessors()
        {
            //
            // If we've gotten here is means that we have a location that has roundtripped through persistence
            // CompiledDataContext & ICER don't round trip so we need to get them back from the current tree 
            // and get new pointers to the get/set methods for this expression
            ICompiledExpressionRoot compiledRoot = GetCompiledExpressionRoot();
            CompiledLocation<T> tempLocation = (CompiledLocation<T>)compiledRoot.InvokeExpression(this.expressionId, this.locations);
            this.getMethod = tempLocation.getMethod;
            this.setMethod = tempLocation.setMethod;
        }

        ICompiledExpressionRoot GetCompiledExpressionRoot()
        {
            if (this.rootInstance != null && this.rootInstance.Activity != null)
            {
                ICompiledExpressionRoot compiledExpressionRoot;
                Activity rootActivity = this.rootInstance.Activity;

                Activity compiledRootActivity = null;
                Activity expressionActivity = null;
                
                if (QualifiedId.TryGetElementFromRoot(rootActivity, this.compiledRootActivityQualifiedId, out compiledRootActivity) &&
                    QualifiedId.TryGetElementFromRoot(rootActivity, this.expressionActivityQualifiedId, out expressionActivity))
                {
                    if (CompiledExpressionInvoker.TryGetCompiledExpressionRoot(expressionActivity, compiledRootActivity, out compiledExpressionRoot))
                    {
                        //
                        // Revalidate to make sure we didn't hit an ID shift
                        if (compiledExpressionRoot.CanExecuteExpression(this.expressionText, true /* this is always a reference */, this.locationReferences, out this.expressionId))
                        {
                            return compiledExpressionRoot;
                        }
                    }
                }
                //
                // We were valid when this location was generated so an ID shift occurred (likely due to a dynamic update)
                // Need to search all of the ICERs for one that can execute this expression.
                if (FindCompiledExpressionRoot(rootActivity, out compiledExpressionRoot))
                {
                    return compiledExpressionRoot;
                }
            }
            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnableToLocateCompiledLocationContext(this.expressionText)));
        }

        bool FindCompiledExpressionRoot(Activity activity, out ICompiledExpressionRoot compiledExpressionRoot)
        {
            if (CompiledExpressionInvoker.TryGetCompiledExpressionRoot(activity, this.forImplementation, out compiledExpressionRoot))
            {
                if (compiledExpressionRoot.CanExecuteExpression(this.expressionText, true /* this is always a reference */, this.locationReferences, out this.expressionId))
                {
                    return true;
                }
            }

            foreach (Activity containedActivity in WorkflowInspectionServices.GetActivities(activity))
            {
                if (FindCompiledExpressionRoot(containedActivity, out compiledExpressionRoot))
                {
                    return true;
                }
            }

            compiledExpressionRoot = null;
            return false;
        }

        class CompiledLocationReference : LocationReference
        {
            string name;
            Type type;

            public CompiledLocationReference(string name, Type type)
            {
                this.name = name;
                this.type = type;
            }

            protected override string NameCore
            {
                get 
                {
                    return name;
                }
            }

            protected override Type TypeCore
            {
                get 
                {
                    return type;
                }
            }

            public override Location GetLocation(ActivityContext context)
            {
                //
                // We should never hit this, these references are strictly for preserving location names/types
                // through persistence to allow for revalidation on the other side
                // Actual execution occurs through the locations that were stored separately
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompiledLocationReferenceGetLocation));
            }
        }
    }
}
