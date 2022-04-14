using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ServMail
{
    public partial class NewPass : Form
    {
        #region SQLIE
        public static String conexionsqllast = "server=148.223.153.37,5314; database=InfEq;User ID=eordazs;Password=Corpame*2013; integrated security = false ; MultipleActiveResultSets=True";
        #endregion

        public NewPass()
        {
            InitializeComponent();
        }

        private void NewPass_Load(object sender, EventArgs e)
        {
            name.Text = ListaUsuarios.name2;
            user.Text = ListaUsuarios.mailnickname;
            mail.Text = ListaUsuarios.mail2;

            pass.Text = generarPass();
        }

        public static String generarPass()
        {
            string letrasmin = "abcdefghijklmnopqrstuvwxyz";
            string letrasmay = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numeros = "1234567890";
            string caracteres = "#*";

            string password = "";

            int cc = 0, cn = 0, ce = 0, cm = 0;

            Random r = new Random();

            while (password.Length < 10)
            {
                switch (r.Next(0, 4))
                {
                    case 0:
                        if (cc < 3)
                        {
                            char c = letrasmin[r.Next(letrasmin.Length)];
                            cc++;
                            password += c;
                        }
                        break;
                    case 1:
                        if (cm < 3)
                        {
                            char mm = letrasmay[r.Next(letrasmay.Length)];
                            cm++;
                            password += mm;
                        }

                        break;
                    case 2:
                        if (cn < 3)
                        {
                            char n = numeros[r.Next(numeros.Length)];
                            cn++;
                            password += n;
                        }

                        break;
                    case 3:
                        if (ce < 1)
                        {
                            char eee = caracteres[r.Next(caracteres.Length)];
                            ce++;
                            password += eee;
                        }
                        break;
                }
            }
            return password;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pass.Text = generarPass();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DirectoryEntry de = new DirectoryEntry(Form1.path,Form1.dominio + @"\" + Form1.usu, Form1.pass, AuthenticationTypes.Secure);
            DirectorySearcher ds = new DirectorySearcher(de);
            try
            {
                string strFilter = "(&(objectClass=user)(|(sAMAccountName=" + ListaUsuarios.mailnickname + ")))";
                ds.Filter = strFilter;
                ds.PropertiesToLoad.Add("displayName");
                ds.PropertiesToLoad.Add("sAMAccountName");
                ds.PropertiesToLoad.Add("DistinguishedName");
                ds.PropertiesToLoad.Add("CN");
                SearchResult result = ds.FindOne();
                string dn = result.Properties["DistinguishedName"][0].ToString();

                DirectoryEntry user = result.GetDirectoryEntry();
                //user.Properties["sAMAccountName"].Value = "eordazs";
                //user.CommitChanges();
                user.AuthenticationType = AuthenticationTypes.Secure;
                user.Invoke("SetPassword", new object[] { pass.Text });
                user.CommitChanges();


                try
                {
                    String correo = mail.Text;
                    String usuario = ListaUsuarios.mailnickname;
                    String nombre = name.Text;
                    
                    using (SqlConnection conexion_delete = new SqlConnection(conexionsqllast))
                    {
                        String delete = "delete from GetName WHERE usuario='"+ usuario + "' AND correo='"+ correo + "'";
                        Console.WriteLine(delete);
                        conexion_delete.Open();
                        SqlCommand comm2_delete = new SqlCommand(delete, conexion_delete);
                        comm2_delete.ExecuteReader();
                    }
                    
                    using (SqlConnection conexion2 = new SqlConnection(conexionsqllast))
                    {
                        String new_pass = Encriptar(pass.Text);
                        String insert = "INSERT INTO GetName VALUES ('" + nombre + "','" + usuario + "','" + correo + "', '" + new_pass + "', '" + DateTime.Now + "')";
                        Console.WriteLine(insert);
                        conexion2.Open();
                        SqlCommand comm2 = new SqlCommand(insert, conexion2);
                        comm2.ExecuteReader();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error en base de datos.\n\nMensaje: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                DialogResult msj = MessageBox.Show("Contraseña Reseteada.\n\n¿Quieres enviar correo con la información?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (msj == DialogResult.Yes)
                {
                    String correo = mail.Text;
                    String usuario = "unne\\"+ListaUsuarios.mailnickname;

                    List<string> lstAllRecipients = new List<string>();
                    //Below is hardcoded - can be replaced with db data
                    lstAllRecipients.Add(correo);

                    Outlook.Application outlookApp = new Outlook.Application();
                    Outlook._MailItem oMailItem = (Outlook._MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);
                    Outlook.Inspector oInspector = oMailItem.GetInspector;

                    // Recipient
                    Outlook.Recipients oRecips = (Outlook.Recipients)oMailItem.Recipients;
                    foreach (String recipient in lstAllRecipients)
                    {
                        Outlook.Recipient oRecip = (Outlook.Recipient)oRecips.Add(recipient);
                        oRecip.Resolve();
                    }

                    //Outlook.Recipient oCCRecip = oRecips.Add("edson.ordaz@unne.com.mx");
                    Outlook.Recipient oCCRecip = oRecips.Add("daniel.gonzalez@unne.com.mx");

                    oCCRecip.Type = (int)Outlook.OlMailRecipientType.olBCC;
                    oCCRecip.Resolve();

                    oMailItem.Subject = "Reseteo contraseña correo";

                    String FirmaBody = oMailItem.HTMLBody;

                    //Body para Edson
                    oMailItem.HTMLBody = "<html xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" xmlns:m=\"http://schemas.microsoft.com/office/2004/12/omml\" xmlns=\"http://www.w3.org/TR/REC-html40\"><head><meta http-equiv=Content-Type content=\"text/html; charset=iso-8859-1\"><meta name=Generator content=\"Microsoft Word 15 (filtered medium)\"><style><!--/* Font Definitions */@font-face	{font-family:\"Cambria Math\";	panose-1:2 4 5 3 5 4 6 3 2 4;}@font-face	{font-family:Calibri;	panose-1:2 15 5 2 2 2 4 3 2 4;}/* Style Definitions */p.MsoNormal, li.MsoNormal, div.MsoNormal	{margin:0cm;	margin-bottom:.0001pt;	font-size:11.0pt;	font-family:\"Calibri\",sans-serif;	mso-fareast-language:EN-US;}a:link, span.MsoHyperlink	{mso-style-priority:99;	color:#0563C1;	text-decoration:underline;}a:visited, span.MsoHyperlinkFollowed	{mso-style-priority:99;	color:#954F72;	text-decoration:underline;}span.EstiloCorreo17	{mso-style-type:personal-compose;	font-family:\"Calibri\",sans-serif;	color:#2F5496;}.MsoChpDefault	{mso-style-type:export-only;	font-family:\"Calibri\",sans-serif;	mso-fareast-language:EN-US;}@page WordSection1	{size:612.0pt 792.0pt;	margin:70.85pt 3.0cm 70.85pt 3.0cm;}div.WordSection1	{page:WordSection1;}--></style><!--[if gte mso 9]><xml><o:shapedefaults v:ext=\"edit\" spidmax=\"1026\" /></xml><![endif]--><!--[if gte mso 9]><xml><o:shapelayout v:ext=\"edit\"><o:idmap v:ext=\"edit\" data=\"1\" /></o:shapelayout></xml><![endif]--></head><body lang=ES-MX link=\"#0563C1\" vlink=\"#954F72\"><div class=WordSection1><p class=MsoNormal><span style='color:#2F5496'>Buen día.<o:p></o:p></span></p><p class=MsoNormal><span style='color:#2F5496'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal><span style='color:#2F5496'>Por medio de la presente te hacemos llegar tu usuario y contraseña para el uso del correo electrónico corporativo. Es muy importante que utilices correctamente esta información considerando las siguientes políticas:<o:p></o:p></span></p><p class=MsoNormal><span style='color:#2F5496'>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; El uso de internet es una herramienta de trabajo para el uso de actividades de la empresa.<o:p></o:p></span></p><p class=MsoNormal><span style='color:#2F5496'>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Está estrictamente prohibido escuchar música&nbsp; y ver videos en internet, así como cualquier otra actividad que consuma recursos de internet.<o:p></o:p></span></p><p class=MsoNormal><span style='color:#2F5496'>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Se prohíbe la instalación de programas no autorizados por la empresa y en caso de requerir la instalación de un programa, favor de apoyarse con el área de sistemas con la finalidad de no instalar software basura que viene con los programas gratuitos.<o:p></o:p></span></p><p class=MsoNormal><span style='color:#2F5496'>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; La utilización del usuario y contraseña son de uso PERSONAL, es decir, que tú eres el responsable de cualquier solicitud que se haga en el sistema con ese usuario, por lo tanto no debes prestarlo a nadie.<o:p></o:p></span></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><table class=MsoNormalTable border=0 cellspacing=0 cellpadding=0 style='border-collapse:collapse'><tr><td width=198 valign=top style='width:148.45pt;border:solid windowtext 1.0pt;background:#A8D08D;padding:0cm 0cm 0cm 0cm'><p class=MsoNormal align=center style='text-align:center'><b>CORREO ELECTRÓNICO<o:p></o:p></b></p></td><td width=106 style='width:79.65pt;border:solid windowtext 1.0pt;border-left:none;background:#A8D08D;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'><b>USUARIO<o:p></o:p></b></p></td><td width=151 style='width:4.0cm;border:solid windowtext 1.0pt;border-left:none;background:#A8D08D;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'><b>CONTRASEÑA<o:p></o:p></b></p></td></tr><tr><td width=198 valign=top style='width:148.45pt;border:solid windowtext 1.0pt;border-top:none;padding:0cm 0cm 0cm 0cm'><p class=MsoNormal align=center style='text-align:center'><a href=\"mailto:" + correo + "\">" + correo + "</a><o:p></o:p></p></td><td width=106 style='width:79.65pt;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'>" + usuario + "<o:p></o:p></p></td><td width=151 style='width:4.0cm;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'>" + pass.Text + "<o:p></o:p></p></td></tr></table>";

                    //Body para Daniel
                    //oMailItem.HTMLBody = "<html xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" xmlns:m=\"http://schemas.microsoft.com/office/2004/12/omml\" xmlns=\"http://www.w3.org/TR/REC-html40\"><head><meta http-equiv=Content-Type content=\"text/html; charset=iso-8859-1\"><meta name=Generator content=\"Microsoft Word 15 (filtered medium)\"><style><!--/* Font Definitions */@font-face	{font-family:\"Cambria Math\";	panose-1:2 4 5 3 5 4 6 3 2 4;}@font-face	{font-family:Calibri;	panose-1:2 15 5 2 2 2 4 3 2 4;}/* Style Definitions */p.MsoNormal, li.MsoNormal, div.MsoNormal	{margin:0cm;	margin-bottom:.0001pt;	font-size:11.0pt;	font-family:\"Calibri\",sans-serif;	mso-fareast-language:EN-US;}a:link, span.MsoHyperlink	{mso-style-priority:99;	color:#0563C1;	text-decoration:underline;}a:visited, span.MsoHyperlinkFollowed	{mso-style-priority:99;	color:#954F72;	text-decoration:underline;}span.EstiloCorreo17	{mso-style-type:personal-compose;	font-family:\"Calibri\",sans-serif;	color:#2F5496;}.MsoChpDefault	{mso-style-type:export-only;	font-size:10.0pt;	font-family:\"Calibri\",sans-serif;	mso-fareast-language:EN-US;}@page WordSection1	{size:612.0pt 792.0pt;	margin:70.85pt 3.0cm 70.85pt 3.0cm;}div.WordSection1	{page:WordSection1;}--></style><!--[if gte mso 9]><xml><o:shapedefaults v:ext=\"edit\" spidmax=\"1026\" /></xml><![endif]--><!--[if gte mso 9]><xml><o:shapelayout v:ext=\"edit\"><o:idmap v:ext=\"edit\" data=\"1\" /></o:shapelayout></xml><![endif]--></head><body lang=ES-MX link=\"#0563C1\" vlink=\"#954F72\"><div class=WordSection1><p class=MsoNormal>Buen día.<o:p></o:p></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal>Por medio de la presente te hacemos llegar tu usuario y contraseña para el uso del correo electrónico corporativo. Es muy importante que utilices correctamente esta información considerando las siguientes políticas:<o:p></o:p></p><p class=MsoNormal>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; El uso de internet es una herramienta de trabajo para el uso de actividades de la empresa.<o:p></o:p></p><p class=MsoNormal>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Está estrictamente prohibido escuchar música&nbsp; y ver videos en internet, así como cualquier otra actividad que consuma recursos de internet.<o:p></o:p></p><p class=MsoNormal>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Se prohíbe la instalación de programas no autorizados por la empresa y en caso de requerir la instalación de un programa, favor de apoyarse con el área de sistemas con la finalidad de no instalar software basura que viene con los programas gratuitos.<o:p></o:p></p><p class=MsoNormal>&#8226;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; La utilización del usuario y contraseña son de uso PERSONAL, es decir, que tú eres el responsable de cualquier solicitud que se haga en el sistema con ese usuario, por lo tanto no debes prestarlo a nadie.<o:p></o:p></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><table class=MsoNormalTable border=0 cellspacing=0 cellpadding=0 style='border-collapse:collapse'><tr><td width=198 valign=top style='width:148.45pt;border:solid windowtext 1.0pt;background:#A8D08D;padding:0cm 0cm 0cm 0cm'><p class=MsoNormal align=center style='text-align:center'><b>CORREO ELECTRÓNICO<o:p></o:p></b></p></td><td width=106 style='width:79.65pt;border:solid windowtext 1.0pt;border-left:none;background:#A8D08D;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'><b>USUARIO<o:p></o:p></b></p></td><td width=151 style='width:4.0cm;border:solid windowtext 1.0pt;border-left:none;background:#A8D08D;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'><b>CONTRASEÑA<o:p></o:p></b></p></td></tr><tr><td width=198 valign=top style='width:148.45pt;border:solid windowtext 1.0pt;border-top:none;padding:0cm 0cm 0cm 0cm'><p class=MsoNormal align=center style='text-align:center'><a href=\"mailto:" + correo + "\">" + correo + "</a><o:p></o:p></p></td><td width=106 style='width:79.65pt;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'>" + usuario + "<o:p></o:p></p></td><td width=151 style='width:4.0cm;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0cm 5.4pt 0cm 5.4pt'><p class=MsoNormal align=center style='text-align:center'>" + pass.Text + "<o:p></o:p></p></td></tr></table><p class=MsoNormal><o:p>&nbsp;</o:p></p></div></body></html>";

                    oMailItem.HTMLBody += FirmaBody;
                    oMailItem.Display(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al resetear contraseña.\n\nError: " + ex.Message, "Error");
            }
            this.Close();
        }

        public static string Encriptar(String textToEncrypt)
        {
            try
            {
                //string textToEncrypt = "EdsonOrdaz";
                string ToReturn = "";
                string publickey = "12345678";
                string secretkey = "87654321";
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

    }
}
