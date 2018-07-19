//---------------------------------------------------------------------
// <copyright file="MetadataItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Globalization;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the base item class for all the metadata
    /// </summary>
    public abstract partial class MetadataItem
    {
        #region Constructors
        /// <summary>
        /// Implementing this internal constructor so that this class can't be derived
        /// outside this assembly
        /// </summary>
        internal MetadataItem()
        {
        }
        internal MetadataItem(MetadataFlags flags)
        {
            _flags = flags;
        }
        #endregion

        #region Fields
        [Flags]
        internal enum MetadataFlags {
            // GlobalItem
            None = 0,  // DataSpace flags are off by one so that zero can be the uninitialized state
            CSpace = 1,   // (1 << 0)
            OSpace = 2,   // (1 << 1)
            OCSpace = 3,  // CSpace | OSpace
            SSpace = 4,   // (1 << 2)
            CSSpace = 5,  // CSpace | SSpace

            DataSpace = OSpace | CSpace | SSpace | OCSpace | CSSpace,

            // MetadataItem
            Readonly = (1 << 3),

            // EdmType
            IsAbstract = (1 << 4),

            // FunctionParameter
            In = (1 << 9),
            Out = (1 << 10),
            InOut = In | Out,
            ReturnValue = (1 << 11),

            ParameterMode = (In | Out | InOut | ReturnValue),
        }
        private MetadataFlags _flags;
        private object _flagsLock = new object();
        private MetadataCollection<MetadataProperty> _itemAttributes;
        private Documentation _documentation;
        #endregion

        #region Properties

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public abstract BuiltInTypeKind BuiltInTypeKind
        {
            get;
        }

        /// <summary>
        /// List of item attributes on this type
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.MetadataProperty, true)]
        public ReadOnlyMetadataCollection<MetadataProperty> MetadataProperties
        {
            get
            {
                if (null == _itemAttributes)
                {
                    MetadataPropertyCollection itemAttributes = new MetadataPropertyCollection(this);
                    if (IsReadOnly)
                    {
                        itemAttributes.SetReadOnly();
                    }
                    System.Threading.Interlocked.CompareExchange<MetadataCollection<MetadataProperty>>(
                        ref _itemAttributes, itemAttributes, null);
                }
                return _itemAttributes.AsReadOnlyMetadataCollection();
            }
        }

        /// <summary>
        /// List of item attributes on this type
        /// </summary>
        internal MetadataCollection<MetadataProperty> RawMetadataProperties
        {
            get
            {
                return _itemAttributes;
            }
        }

        /// <summary>
        /// List of item attributes on this type
        /// </summary>
        public Documentation Documentation
        {
            get
            {
                return _documentation; 
            }
            set
            {
                _documentation = value;
            }
        }

        /// <summary>
        /// Identity of the item
        /// </summary>
        internal abstract String Identity { get; }

        /// <summary>
        /// Just checks for identities to be equal
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal virtual bool EdmEquals(MetadataItem item)
        {
            return ((null != item) &&
                    ((this == item) || // same reference
                     (this.BuiltInTypeKind == item.BuiltInTypeKind &&
                      this.Identity == item.Identity)));
        }

        /// <summary>
        /// Returns true if this item is not-changeable. Otherwise returns false.
        /// </summary>
        internal bool IsReadOnly
        {
            get
            {
                return GetFlag(MetadataFlags.Readonly);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Validates the types and sets the readOnly property to true. Once the type is set to readOnly,
        /// it can never be changed. 
        /// </summary>
        internal virtual void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                if (null != _itemAttributes)
                {
                    _itemAttributes.SetReadOnly();
                }
                SetFlag(MetadataFlags.Readonly, true);
            }
        }

        /// <summary>
        /// Builds identity string for this item. By default, the method calls the identity property.
        /// </summary>
        /// <param name="builder"></param>
        internal virtual void BuildIdentity(StringBuilder builder)
        {
            builder.Append(this.Identity);
        }

        /// <summary>
        /// Adds the given metadata property to the metadata property collection
        /// </summary>
        /// <param name="metadataProperty"></param>
        internal void AddMetadataProperties(List<MetadataProperty> metadataProperties)
        {
            this.MetadataProperties.Source.AtomicAddRange(metadataProperties);
        }
        #endregion

        #region MetadataFlags
        internal DataSpace GetDataSpace()
        {
            switch (_flags & MetadataFlags.DataSpace)
            {
                default: return (DataSpace)(-1);
                case MetadataFlags.CSpace: return DataSpace.CSpace;
                case MetadataFlags.OSpace: return DataSpace.OSpace;
                case MetadataFlags.SSpace: return DataSpace.SSpace;
                case MetadataFlags.OCSpace: return DataSpace.OCSpace;
                case MetadataFlags.CSSpace: return DataSpace.CSSpace;
            }
        }
        internal void SetDataSpace(DataSpace space)
        {
            _flags = (_flags & ~MetadataFlags.DataSpace) | (MetadataFlags.DataSpace & Convert(space));
        }
        private static MetadataFlags Convert(DataSpace space) {
            switch (space)
            {
                default: return MetadataFlags.None; // invalid
                case DataSpace.CSpace: return MetadataFlags.CSpace;
                case DataSpace.OSpace: return MetadataFlags.OSpace;
                case DataSpace.SSpace: return MetadataFlags.SSpace;
                case DataSpace.OCSpace: return MetadataFlags.OCSpace;
                case DataSpace.CSSpace: return MetadataFlags.CSSpace;
            }
        }

        internal ParameterMode GetParameterMode()
        {
            switch (_flags & MetadataFlags.ParameterMode)
            {
                default: return (ParameterMode)(-1); // invalid
                case MetadataFlags.In: return ParameterMode.In;
                case MetadataFlags.Out: return ParameterMode.Out;
                case MetadataFlags.InOut: return ParameterMode.InOut;
                case MetadataFlags.ReturnValue: return ParameterMode.ReturnValue;
            }
        }
        internal void SetParameterMode(ParameterMode mode)
        {
            _flags = (_flags & ~MetadataFlags.ParameterMode) | (MetadataFlags.ParameterMode & Convert(mode));
        }
        private static MetadataFlags Convert(ParameterMode mode)
        {
            switch (mode)
            {
                default: return MetadataFlags.ParameterMode; // invalid
                case ParameterMode.In: return MetadataFlags.In;
                case ParameterMode.Out: return MetadataFlags.Out;
                case ParameterMode.InOut: return MetadataFlags.InOut;
                case ParameterMode.ReturnValue: return MetadataFlags.ReturnValue;
            }
        }

        internal bool GetFlag(MetadataFlags flag)
        {
            return (flag == (_flags & flag)); 
        }
        internal void SetFlag(MetadataFlags flag, bool value)
        {
            if ((flag & MetadataFlags.Readonly) == MetadataFlags.Readonly)
            {
                Debug.Assert(System.Convert.ToInt32(flag & ~MetadataFlags.Readonly,CultureInfo.InvariantCulture) == 0,
                            "SetFlag() invoked with Readonly and additional flags.");
            }

            lock (_flagsLock)
            {
                // an attempt to set the ReadOnly flag on a MetadataItem that is already read-only
                // is a no-op
                //
                if (IsReadOnly && ((flag & MetadataFlags.Readonly) == MetadataFlags.Readonly))
                {
                    return;
                }

                Util.ThrowIfReadOnly(this);
                if (value)
                {
                    _flags |= flag;
                }
                else
                {
                    _flags &= ~flag;
                }
            }
        }
        #endregion
    }
}
