
namespace System.Activities.Presentation.Model {

    using System.Activities.Presentation.Services;
    using System.Activities.Presentation;

    using System;

    /// <summary>
    /// The ModelFactory class should be used to create instances 
    /// of items in the designer. ModelFactory is designed to be 
    /// a static API for convenience.  The underlying implementation 
    /// of this API simply calls through to the ModelService’s 
    /// CreateItem method.
    /// </summary>
    public static class ModelFactory {

        /// <summary>
        /// Creates a new item for the given item type.
        /// </summary>
        /// <param name="context">
        /// The designer's editing context.
        /// </param>
        /// <param name="itemType">
        /// The type of item to create.
        /// </param>
        /// <param name="arguments">
        /// An optional array of arguments that should be passed to the constructor of the item.
        /// </param>
        /// <returns>
        /// The newly created item type.
        /// </returns>
        /// <exception cref="ArgumentNullException">if itemType or context is null.</exception>
        /// <exception cref="InvalidOperationException">if there is no editing model in the context that can create new items.</exception>
        public static ModelItem CreateItem(EditingContext context, Type itemType, params object[] arguments) {
            return CreateItem(context, itemType, CreateOptions.None, arguments);
        }

        /// <summary>
        /// Creates a new item for the given item type.
        /// </summary>
        /// <param name="context">
        /// The designer's editing context.
        /// </param>
        /// <param name="itemType">
        /// The type of item to create.
        /// </param>
        /// <param name="options">
        /// A set of create options to use when creating the item.  The default value is CreateOptions.None.
        /// </param>
        /// <param name="arguments">
        /// An optional array of arguments that should be passed to the constructor of the item.
        /// </param>
        /// <returns>
        /// The newly created item type.
        /// </returns>
        /// <exception cref="ArgumentNullException">if itemType or context is null.</exception>
        /// <exception cref="InvalidOperationException">if there is no editing model in the context that can create new items.</exception>
        public static ModelItem CreateItem(EditingContext context, Type itemType, CreateOptions options, params object[] arguments) {
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (itemType == null) throw FxTrace.Exception.ArgumentNull("itemType");
            if (!EnumValidator.IsValid(options)) throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("options"));

            ModelService ms = context.Services.GetRequiredService<ModelService>();
            return ms.InvokeCreateItem(itemType, options, arguments);
        }

        /// <summary>
        /// Creates a new model item by creating a deep copy of the isntance provided.
        /// </summary>
        /// <param name="context">
        /// The designer's editing context.
        /// </param>
        /// <param name="item">
        /// The item to clone.
        /// </param>
        /// <returns>
        /// The newly created item.
        /// </returns>
        public static ModelItem CreateItem(EditingContext context, object item) {
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (item == null) throw FxTrace.Exception.ArgumentNull("item");

            ModelService ms = context.Services.GetRequiredService<ModelService>();
            return ms.InvokeCreateItem(item);
        }

        /// <summary>
        /// Create a new model item that represents a the value of a static member of a the given class.
        /// For example, to add a reference to Brushes.Red to the model call this methods with 
        /// typeof(Brushes) and the string "Red". This will be serialized into XAML as 
        /// {x:Static Brushes.Red}.
        /// </summary>
        /// <param name="context">
        /// The designer's editing context.
        /// </param>
        /// <param name="type">
        /// The type that contains the static member being referenced.
        /// </param>
        /// <param name="memberName">
        /// The name of the static member being referenced.
        /// </param>
        /// <returns></returns>
        public static ModelItem CreateStaticMemberItem(EditingContext context, Type type, string memberName) {
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (type == null) throw FxTrace.Exception.ArgumentNull("type");
            if (memberName == null) throw FxTrace.Exception.ArgumentNull("memberName");

            ModelService ms = context.Services.GetRequiredService<ModelService>();
            return ms.InvokeCreateStaticMemberItem(type, memberName);
        }
    }
}
