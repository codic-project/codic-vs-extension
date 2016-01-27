using System.Collections.Generic;
using CodicExtension.Model;

namespace CodicExtension.Settings
{
    internal class Options
    {
        public string ProjectId { get; set; }
        public string AccessToken { get; set; }

        private Options()
        {
        }

        public static Options Get()
        {
            var customOptions = Global.DTE.Properties["CodicExtension", "Options"];

            var res = new Options
            {
                AccessToken = (string)customOptions.Item("AccessToken").Value,
                ProjectId = (string)customOptions.Item("ProjectId").Value,
                //DialogWidth = (int)customOptions.Item("DialogWidth").Value,
                //DialogHeight = (int)customOptions.Item("DialogHeight").Value,
            };

            return res;
        }
    }
}