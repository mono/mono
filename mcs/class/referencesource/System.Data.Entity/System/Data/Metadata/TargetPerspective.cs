//---------------------------------------------------------------------
// <copyright file="TargetPerspective.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Internal helper class for query
    /// </summary>
    internal class TargetPerspective : Perspective
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of perspective class so that query can work
        /// ignorant of all spaces
        /// </summary>
        /// <param name="metadataWorkspace">runtime metadata container</param>
        internal TargetPerspective(MetadataWorkspace metadataWorkspace)
            : base(metadataWorkspace, TargetPerspective.TargetPerspectiveDataSpace)
        {
            _modelPerspective = new ModelPerspective(metadataWorkspace);
        }
        #endregion

        #region Fields
        internal const DataSpace TargetPerspectiveDataSpace = DataSpace.SSpace;
        // TargetPerspective uses a ModelPerspective for a second lookup in type lookup
        private ModelPerspective _modelPerspective;
        #endregion

        #region Methods 
        /// <summary>
        /// Look up a type in the target data space based upon the fullName
        /// </summary>
        /// <param name="fullName">fullName</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="usage"></param>
        /// <returns>a list of types that have the specified full name but may differ by strong name</returns>
        internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage usage)
        {
            EntityUtil.CheckStringArgument(fullName, "fullName");
            
            EdmType edmType = null;
            if (this.MetadataWorkspace.TryGetItem<EdmType>(fullName, ignoreCase, this.TargetDataspace, out edmType))
            {
                usage = TypeUsage.Create(edmType);
                usage = Helper.GetModelTypeUsage(usage);
                return true;
            }

            return _modelPerspective.TryGetTypeByName(fullName, ignoreCase, out usage);
        }

        /// <summary>
        /// Returns the entity container in CSpace or SSpace
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="entityContainer"></param>
        /// <returns></returns>
        internal override bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
        {
            if (!base.TryGetEntityContainer(name, ignoreCase, out entityContainer))
            {
                return _modelPerspective.TryGetEntityContainer(name, ignoreCase, out entityContainer);
            }

            return true;
        }
        #endregion
    }
}
