// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/***************************************************************************\
*
*
*  Class for Xaml markup extension for Arrays
*
*
\***************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Markup
{
    /// <summary>
    ///  Class for Xaml markup extension for Arrays.
    /// </summary>
    [TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [ContentProperty("Items")]
    [MarkupExtensionReturnType(typeof(Array))]
    public class ArrayExtension : MarkupExtension
    {
        /// <summary>
        ///  Constructor that takes no parameters.  This creates an empty array.
        /// </summary>
        public ArrayExtension()
        {
        }
        
        /// <summary>
        ///  Constructor that takes one parameter.  This initializes the type of the array.
        /// </summary>
        public ArrayExtension(
            Type arrayType)
        {
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            _arrayType = arrayType;
        }

        /// <summary>
        /// Constructor for writing
        /// </summary>
        /// <param name="elements">The array to write</param>
        public ArrayExtension(Array elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            _arrayList.AddRange(elements);
            _arrayType = elements.GetType().GetElementType();
        }

        ///<summary>
        /// Called to Add an object as a new array item.  This will append the object to the end
        /// of the array.
        ///</summary>
        ///<param name="value">
        /// Object to add to the end of the array.
        ///</param>
        public void AddChild(Object value)
        {
            _arrayList.Add(value);
        }

        ///<summary>
        /// Called to Add a text as a new array item.  This will append the object to the end
        /// of the array.
        ///</summary>
        ///<param name="text">
        /// Text to Add to the end of the array
        ///</param> 
        public void AddText(string text)
        {
            AddChild(text);
        }

        ///<summary>
        /// Get and set the type of array to be created when calling ProvideValue
        ///</summary>
        [ConstructorArgument("type")]
        public Type Type
        {
            get { return _arrayType; }
            set { _arrayType = value; }
        }

        /// <summary>
        /// An IList accessor to the contents of the array
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList Items
        {
            get { return _arrayList; }
        }

        /// <summary>
        ///  Return an array that is sized to the number of objects added to the ArrayExtension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        ///  The Array containing all the objects added to this extension.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_arrayType == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.MarkupExtensionArrayType));
            }
            
            object retArray = null;

            try
            {
                retArray = _arrayList.ToArray(_arrayType);
            }
            catch (System.InvalidCastException)
            {
                // If an element was added to the ArrayExtension that does not agree with the
                // ArrayType, then an InvalidCastException will occur.  Generate a more
                // meaningful error for this case.
                throw new InvalidOperationException(SR.Get(SRID.MarkupExtensionArrayBadType, _arrayType.Name));
            }

            return retArray;
        }

        private ArrayList _arrayList = new ArrayList();
        private Type      _arrayType;

    }
}
