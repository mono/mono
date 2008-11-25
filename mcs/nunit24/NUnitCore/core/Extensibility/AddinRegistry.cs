using System;
using System.Collections;
using System.Reflection;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// Summary description for AddinRegistry.
	/// </summary>
	public class AddinRegistry : MarshalByRefObject, IAddinRegistry, IService
    {
        #region Instance Fields
        private ArrayList addins = new ArrayList();
		#endregion

		#region IAddinRegistry Members

        //public void Register(Type type)
        //{
        //    addins.Add( new Addin( type ) );
        //}

		public void Register(Addin addin)
		{
			addins.Add( addin );
		}

        public void Register(Assembly assembly)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.GetCustomAttributes(typeof(NUnitAddinAttribute), false).Length == 1)
                {
                    Addin addin = new Addin(type);
                    Register(addin);
                }
            }
        }

		public  IList Addins
		{
			get
			{
				return addins;
			}
		}

		public void SetStatus( string name, AddinStatus status )
		{
			foreach( Addin addin in addins )
				if ( addin.Name == name )
					addin.Status = status;
		}
		#endregion

		#region IService Members
		public void InitializeService()
		{
		}

		public void UnloadService()
		{
		}
		#endregion
	}
}
