namespace System.Web.Routing {
    using System.Collections.Generic;
    using System.Linq;

    // Represents a segment of a URL that is not a separator. It contains subsegments such as literals and parameters.
    internal sealed class ContentPathSegment : PathSegment {
        public ContentPathSegment(IList<PathSubsegment> subsegments) {
            Subsegments = subsegments;
        }

        public bool IsCatchAll {
            get {
                // 
                return Subsegments.Any<PathSubsegment>(seg => (seg is ParameterSubsegment) && (((ParameterSubsegment)seg).IsCatchAll));
            }
        }

        public IList<PathSubsegment> Subsegments {
            get;
            private set;
        }

#if ROUTE_DEBUGGING
        public override string LiteralText {
            get {
                List<string> s = new List<string>();
                foreach (PathSubsegment subsegment in Subsegments) {
                    s.Add(subsegment.LiteralText);
                }
                return String.Join(String.Empty, s.ToArray());
            }
        }

        public override string ToString() {
            List<string> s = new List<string>();
            foreach (PathSubsegment subsegment in Subsegments) {
                s.Add(subsegment.ToString());
            }
            return "[ " + String.Join(", ", s.ToArray()) + " ]";
        }
#endif
    }
}
