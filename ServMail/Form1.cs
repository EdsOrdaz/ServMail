using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class Form1 : Form
    {
        /*
         V1.44
        - Se agrega encriptacion al guardar la contraseña para GetName

        */
        private String version = "1.44";
        private String version_hash = "bb0cb2c25a057125c6c35b19b2289fc9";
        #region SQLIF
        //dat nom
        private String select = "SELECT ltrim(rtrim(nombre)) as nombre, ltrim(rtrim(apepat)) as paterno, ltrim(rtrim(apemat)) as materno,email FROM [Nom2001].[dbo].[nomtrab] where status='A' ORDER BY nombre asc";
        private String conexionsql = "server=40.76.105.1,5055; database=Nom2001;User ID=reportesUNNE;Password=8rt=h!RdP9gVy; integrated security = false ; MultipleActiveResultSets=True";
        //dat InfEq
        public static String servidor = "148.223.153.37,5314";
        public static String basededatos = "InfEq";
        public static String usuariobd = "eordazs";
        public static String passbd = "Corpame*2013";
        public static string nsqlEx = "server=" + servidor + "; database=" + basededatos + " ;User ID=" + usuariobd + ";Password=" + passbd + "; integrated security = false ; MultipleActiveResultSets=True";
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        //Lista de usuario NOM
        public static List<String[]> lista = new List<String[]>();

        public static string path = @"LDAP://10.10.10.25";   //CAMBIAR POR VUESTRO PATH (URL DEL SERVICIO DE DIRECTORIO LDAP)
        public static string dominio = @"unne";       //CAMBIAR POR VUESTRO DOMINIO
        public static string usu;                           //USUARIO DEL DOMINIO
        public static string pass;                          //PASSWORD DEL USUARIO
        //public static string domUsu = dominio + @"\" + usu; //CADENA DE DOMINIO + USUARIO A COMPROBAR
        public static Boolean permiso;
        public static List<userLADP> d = new List<userLADP>();

        public static string PrimeraMayuscula(String value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            usu = user.Text;
            pass = pass2.Text;

            if (cargando.IsBusy != true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.TransparencyKey = Color.Gray;
                this.BackColor = Color.Gray;
                this.Size = new Size(210, 213);
                pictureBox1.BringToFront();
                pictureBox1.Visible = true;
                cargando.RunWorkerAsync();
            }
        }


        private void User_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void Pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private bool AutenticaUsuario(String path, String user, String pass)
        {
            //Los datos que hemos pasado los 'convertimos' en una entrada de Active Directory para hacer la consulta
            DirectoryEntry de = new DirectoryEntry(path, user, pass, AuthenticationTypes.Secure);
            try
            {
                //Inicia el chequeo con las credenciales que le hemos pasado
                //Si devuelve algo significa que ha autenticado las credenciales
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.FindOne();
                return true;
            }
            catch (Exception ex)
            {
                //Si no devuelve nada es que no ha podido autenticar las credenciales
                //ya sea porque no existe el usuario o por que no son correctas
                MessageBox.Show("Error al iniciar sesión\n\nMensaje: "+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Cargando_DoWork(object sender, DoWorkEventArgs e)
        {
            //Revisar version del programa
            try
            {
                using (SqlConnection conexion2 = new SqlConnection(nsqlEx))
                {
                    conexion2.Open();
                    String sql2 = "SELECT (SELECT valor FROM Configuracion WHERE nombre='ServMail_Version') as version,valor FROM Configuracion WHERE nombre='ServMail_hash'";
                    SqlCommand comm2 = new SqlCommand(sql2, conexion2);
                    SqlDataReader nwReader2 = comm2.ExecuteReader();
                    if (nwReader2.Read())
                    {
                        if (nwReader2["valor"].ToString() != version_hash)
                        {
                            MessageBox.Show("No se puede inciar sesion porque hay una nueva version.\n\nNueva version: " + nwReader2["version"] + "\nVersion actual: " + version + "\n\nEl programa se cerrara.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                            e.Cancel = true;
                            return;

                        }
                    }
                    else
                    {
                        MessageBox.Show("No se puedo verificar la version del programa.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en validar la version del programa\n\nMensaje: " + ex.Message, "Información del Equipo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                e.Cancel = true;
                return;
            }

            #region Lista NOM
            //Cargar usuarios de NOM - USAR CUANDO SE PUEDAN CREAR BUZONES
            
            try
            {
                using (SqlConnection conexion = new SqlConnection(conexionsql))
                {
                    conexion.Open();
                    SqlCommand comm = new SqlCommand(select, conexion);
                    SqlDataReader nwReader = comm.ExecuteReader();
                    while (nwReader.Read())
                    {
                        String[] n = new String[7];
                        n[0] = PrimeraMayuscula(nwReader["nombre"].ToString().ToLower());
                        n[1] = PrimeraMayuscula(nwReader["paterno"].ToString().ToLower());
                        n[2] = (string.IsNullOrEmpty(nwReader["materno"].ToString())) ? "" : PrimeraMayuscula(nwReader["materno"].ToString().ToLower());
                        n[3] = n[0] + " " + n[1] + " " + n[2];
                        char nom = n[0][0];
                        String id;
                        if (string.IsNullOrEmpty(nwReader["materno"].ToString()))
                        {
                            id = nom.ToString() + n[1];
                        }
                        else
                        {
                            char a = n[2][0];
                            id = nom.ToString() + n[1] + a.ToString();
                        }
                        n[4] = id.ToLower();
                        n[5] = n[3].ToUpper();
                        n[6] = nwReader["email"].ToString();
                        lista.Add(n);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error en la busqueda de usuario NOM\n\nMensaje: " + ex.Message, "Información del Equipo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }
            
            #endregion

            //Cargar usuarios Active Directory
            permiso = AutenticaUsuario(path, dominio + @"\" + usu, pass);
            if (permiso)
            {
                ConsultarUsuarios();
            }
        }

        private void Cargando_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                pictureBox1.Visible = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.BackColor = Control.DefaultBackColor;
                this.Size = new Size(235, 180);
                return;
            }
            if (permiso)
            {
                /*
                ListaNOM listanom = new ListaNOM();
                Hide();
                listanom.Show();
                */

                ListaUsuarios listausuarios = new ListaUsuarios();
                Hide();
                listausuarios.Show();
            }
            else
            {
                pictureBox1.Visible = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.BackColor = Control.DefaultBackColor;
                this.Size = new Size(235, 180);
            }
        }


        private static DirectoryEntry createDirectoryEntry()
        {
            DirectoryEntry ldapConnection = new DirectoryEntry(path, dominio + @"\" + usu, pass, AuthenticationTypes.Secure);
            return ldapConnection;
        }
        public List<userLADP> ConsultarUsuarios()
        {
            DirectoryEntry myLdapConnection = createDirectoryEntry();
            using (var buscadorDirectorio = new DirectorySearcher(myLdapConnection, "(&(objectClass=user)(objectCategory=person))"))
            {
                try
                {
                    SearchResult result;
                    SearchResultCollection iResult = buscadorDirectorio.FindAll();
                    if (iResult != null)
                    {
                        for (int counter = 0; counter < iResult.Count; counter++)
                        {
                            userLADP user = new userLADP();
                            user.activo = false;
                            result = iResult[counter];
                            user.samaccountname = (result.Properties.Contains("samaccountname")) ? (String)result.Properties["samaccountname"][0] : "";

                            user.givenname = (result.Properties.Contains("givenname")) ? (String)result.Properties["givenname"][0] : "";
                            user.departament = (result.Properties.Contains("departament")) ? (String)result.Properties["departament"][0] : "";
                            user.name = (result.Properties.Contains("name")) ? (String)result.Properties["name"][0] : "";
                            /*
                            user.whencreated = (result.Properties.Contains("whencreated")) ? (String)result.Properties["whencreated"][0] : "";
                            Console.WriteLine("whencreated: " + user.whencreated);
                            */
                            user.mailnickname = (result.Properties.Contains("mailnickname")) ? (String)result.Properties["mailnickname"][0] : "";
                            user.mail = (result.Properties.Contains("mail")) ? (String)result.Properties["mail"][0] : "";
                            user.displayname = (result.Properties.Contains("displayname")) ? (String)result.Properties["displayname"][0] : "";

                            if (result.Properties.Contains("userAccountControl"))
                            {
                                user.state = (int)result.Properties["userAccountControl"][0];
                                user.activo = ValidarEstadoActivoUsuario(user.state);
                            }


                            /*
                            if (user.samaccountname == "eordazs")
                            {
                                Console.WriteLine(result);
                                Console.WriteLine(result);
                            }
                            */
                            d.Add(user);
                        }
                    }

                }
                catch (Exception ex)
                {
                    string mensaje = "Error: ID = " + ex.GetHashCode() + " || Mensaje = " + ex.Message.ToString();
                }
            }
            myLdapConnection.Dispose();

            Console.WriteLine(d);
            return d;
        }

        public bool ValidarEstadoActivoUsuario(int estado)
        {
            bool activo = false;

            if (estado == 512 || estado == 512 || estado == 66048 || estado == 262656 || estado == 8388608)
            {
                activo = true;
            }

            return activo;
        }
        public class userLADP : ListaUsuarios
        {
            //ID Usuario
            public string samaccountname { get; set; }
            //Nombre
            public string givenname { get; set; }
            //Departament
            public string departament { get; set; }
            //MB Cuota
            //public string mdboverquotalimit { get; set; }
            //Nombre Completo
            public string name { get; set; }
            //NFecha de creacion
            public string whencreated { get; set; }
            //IDCorreo
            public string mailnickname { get; set; }
            //Correo
            public string mail { get; set; }
            //Nombre Para Mostrar
            public string displayname { get; set; }
            public int state { get; set; }
            public bool activo { get; set; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            usu = user.Text;
            pass = pass2.Text;

            if (cargando.IsBusy != true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.TransparencyKey = Color.Gray;
                this.BackColor = Color.Gray;
                this.Size = new Size(210, 213);
                pictureBox1.BringToFront();
                pictureBox1.Visible = true;
                cargando.RunWorkerAsync();
            }
        }
    }
}
