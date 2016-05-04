// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Collections.Generic;

    class ViewStateIdManager
    {
        readonly char separatorChar = '_';
        Dictionary<string, int> prefixToIntMap = new Dictionary<string, int>();

        public void UpdateMap(string id)
        {
            int separatorLocation = id.LastIndexOf('_');

            // If the separator is not found or if the separator is the first or last character
            // in the id then use id value itself as the prefix.
            if (separatorLocation == -1 || separatorLocation == 0 || separatorLocation == id.Length - 1)
            {
                this.prefixToIntMap[id] = 0;
            }
            else
            {
                string[] idParts = new string[2];
                idParts[0] = id.Substring(0, separatorLocation);
                idParts[1] = id.Substring(separatorLocation + 1, id.Length - (separatorLocation + 1));

                int suffix;
                if (int.TryParse(idParts[1], out suffix))
                {
                    int oldValue;
                    if (this.prefixToIntMap.TryGetValue(idParts[0], out oldValue))
                    {
                        if (suffix > oldValue)
                        {
                            this.prefixToIntMap[idParts[0]] = suffix;
                        }
                    }
                    else
                    {
                        this.prefixToIntMap[idParts[0]] = suffix;
                    }
                }
                else
                {
                    this.prefixToIntMap[id] = 0;
                }
            }
        }

        public string GetNewId(string prefix)
        {
            int suffix = 0;
            this.prefixToIntMap.TryGetValue(prefix, out suffix);
            
            while (suffix == int.MaxValue)
            {
                prefix = prefix + this.separatorChar + suffix;
                this.prefixToIntMap.TryGetValue(prefix, out suffix);
            }

            this.prefixToIntMap[prefix] = ++suffix;
            return prefix + this.separatorChar + suffix;
        }
    };
}
