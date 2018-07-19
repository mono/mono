//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System.Collections.Generic;
    using System.Activities.Debugger.Symbol;

    // Keep track of instrumentation information.
    // - which subroot has source file but not yet instrumented.
    // - which subroots share the same source file
    // SubRoot is defined as an activity that has a source file
    // (Custom Activity).
    class InstrumentationTracker
    {
        // Root of the workflow to keep track.
        Activity root;

        // Mapping of subroots to their source files.
        Dictionary<Activity, string> uninstrumentedSubRoots;

        Dictionary<Activity, string> UninstrumentedSubRoots
        {
            get
            {
                if (this.uninstrumentedSubRoots == null)
                {
                    InitializeUninstrumentedSubRoots();
                }
                return this.uninstrumentedSubRoots;
            }
        }

        public InstrumentationTracker(Activity root)
        {
            this.root = root;
        }

        // Initialize UninstrumentedSubRoots by traversing the workflow.
        void InitializeUninstrumentedSubRoots()
        {
            this.uninstrumentedSubRoots = new Dictionary<Activity, string>();

            Queue<Activity> activitiesRemaining = new Queue<Activity>();

            CollectSubRoot(this.root);
            activitiesRemaining.Enqueue(this.root);

            while (activitiesRemaining.Count > 0)
            {
                Activity toProcess = activitiesRemaining.Dequeue();

                foreach (Activity activity in WorkflowInspectionServices.GetActivities(toProcess))
                {
                    if (!uninstrumentedSubRoots.ContainsKey(activity))
                    {
                        CollectSubRoot(activity);
                        activitiesRemaining.Enqueue(activity);
                    }
                }
            }
        }

        // Collect subroot as uninstrumented activity.
        void CollectSubRoot(Activity activity)
        {
            string wfSymbol = DebugSymbol.GetSymbol(activity) as string;
            if (!string.IsNullOrEmpty(wfSymbol))
            {
                this.uninstrumentedSubRoots.Add(activity, wfSymbol);
            }
            else
            {
                string sourcePath = XamlDebuggerXmlReader.GetFileName(activity) as string;
                if (!string.IsNullOrEmpty(sourcePath))
                {
                    this.uninstrumentedSubRoots.Add(activity, sourcePath);
                }
            }
        }

        // Whether this is unistrumented sub root.
        public bool IsUninstrumentedSubRoot(Activity subRoot)
        {
            return this.UninstrumentedSubRoots.ContainsKey(subRoot);
        }


        // Returns Activities that have the same source as the given subRoot.
        // This will return other instantiation of the same custom activity.
        // Needed to avoid re-instrumentation of the same file.
        public List<Activity> GetSameSourceSubRoots(Activity subRoot)
        {
            string sourcePath;
            List<Activity> sameSourceSubRoots = new List<Activity>();
            if (this.UninstrumentedSubRoots.TryGetValue(subRoot, out sourcePath))
            {
                foreach (KeyValuePair<Activity, string> entry in this.UninstrumentedSubRoots)
                {
                    if (entry.Value == sourcePath && entry.Key != subRoot)
                    {
                        sameSourceSubRoots.Add(entry.Key);
                    }
                }
            }
            return sameSourceSubRoots;
        }

        // Mark this sub root as instrumented.
        public void MarkInstrumented(Activity subRoot)
        {
            this.UninstrumentedSubRoots.Remove(subRoot);
        }
    }
}
