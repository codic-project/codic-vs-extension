using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace CodicExtension.Settings
{
    [Guid(Guids.OptionsPageGuid)]
    [ComVisible(true)]
    public partial class OptionsPage : DialogPage
    {
        private OptionsPageControl _optionsControl;

        public OptionsPage()
        {

        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string AccessToken { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ProjectId { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                _optionsControl = new OptionsPageControl
                {
                    Location = new Point(0, 0),
                    OptionsPage = this
                };
                return _optionsControl;
            }
        }

        public override void SaveSettingsToStorage()
        {
            if (_optionsControl != null)
            {
                _optionsControl.UpdateOptions();
                base.SaveSettingsToStorage();
            }
        }

    }
}
