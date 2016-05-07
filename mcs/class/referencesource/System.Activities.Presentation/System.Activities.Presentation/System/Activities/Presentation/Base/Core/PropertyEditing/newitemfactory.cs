namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Windows;
    using System.Reflection;
    using System.IO;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;
    using System.Activities.Presentation.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    /// <summary>
    /// Base class that represents a factory for creating new items for a collection or
    /// for a property value.  3rd party control developers may choose to derive from this class
    /// to override the default behavior of specifying names and images that are used by
    /// collection editor and sub-property editor when creating instances of custom controls.
    /// </summary>
    class NewItemFactory {

        private Type[] NoTypes = new Type[0];

        /// <summary>
        /// Default constructor
        /// </summary>
        public NewItemFactory() { }

        /// <summary>
        /// Returns an object that can be set as the Content of a ContentControl 
        /// and that will be used an icon for the requested type by the property editing host.
        /// The default implementation of this method uses naming convention, searching for
        /// the embedded resources in the same assembly as the control, that are named the 
        /// same as the control (including namespace), followed by ".Icon", followed by
        /// the extension for the file type itself.  Currently, only ".png", ".bmp", ".gif",
        /// ".jpg", and ".jpeg" extensions are recognized.
        /// </summary>
        /// <param name="type">Type of the object to look up</param>
        /// <param name="desiredSize">The desired size of the image to retrieve.  If multiple
        /// images are available this method retrieves the image that most closely
        /// resembles the requested size.  However, it is not guaranteed to return an image
        /// that matches the desired size exactly.</param>
        /// <returns>An image for the specified type.</returns>
        /// <exception cref="ArgumentNullException">If type is null</exception>
        public virtual object GetImage(Type type, Size desiredSize) {
            if (type == null)
                throw FxTrace.Exception.ArgumentNull("type");

            return ManifestImages.GetImage(type, desiredSize);
        }

        /// <summary>
        /// Returns the name for the item this factory adds for the passed in type.  This is 
        /// the name that will be used in the "Add Item" drop down to identify the type being added.
        /// The default implementation returns the short type name.
        /// </summary>
        /// <param name="type">Type to retrieve the display name for.</param>
        /// <returns>The display name for the specified type</returns>
        /// <exception cref="ArgumentNullException">If type is null</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The intended usage in the larger scope of the class is the stronger type")]
        public virtual string GetDisplayName(Type type)
        {
            if (type == null)
                throw FxTrace.Exception.ArgumentNull("type");

            return type.Name;
        }

        /// <summary>
        /// Returns an instance of an item that is added to the collection for the passed in Type.
        /// The default implementation looks for public constructors that take no arguments.
        /// If no such constructors are found, null is returned.
        /// </summary>
        /// <param name="type">Type of the object to create</param>
        /// <returns>Instance of the specified type, or null if no appropriate constructor was found
        /// </returns>
        /// <exception cref="ArgumentNullException">If type is null</exception>
        public virtual object CreateInstance(Type type) {
            if (type == null)
                throw FxTrace.Exception.ArgumentNull("type");

            ConstructorInfo ctor = type.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                NoTypes,
                null);

            return ctor == null ? null : ctor.Invoke(null);
        }
    }
}
