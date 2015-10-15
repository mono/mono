//---------------------------------------------------------------------
// <copyright file="UniqueIdentifierService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
namespace System.Data.Entity.Design.Common
{
    /// <summary>
    /// Service making names within a scope unique. Initialize a new instance
    /// for every scope.
    /// 
    /// 


    internal sealed class UniqueIdentifierService
    {
        internal UniqueIdentifierService(bool caseSensitive)
        {
            _knownIdentifiers = new Dictionary<string, bool>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _identifierToAdjustedIdentifier = new Dictionary<object, string>();
            _transform = s => s;
        }

        internal UniqueIdentifierService(bool caseSensitive, Func<string, string> transform)
        {
            Debug.Assert(transform != null, "use the other constructor if you don't want any transform");
            _knownIdentifiers = new Dictionary<string, bool>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _identifierToAdjustedIdentifier = new Dictionary<object, string>();
            _transform = transform;
        }

        private readonly Dictionary<string, bool> _knownIdentifiers;
        private readonly Dictionary<object, string> _identifierToAdjustedIdentifier;
        private readonly Func<string, string> _transform;

        /// <summary>
        /// This method can be used in when you have an
        /// identifier that you know can't be used, but you don't want
        /// an adjusted version of it
        /// </summary>
        /// <param name="identifier"></param>
        internal void RegisterUsedIdentifier(string identifier)
        {
            Debug.Assert(!_knownIdentifiers.ContainsKey(identifier), "don't register identifiers that already exist");
            _knownIdentifiers.Add(identifier, true);
        }


        /// <summary>
        /// Given an identifier, makes it unique within the scope by adding
        /// a suffix (1, 2, 3, ...), and returns the adjusted identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Must not be null or empty.</param>
        /// <param name="value">Object associated with this identifier in case it is required to
        /// retrieve the adjusted identifier. If not null, must not exist in the current scope already.</param>
        /// <returns>Identifier adjusted to be unique within the scope.</returns>
        internal string AdjustIdentifier(string identifier, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(identifier), "identifier is null or empty");

            // find a unique name by adding suffix as necessary
            int numberOfConflicts = 0;
            string adjustedIdentifier = _transform(identifier);
            while (_knownIdentifiers.ContainsKey(adjustedIdentifier))
            {
                ++numberOfConflicts;
                adjustedIdentifier = _transform(identifier) + numberOfConflicts.ToString(CultureInfo.InvariantCulture);
            }

            // remember the identifier in this scope
            Debug.Assert(!_knownIdentifiers.ContainsKey(adjustedIdentifier), "we just made it unique");
            _knownIdentifiers.Add(adjustedIdentifier, true);

            if (null != value)
            {
                Debug.Assert(!_identifierToAdjustedIdentifier.ContainsKey(value), "should never register one value twice");
                _identifierToAdjustedIdentifier.Add(value, adjustedIdentifier);
            }

            return adjustedIdentifier;
        }

        
        /// <summary>
        /// Simple overload when you don't need to track back to an object
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal string AdjustIdentifier(string identifier)
        {
            return AdjustIdentifier(identifier, null);
        }

        /// <summary>
        /// Determines the adjusted name for an identifier if it has been registered in this scope.
        /// </summary>
        internal bool TryGetAdjustedName(object value, out string adjustedIdentifier)
        {
            return _identifierToAdjustedIdentifier.TryGetValue(value, out adjustedIdentifier);
        }
    }
}
