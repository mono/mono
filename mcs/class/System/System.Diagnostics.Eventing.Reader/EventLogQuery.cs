namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;

    public class EventLogQuery
    {
        private string path;
        private PathType pathType;
        private string query;
        private bool reverseDirection;
        private EventLogSession session;
        private bool tolerateErrors;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogQuery(string path, PathType pathType) : this(path, pathType, null)
        {
        }

        public EventLogQuery(string path, PathType pathType, string query)
        {
            this.session = EventLogSession.GlobalSession;
            this.path = path;
            this.pathType = pathType;
            if (query == null)
            {
                if (path == null)
                {
                    throw new ArgumentNullException("path");
                }
            }
            else
            {
                this.query = query;
            }
        }

        internal string Path
        {
            get
            {
                return this.path;
            }
        }

        internal string Query
        {
            get
            {
                return this.query;
            }
        }

        public bool ReverseDirection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reverseDirection;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.reverseDirection = value;
            }
        }

        public EventLogSession Session
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.session;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.session = value;
            }
        }

        internal PathType ThePathType
        {
            get
            {
                return this.pathType;
            }
        }

        public bool TolerateQueryErrors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tolerateErrors;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.tolerateErrors = value;
            }
        }
    }
}

