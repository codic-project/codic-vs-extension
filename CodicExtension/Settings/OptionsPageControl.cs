using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodicExtension.Model;

namespace CodicExtension.Settings
{
    public partial class OptionsPageControl : UserControl
    {
        private OptionsPage _optionsPage;

        public OptionsPageControl()
        {
            InitializeComponent();
            
            linkLabel1.Text = "アクセストークンは、codicにサインアップ後、\nAPIステータスのページより取得できます";
            linkLabel1.Links.Add(10, 5, "https://codic.jp");
            linkLabel1.LinkClicked += (sender, args) =>
            {
                System.Diagnostics.Process.Start(args.Link.LinkData.ToString());
            };

            projectIdCombo.ValueMember = "Id";
        }

        /// <summary>
		/// Gets or Sets the reference to the underlying OptionsPage object.
		/// </summary>
		public OptionsPage OptionsPage
        {
            get
            {
                return _optionsPage;
            }
            set
            {
                _optionsPage = value;
                InitView(_optionsPage);
            }
        }

        private async void InitView(OptionsPage optionPage)
        {
            accessTokenField.Text = optionPage.AccessToken;

            await FillProjectsAsync(optionPage.AccessToken, optionPage.ProjectId);
        }

        private async Task<Boolean> FillProjectsAsync(String accessToken, String selectedId)
        {
            statusLabel.Text = "";
            FillProjectsList(projectIdCombo, new List<Project>(), selectedId);

            if (accessToken == null || accessToken.Equals(""))
            {
                // TODO : Show thick
                return false;
            }

            try
            {
                var api = new CodicApi();
                List<Project> projects = await api.GetUserProjectsAsync(accessToken);
                FillProjectsList(projectIdCombo, projects, selectedId);
            }
            catch (ApiException e)
            {
                statusLabel.Text = e.Message;
                return false;
            }
            return true;
        }

        private void FillProjectsList(ComboBox cb, List<Project> projects, string selectedCode)
        {
            cb.DataSource = projects; 
            if (projects.Count > 0)
            {
                cb.SelectedItem = projects.Find(l => l.Id == selectedCode) ?? projects[0];
            }
        }

        /// <summary>
        /// Update Option page property.
        /// </summary>
		public void UpdateOptions()
        {
            OptionsPage.AccessToken = accessTokenField.Text;
            OptionsPage.ProjectId = (string)projectIdCombo.SelectedValue;
        }

        private void accessTokenField_TextChanged(object sender, EventArgs e)
        {
            FillProjectsAsync(this.accessTokenField.Text,
                (String)this.projectIdCombo.SelectedValue);
        }
    }
}
