using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class Prueba : Form
    {
        public Prueba()
        {
            InitializeComponent();
        }

        private void Prueba_Load(object sender, EventArgs e)
        {
            string loginName = "Administrador";
            string loginPassword = "Ticu2013*";
            SecureString ssLoginPassword = new SecureString();
            foreach (char x in loginPassword)
                ssLoginPassword.AppendChar(x);


            PSCredential remoteMachineCredentials = new PSCredential(loginName, ssLoginPassword);

            // Set the connection Info
            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri("http://servmail.unne.local/PowerShell/"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", remoteMachineCredentials);

            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Negotiate;
            //connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            Runspace runspace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace(connectionInfo);

            PowerShell powershell = PowerShell.Create();
            PSCommand command = new PSCommand();
            command.AddCommand("date");
            //command.AddCommand("Get-Mailbox");
            //command.AddParameter("Identity", "danielr");

            powershell.Commands = command;
            try
            {
                // open the remote runspace
                runspace.Open();
                // associate the runspace with powershell
                powershell.Runspace = runspace;
                // invoke the powershell to obtain the results
                powershell.Invoke();
                var results = powershell.Invoke();
                runspace.Close();
                foreach (PSObject obj in results)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    //stringBuilder.AppendLine(obj.ToString());
                    MessageBox.Show(obj.ToString());
                }

            }
            finally
            {
                // dispose the runspace and enable garbage collection
                runspace.Dispose();
                runspace = null;
                // Finally dispose the powershell and set all variables to null to free
                // up any resources.
                powershell.Dispose();
                powershell = null;
            }
        }
    }
}
