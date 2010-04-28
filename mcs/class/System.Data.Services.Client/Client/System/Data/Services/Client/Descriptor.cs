//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    public abstract class Descriptor
    {
        #region Fields

        private uint changeOrder = UInt32.MaxValue;

        private bool saveContentGenerated;

        private EntityStates saveResultProcessed;

        private Exception saveError;

        private EntityStates state;

        #endregion

        internal Descriptor(EntityStates state)
        {
            this.state = state;
        }

        #region Public Properties

        public EntityStates State
        {
            get { return this.state; }
            internal set { this.state = value; }
        }

        #endregion

        #region Internal Properties
        
        internal abstract bool IsResource
        {
            get;
        }

        internal uint ChangeOrder
        {
            get { return this.changeOrder; }
            set { this.changeOrder = value; }
        }

        internal bool ContentGeneratedForSave
        {
            get { return this.saveContentGenerated; }
            set { this.saveContentGenerated = value; }
        }

        internal EntityStates SaveResultWasProcessed
        {
            get { return this.saveResultProcessed; }
            set { this.saveResultProcessed = value; }
        }

        internal Exception SaveError
        {
            get { return this.saveError; }
            set { this.saveError = value; }
        }
        
        internal virtual bool IsModified
        {
            get
            {
                System.Diagnostics.Debug.Assert(
                    (EntityStates.Added == this.state) ||
                    (EntityStates.Modified == this.state) ||
                    (EntityStates.Unchanged == this.state) ||
                    (EntityStates.Deleted == this.state),
                    "entity state is not valid");

                return (EntityStates.Unchanged != this.state);
            }
        }

        #endregion
    }
}
