using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync
{
    public class Options
    {
        [Option('s', "save", Required = false, HelpText = "Save changes back to dataverse?")]
        public bool Save { get; set; }

        [Option('u', "url", Required = true, HelpText = "Dataverse URL, e.g. https://crmorg.crm4.dynamics.com/")]
        public string Url { get; set; }

        [Option('e', "email", Required = true, HelpText = "Email, e.g. portaladmin@crmorg.onmicrosoft.com")]
        public string Email { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password")]
        public string Password { get; set; }

        [Option('f', "folder", Required = true, HelpText = "Folder to export and import website structure")]
        public string Folder { get; set; }
    }
}
