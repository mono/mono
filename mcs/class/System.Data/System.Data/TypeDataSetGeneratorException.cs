//
// System.Data.TypedDataSetGeneratorException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class TypedDataSetGeneratorException : DataException
	{

		ArrayList errorList;

		#region Constructors
		public TypedDataSetGeneratorException ()
			: base (Locale.GetText ("There is a name conflict"))
		{
		}

		public TypedDataSetGeneratorException (ArrayList list)
			: base (Locale.GetText ("There is a name conflict"))
		{
			errorList = list;
		}

		protected TypedDataSetGeneratorException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion //Constructors	

		public ArrayList ErrorList
                {
                        get 
                        {
                                return errorList;
                        }
                                           
		}

		#region Methods
                                                                                                    
                public override void GetObjectData (SerializationInfo si, StreamingContext context)
                {
                        if (si == null)
                                throw new ArgumentNullException ("si");
                                                                                                    
                        si.AddValue ("ErrorList", errorList);
                        base.GetObjectData (si, context);
                }
                                                                                                    
                #endregion // Methods

	}
}
