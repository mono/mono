//---------------------------------------------------------------------
// <copyright file="EntityViewGenerationAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
namespace System.Data.Mapping
{
    /// <summary>
    /// Attribute to mark the assemblies that contain the generated views type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EntityViewGenerationAttribute : System.Attribute
    {
        #region Constructors
        /// <summary>
        /// Constructor for EntityViewGenerationAttribute
        /// </summary>
        public EntityViewGenerationAttribute(Type viewGenerationType)
        {
            EntityUtil.CheckArgumentNull<Type>(viewGenerationType, "viewGenType");
            m_viewGenType = viewGenerationType;
        }
        #endregion

        #region Fields
        private Type m_viewGenType;
        #endregion

        #region Properties
        public Type ViewGenerationType
        {
            get { return m_viewGenType; }
        }
        #endregion
    }
}

