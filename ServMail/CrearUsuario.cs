using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class CrearUsuario : Form
    {
        private String conexionsqllast = "server=148.223.153.37,5314; database=InfEq;User ID=eordazs;Password=Corpame*2013; integrated security = false ; MultipleActiveResultSets=True";

        public CrearUsuario()
        {
            InitializeComponent();
        }

        private void CrearUsuario_Load(object sender, EventArgs e)
        {
            carpeta.Text = SeleccionarCarpeta.foldermostrar;
            pass.Text = NewPass.generarPass();
            name.Text = ListaNOM.name1;
            apellido.Text = ListaNOM.ap1 + " " + ListaNOM.am1;
            displayname.Text = ListaNOM.name2;

            String[] nombrecorreo = name.Text.Split(' ');
            String[] apellidopaterno = apellido.Text.Split(' ');

            mail.Text = nombrecorreo[0].ToLower()+"."+ apellidopaterno[0].ToLower()+"@unne.com.mx";

            carpeta.Text = SeleccionarCarpeta.foldermostrar;
            usuario.Text = ListaNOM.mailnickname;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pass.Text = NewPass.generarPass();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DirectoryEntry objADAM;       // Binding object.
            DirectoryEntry objUser;       // User object.
            // Cargar conexion con AD
            try
            {
                objADAM = new DirectoryEntry(SeleccionarCarpeta.folderusuario, Form1.dominio + @"\" + Form1.usu, Form1.pass, AuthenticationTypes.Secure);
                objADAM.RefreshCache();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el Active Directory\n\nMensaje: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Especificar usuario
            String strUser = "CN="+displayname.Text;
            Console.WriteLine("Create:  {0}", strUser);

            // Crear Usuario
            try
            {
                objUser = objADAM.Children.Add(strUser, "user");

                objUser.Properties["sn"].Add(apellido.Text);
                objUser.Properties["givenname"].Add(name.Text);
                objUser.Properties["samaccountname"].Add(usuario.Text);
                objUser.Properties["displayName"].Add(displayname.Text);
                objUser.Properties["userPrincipalName"].Add(usuario.Text+"@unne.local");
                objUser.CommitChanges();

                //Establecer Contraseña
                objUser.Invoke("SetPassword", new object[] { pass.Text });
                objUser.CommitChanges();

                //Habilitar Cuenta
                int val = (int)objUser.Properties["userAccountControl"].Value;
                objUser.Properties["userAccountControl"].Value = val & ~0x0002;
                objUser.CommitChanges();
                
                int NON_EXPIRE_FLAG = 0x10000;
                val = (int)objUser.Properties["userAccountControl"].Value;
                objUser.Properties["userAccountControl"].Value = val | NON_EXPIRE_FLAG;
                objUser.CommitChanges();


                MessageBox.Show("Usuario creado correctamente", "Nuevo Correo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                guardardatos();

                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el usuario.\n\nMensaje: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Hide();
                return;
            }
        }

        private void guardardatos()
        {
            try
            {
                using (SqlConnection conexion2 = new SqlConnection(conexionsqllast))
                {
                    String nombre = displayname.Text;
                    String correo = mail.Text;
                    String user = usuario.Text;
                    Console.WriteLine(correo);

                    String insert = "INSERT INTO GetName VALUES ('" + nombre + "','" + user + "','" + correo + "', '" + pass.Text + "', '" + DateTime.Now + "')";
                    Console.WriteLine(insert);
                    conexion2.Open();
                    SqlCommand comm2 = new SqlCommand(insert, conexion2);
                    comm2.ExecuteReader();
                    MessageBox.Show("Registro Guardado.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Hide();

                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error al guardar datos.\n\nMensaje: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
