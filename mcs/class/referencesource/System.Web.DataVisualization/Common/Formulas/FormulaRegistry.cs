//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		FormulaRegistry.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	FormulaRegistry
//
//  Purpose:	Keep track of all registered formula module types.
//
//	Reviewed:	GS - August 6, 2002
//				AG - August 7, 2002
//
//===================================================================


#region Used namespace

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

#endregion



#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Formulas
#else
	namespace System.Web.UI.DataVisualization.Charting.Formulas
#endif
{
	/// <summary>
	/// Keep track of all registered formula modules types.
	/// </summary>
	internal class FormulaRegistry : IServiceProvider
	{
		#region Fields

		// Storage for all registered formula modules
		internal	Hashtable	registeredModules = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private Hashtable _createdModules = new Hashtable(StringComparer.OrdinalIgnoreCase);
		private		ArrayList	_modulesNames = new ArrayList();

		#endregion

		#region Methods

		/// <summary>
		/// Formula Registry public constructor
		/// </summary>
		public FormulaRegistry()
		{
		}

		/// <summary>
		/// Adds modules into the registry.
		/// </summary>
		/// <param name="name">Module name.</param>
		/// <param name="moduleType">Module class type.</param>
		public void Register(string name, Type moduleType)
		{
			// First check if module with specified name already registered
			if(registeredModules.Contains(name))
			{
				// If same type provided - ignore
				if(registeredModules[name].GetType() == moduleType)
				{
					return;
				}

				// Error - throw exception
				throw( new ArgumentException( SR.ExceptionFormulaModuleNameIsNotUnique( name ) ) );
			}

			// Add Module Name
			_modulesNames.Add(name);

			// Make sure that specified class support IFormula interface
			bool	found = false;
			Type[]	interfaces = moduleType.GetInterfaces();
			foreach(Type type in interfaces)
			{   
				if(type == typeof(IFormula))
				{
					found = true;
					break;
				}
			}
			if(!found)
			{
				throw( new ArgumentException( SR.ExceptionFormulaModuleHasNoInterface));
			}

			// Add formula module to the hash table
			registeredModules[name] = moduleType;
		}
		
		/// <summary>
		/// Returns formula module registry service object.
		/// </summary>
		/// <param name="serviceType">Service AxisName.</param>
		/// <returns>Service object.</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(FormulaRegistry))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionFormulaModuleRegistryUnsupportedType( serviceType.ToString())));
		}

		/// <summary>
		/// Returns formula module object by name.
		/// </summary>
		/// <param name="name">Formula Module name.</param>
		/// <returns>Formula module object derived from IFormula.</returns>
		public IFormula GetFormulaModule(string name)
		{
			// First check if formula module with specified name registered
			if(!registeredModules.Contains(name))
			{
				throw( new ArgumentException( SR.ExceptionFormulaModuleNameUnknown( name ) ) );
			}

			// Check if the formula module object is already created
			if(!_createdModules.Contains(name))
			{	
				// Create formula module object
				_createdModules[name] = 
					((Type)registeredModules[name]).Assembly.
					CreateInstance(((Type)registeredModules[name]).ToString());
			}

			return (IFormula)_createdModules[name];
		}

		/// <summary>
		/// Returns the name of the module.
		/// </summary>
		/// <param name="index">Module index.</param>
		/// <returns>Module Name.</returns>
		public string GetModuleName( int index )
		{
			return (string)_modulesNames[index];
		}

		#endregion

		#region Properties

		/// <summary>
		/// Return the number of registered modules.
		/// </summary>
		public int Count
		{
			get
			{
				return _modulesNames.Count;
			}
		}
		
		#endregion
	}

    /// <summary>
    /// Interface which defines the set of standard methods and
    /// properties for each formula module
    /// </summary>
	internal interface IFormula
	{
		#region IFormula Properties and Methods

		/// <summary>
		/// Formula Module name
		/// </summary>
		string Name			{ get; }

        /// <summary>
        /// The first method in the module, which converts a formula 
        /// name to the corresponding private method.
        /// </summary>
        /// <param name="formulaName">String which represent a formula name</param>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Formula parameters</param>
        /// <param name="extraParameterList">Array of strings - Extra Formula parameters from DataManipulator object</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		void Formula(string formulaName, double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList, out string [][] outLabels  );

		#endregion
	}
}

