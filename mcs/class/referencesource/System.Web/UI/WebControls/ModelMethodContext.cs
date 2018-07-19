namespace System.Web.UI.WebControls {
    using System;
    using System.Web.ModelBinding;

    /// <summary>
    /// Used to invoke <see cref='System.Web.UI.Page.UpdateModel' />/<see cref='System.Web.UI.Page.TryUpdateModel' /> 
    /// methods where <see cref='System.Web.UI.Page' /> is directly not accessible.
    /// An example is a custom class defining the Select/Update/Delete/InsertMethod properties for 
    /// data binding can have a parameter of type <see cref='System.Web.UI.WebControls.ModelMethodContext' />
    /// in the above methods and use it to invoke the above methods. Alternately instead of a method parameter,
    /// <see cref='System.Web.UI.WebControls.ModelMethodContext.Current' /> can also be used within the 
    /// Select/Update/Delete/InsertMethod code to invoke TryUpdateModel/UpdateModel methods.
    /// </summary>
    public class ModelMethodContext {
        private Page _page;

        public ModelMethodContext(Page page) {
            if (page == null) {
                throw new ArgumentNullException("page");
            }

            _page = page;
        }

        public ModelStateDictionary ModelState {
            get {
                return _page.ModelState;
            }
        }

        /// <summary>
        /// Gets the ModelMethodContext corresponding to the <see cref='System.Web.UI.Page' /> from <see cref='System.Web.HttpContext.Current' />.
        /// Can be null when the current request is not for a <see cref='System.Web.UI.Page' />.
        /// </summary>
        public static ModelMethodContext Current {
            get {
                Page page = HttpContext.Current.Handler as Page;
                if (page != null) {
                    return new ModelMethodContext(page);
                }

                return null;
            }
        }

        /// <summary>
        /// Updates the model object from the values within a databound control. This must be invoked 
        /// within the Select/Update/Delete/InsertMethods used for data binding.
        /// Throws an exception if the update fails.
        /// </summary>
        public virtual void UpdateModel<TModel>(TModel model) where TModel : class {
            _page.UpdateModel<TModel>(model);
        }

        /// <summary>
        /// Updates the model object from the values provided by given valueProvider.
        /// Throws an exception if the update fails.
        /// </summary>
        public virtual void UpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {
            _page.UpdateModel<TModel>(model, valueProvider);
        }

        /// <summary>
        /// Attempts to update the model object from the values provided by given valueProvider.
        /// </summary>
        /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
        public virtual bool TryUpdateModel<TModel>(TModel model) where TModel : class {
            return _page.TryUpdateModel<TModel>(model);
        }

        /// <summary>
        /// Attempts to update the model object from the values within a databound control. This
        /// must be invoked within the Select/Update/Delete/InsertMethods used for data binding. 
        /// </summary>
        /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
        public virtual bool TryUpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {
            return _page.TryUpdateModel<TModel>(model, valueProvider);
        }
    }
}
