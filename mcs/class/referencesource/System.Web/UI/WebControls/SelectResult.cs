using System.Collections;

namespace System.Web.UI.WebControls {
    public sealed class SelectResult {
        public SelectResult(int totalRowCount, IEnumerable results) {
            if (totalRowCount < 0) {
                throw new ArgumentOutOfRangeException("totalRowCount");
            }
            
            TotalRowCount = totalRowCount;
            Results = results;
        }

        public int TotalRowCount { get; private set; }
        public IEnumerable Results { get; private set; }
    }
}
