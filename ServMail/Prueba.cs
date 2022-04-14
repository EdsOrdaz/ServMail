using System;
using System.DirectoryServices;
using System.IO;
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
                MessageBox.Show("Error al iniciar sesión\n\nMensaje: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public static string path = @"LDAP://10.10.10.30";   //CAMBIAR POR VUESTRO PATH (URL DEL SERVICIO DE DIRECTORIO LDAP)
        public static string dominio = @"unne";       //CAMBIAR POR VUESTRO DOMINIO
        public static string usu="Administrador";                           //USUARIO DEL DOMINIO
        public static string pass="Ticu2013*";                          //PASSWORD DEL USUARIO

        public static Boolean permiso;
        private void Prueba_Load(object sender, EventArgs e)
        {
            permiso = AutenticaUsuario(path, dominio + @"\" + usu, pass);
            if (permiso)
            {
                Console.WriteLine("SI ENTRO");
            }
        }
    }
}
