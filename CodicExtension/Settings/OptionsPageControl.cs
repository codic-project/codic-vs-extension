using System;
using System.Collections.Generic;
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

        private void InitView(OptionsPage optionPage)
        {
            accessTokenField.Text = optionPage.AccessToken;

            FillProjectsAync(optionPage.AccessToken, optionPage.ProjectId);
        }

        private async void FillProjectsAync(String accessToken, String selectedId)
        {
            statusLabel.Text = "";
            FillProjectsList(projectIdCombo, new List<Project>(), selectedId);

            if (accessToken == null || accessToken.Equals(""))
            {
                // TODO : Show thick
                return;
            }

            try
            {
                var api = new CodicApi();
                List<Project> projects = await api.GetUserProjectsAync(accessToken);
                FillProjectsList(projectIdCombo, projects, selectedId);
            }
            catch (ApiException e)
            {
                statusLabel.Text = e.Message;
            }
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
            FillProjectsAync(this.accessTokenField.Text,
                (String)this.projectIdCombo.SelectedValue);
        }
    }
}
