//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceWizardForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//
// Containing form for the wizard panels
//------------------------------------------------------------------------------

using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.UI.Design.WebControls.Util;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceWizardForm : WizardForm
    {
        private EntityDataSourceState _entityDataSourceState;

        private EntityDataSourceConfigureObjectContext _configureContext;
        private EntityDataSourceDataSelection _configureDataSelection;
        private readonly EntityDataSourceDesignerHelper _helper;

        public EntityDataSourceWizardForm(IServiceProvider serviceProvider, EntityDataSourceState entityDataSourceState, EntityDataSourceDesigner entityDataSourceDesigner)
            : base(serviceProvider)
        {
            _entityDataSourceState = entityDataSourceState;
            this.SetGlyph(new Bitmap(BitmapSelector.GetResourceStream(typeof(EntityDataSourceWizardForm), "EntityDataSourceWizard.bmp")));
            this.Text = String.Format(CultureInfo.InvariantCulture, Strings.Wizard_Caption(((EntityDataSource)entityDataSourceDesigner.Component).ID));

            _helper = entityDataSourceDesigner.Helper;

            EntityDataSourceConfigureObjectContextPanel contextPanel = new EntityDataSourceConfigureObjectContextPanel();
            _configureContext = new EntityDataSourceConfigureObjectContext(contextPanel, this, entityDataSourceDesigner.Helper, _entityDataSourceState);

            EntityDataSourceDataSelectionPanel dataPanel = new EntityDataSourceDataSelectionPanel();
            _configureDataSelection = new EntityDataSourceDataSelection(dataPanel, this, entityDataSourceDesigner.Helper, _entityDataSourceState);

            _configureContext.ContainerNameChanged += _configureDataSelection.ContainerNameChangedHandler;

            _configureContext.LoadState();
            _configureDataSelection.LoadState();

            // Adds the panels to the wizard
            SetPanels(new WizardPanel[] {
                contextPanel,
                dataPanel});
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.EntityDataSource.ConfigureDataSource";
            }
        }

        public EntityDataSourceState EntityDataSourceState
        {
            get
            {
                return _entityDataSourceState;
            }
        }

        protected override void OnFinishButtonClick(object sender, EventArgs e)
        {
            _configureContext.SaveState();
            _configureDataSelection.SaveState();
            base.OnFinishButtonClick(sender, e);
        }

        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e)
        {
            // Reset the helper so it knows to try to load the web.config file again on future executions
            _helper.CanLoadWebConfig = true; 
            base.OnFormClosed(e);
        }

        public void SetCanFinish(bool enabled)
        {
            FinishButton.Enabled = enabled;
            if (enabled)
            {
                this.AcceptButton = FinishButton;
            }
        }

        public void SetCanNext(bool enabled)
        {
            NextButton.Enabled = enabled;
            if (enabled)
            {
                this.AcceptButton = NextButton;
            }
        }
    }
}
