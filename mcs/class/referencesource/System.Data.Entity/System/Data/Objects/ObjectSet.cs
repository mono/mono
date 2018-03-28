//---------------------------------------------------------------------
// <copyright file="ObjectSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

// ObjectSet<TEntity> is a wrapper around ObjectQuery<TEntity> and CUD
// methods on the ObjectContext. It provides a single source that can
// be used to build queries over an EntitySet as well as perform CUD operations.
//
// devnote: This methods on this class are expected to behave exactly
//          like the ObjectContext methods to which they delegate. Therefore,
//          they should do very little, if any, extra work aside from a 
//          single call to delegate to the appropriate ObjectContext method.
//          The only exceptions to this are cases where we need to do 
//          additional validation that is specific to ObjectSet, such as
//          verifying that an object is in the appropriate EntitySet during
//          DeleteObject and Detach operations. We also should not override
//          or otherwise change ObjectQuery<T> behavior. Users should be able to
//          use an ObjectSet<TEntity> in exactly the same ways they can use an
//          ObjectQuery<T>, except for the fact that the CommandText is
//          determined by the EntitySet name, and cannot be controlled by the user.
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Diagnostics;

namespace System.Data.Objects
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectSet<TEntity> : ObjectQuery<TEntity>, IObjectSet<TEntity>
        where TEntity : class
    {
        private readonly EntitySet _entitySet;

        #region Internal Constructors
        /// <summary>
        /// Creates a new ObjectSet that has a base ObjectQuery with the CommandText that represents
        /// all of the entities in the specified EntitySet. 
        /// Sets the query's command text to the fully-qualified, quoted, EntitySet name, i.e. [EntityContainerName].[EntitySetName]
        /// Explicitly set MergeOption to AppendOnly in order to mirror CreateQuery behavior
        /// </summary>
        /// <param name="entitySet">Metadata EntitySet on which to base the ObjectSet.</param>
        /// <param name="context">ObjectContext to be used for the query and data modification operations.</param>
        internal ObjectSet(EntitySet entitySet, ObjectContext context)
            : base(entitySet, context, MergeOption.AppendOnly)
        {
            Debug.Assert(entitySet != null, "ObjectSet constructor requires a non-null EntitySet");
            Debug.Assert(context != null, "ObjectSet constructor requires a non-null ObjectContext");
            _entitySet = entitySet;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Provides metadata for the EntitySet that is represented by the ObjectSet
        /// </summary>
        public EntitySet EntitySet
        {
            get
            {
                return _entitySet;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds an object to the ObjectContext using the EntitySet referenced by this ObjectSet.
        /// See ObjectContext.AddObject for more details.
        /// </summary>
        /// <param name="entity">Entity to be added</param>
        public void AddObject(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.AddObject -- see devnote at the top of this class
            this.Context.AddObject(FullyQualifiedEntitySetName, entity);
        }

        /// <summary>
        /// Attaches an object to the ObjectContext using the EntitySet referenced by this ObjectSet.
        /// See ObjectContext.AttachTo for more details.
        /// </summary>
        /// <param name="entity">Entity to be attached</param>       
        public void Attach(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.AttachTo -- see devnote at the top of this class
            this.Context.AttachTo(FullyQualifiedEntitySetName, entity);
        }

        /// <summary>
        /// Deletes an object from the ObjectContext. Validates that the object is in the referenced EntitySet in the context.
        /// See ObjectContext.DeleteObject for more details.
        /// </summary>
        /// <param name="entity">Entity to be deleted.</param>
        /// <exception cref="InvalidOperationException">Throws if the specified object is not in the EntitySet.</exception>
        public void DeleteObject(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.DeleteObject -- see devnote at the top of this class
            // Note that in this case we use an internal DeleteObject overload so we can have the context validate
            // the EntitySet after it verifies that the specified object is in the context at all.
            this.Context.DeleteObject(entity, EntitySet);
        }

        /// <summary>
        /// Detaches an object from the ObjectContext. Validates that the object is in the referenced EntitySet in the context.
        /// See ObjectContext.Detach for more details.
        /// </summary>
        /// <param name="entity">Entity to be detached.</param>        
        /// <exception cref="InvalidOperationException">Throws if the specified object is not in the EntitySet.</exception>
        public void Detach(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.Detach -- see devnote at the top of this class
            // Note that in this case we use an internal Detach overload so we can have the context validate
            // the EntitySet after it verifies that the specified object is in the context at all.
            this.Context.Detach(entity, EntitySet);
        }

        /// <summary>
        /// Applies changes from one object to another with the same key in the ObjectContext.
        /// See ObjectContext.ApplyCurrentValues for more details.
        /// </summary>
        /// <param name="TEntity">Entity that contains changes to be applied.</param>
        public TEntity ApplyCurrentValues(TEntity currentEntity)
        {
            // this method is expected to behave exactly like ObjectContext.ApplyCurrentValues -- see devnote at the top of this class
            return this.Context.ApplyCurrentValues<TEntity>(FullyQualifiedEntitySetName, currentEntity);
        }

        /// <summary>
        /// Apply modified properties to the original object with the same key in the ObjectContext.
        /// See ObjectContext.ApplyOriginalValues for more details.
        /// </summary>
        /// <param name="TEntity">Entity that contains values to be applied.</param>
        public TEntity ApplyOriginalValues(TEntity originalEntity)
        {
            // this method is expected to behave exactly like ObjectContext.ApplyOriginalValues -- see devnote at the top of this class
            return this.Context.ApplyOriginalValues<TEntity>(FullyQualifiedEntitySetName, originalEntity);
        }

        /// <summary>
        /// Create an instance of the type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>
        /// An instance of an object of type <typeparamref name="TEntity"/>.
        /// The object will either be an instance of the exact type <typeparamref name="TEntity"/>,
        /// or possibly an instance of the proxy type that corresponds to <typeparamref name="TEntity"/>.
        /// </returns>
        public TEntity CreateObject()
        {
            return this.Context.CreateObject<TEntity>();
        }

        /// <summary>
        /// Create an instance of the type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>
        /// An instance of an object of type <typeparamref name="TEntity"/>.
        /// The object will either be an instance of the exact type <typeparamref name="TEntity"/>,
        /// or possibly an instance of the proxy type that corresponds to <typeparamref name="TEntity"/>.
        /// </returns>
        public T CreateObject<T>() where T : class, TEntity
        {
            return this.Context.CreateObject<T>();
        }
        
        #endregion

        #region Private Properties
        // Used
        private string FullyQualifiedEntitySetName
        {
            get
            {
                // Fully-qualified name is used to ensure the ObjectContext can always resolve the EntitySet name
                // The identifiers used here should not be escaped with brackets ("[]") because the ObjectContext does not allow escaping for the EntitySet name
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", _entitySet.EntityContainer.Name, _entitySet.Name);
            }
        }
        #endregion
    }
}
