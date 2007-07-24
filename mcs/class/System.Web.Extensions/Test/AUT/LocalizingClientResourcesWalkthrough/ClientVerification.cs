using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Resources;


namespace LocalizingScriptResources
{
    public class ClientVerification : Control
    {
        private Button _button;
        private Label _firstLabel;
        private Label _secondLabel;
        private TextBox _answer;
        private int _firstInt;
        private int _secondInt;

        protected override void CreateChildControls()
        {
            Random random = new Random();
            _firstInt = random.Next(0, 20);
            _secondInt = random.Next(0, 20);

            ResourceManager rm = new ResourceManager("SystemWebExtensionsAUT.LocalizingClientResourcesWalkthrough.VerificationResources", this.GetType().Assembly);
            Controls.Clear();

            _firstLabel = new Label();
            _firstLabel.ID = "firstNumber";
            _firstLabel.Text = _firstInt.ToString();

            _secondLabel = new Label();
            _secondLabel.ID = "secondNumber";
            _secondLabel.Text = _secondInt.ToString();

            _answer = new TextBox();
            _answer.ID = "userAnswer";

            _button = new Button();
            _button.ID = "Button";
            _button.Text = rm.GetString("Verify");
            _button.OnClientClick = "return CheckAnswer();";

            Controls.Add(_firstLabel);
            Controls.Add(new LiteralControl(" + "));
            Controls.Add(_secondLabel);
            Controls.Add(new LiteralControl(" = "));
            Controls.Add(_answer);
            Controls.Add(_button);
        }
    }
}
