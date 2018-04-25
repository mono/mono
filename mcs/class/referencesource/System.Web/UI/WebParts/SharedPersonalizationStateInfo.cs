//------------------------------------------------------------------------------
// <copyright file="SharedPersonalizationStateInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    [Serializable]
    public sealed class SharedPersonalizationStateInfo : PersonalizationStateInfo {

        private int _sizeOfPersonalizations;
        private int _countOfPersonalizations;

        public SharedPersonalizationStateInfo(string path,
                                              DateTime lastUpdatedDate,
                                              int size,
                                              int sizeOfPersonalizations,
                                              int countOfPersonalizations) :
                                              base(path, lastUpdatedDate, size) {
            PersonalizationProviderHelper.CheckNegativeInteger(sizeOfPersonalizations, "sizeOfPersonalizations");
            PersonalizationProviderHelper.CheckNegativeInteger(countOfPersonalizations, "countOfPersonalizations");
            _sizeOfPersonalizations = sizeOfPersonalizations;
            _countOfPersonalizations = countOfPersonalizations;
        }

        public int SizeOfPersonalizations {
            get {
                return _sizeOfPersonalizations;
            }
        }

        public int CountOfPersonalizations {
            get {
                return _countOfPersonalizations;
            }
        }
    }
}
