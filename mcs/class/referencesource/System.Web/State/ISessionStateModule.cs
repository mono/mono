//------------------------------------------------------------------------------
// <copyright file="ISessionStateModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

 /*
 * ISessionStateModule
 *
 */
namespace System.Web.SessionState {
    using Threading.Tasks;

     /// <summary>
    /// Defines the contract to implement a custom session-state module.
    /// </summary>
    public interface ISessionStateModule : IHttpModule {

         /// <summary>
        /// Synchronously release the session state acquired by the module in the current context.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        void ReleaseSessionState(HttpContext context);

         /// <summary>
        /// Asynchronously release the session state acquired by the module in the current context.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        Task ReleaseSessionStateAsync(HttpContext context);
    }
}