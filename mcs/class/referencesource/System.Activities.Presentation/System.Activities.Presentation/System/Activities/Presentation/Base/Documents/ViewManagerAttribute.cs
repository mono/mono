
namespace System.Activities.Presentation.Documents
{

    using System.Activities.Presentation;
    using System.Activities.Presentation.Internal.Properties;
    using System;
    using System.Globalization;

    /// <summary>
    /// This attribute can be placed on the root of a model
    /// object graph to specify what view manager should be
    /// used to present the view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class ViewManagerAttribute : Attribute
    {
        private Type _viewManagerType;


        /// <summary>
        /// An empty ViewManagerAttribute allows you to "unset" the view manager from a base class.
        /// </summary>
        public ViewManagerAttribute()
        {
        }

        /// <summary>
        /// Creates a new ViewManager attribute.
        /// </summary>
        /// <param name="viewManagerType">The type of view manager to use.  The type specified must derive from ViewManager.</param>
        /// <exception cref="ArgumentNullException">If viewManagerType is null.</exception>
        /// <exception cref="ArgumentException">If viewManagerType does not specify a type that derives from ViewManager.</exception>
        public ViewManagerAttribute(Type viewManagerType)
        {
            if (viewManagerType == null) throw FxTrace.Exception.ArgumentNull("viewManagerType");
            if (!typeof(ViewManager).IsAssignableFrom(viewManagerType))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Resources.Error_InvalidArgumentType,
                    "viewManagerType",
                    typeof(ViewManager).FullName)));
            }
            _viewManagerType = viewManagerType;
        }

        /// <summary>
        /// The type of view manager to create for the model.
        /// </summary>
        public Type ViewManagerType
        {
            get { return _viewManagerType; }
        }
    }
}
