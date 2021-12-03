using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class ListaUsuarios : Form
    {
        private String conexionsqllast = "server=148.223.153.37,5314; database=InfEq;User ID=eordazs;Password=Corpame*2013; integrated security = false ; MultipleActiveResultSets=True";

        public ListaUsuarios()
        {
            InitializeComponent();
        }

        private void ListaUsuarios_Load(object sender, EventArgs e)
        {
            foreach(Form1.userLADP value in Form1.d)
            {
                dataGridView1.Rows.Add(value.name, value.samaccountname, value.mail);
            }
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            dataGridView1.Rows.Clear();
            if(!String.IsNullOrEmpty(textBox1.Text))
            {
                foreach (Form1.userLADP value in Form1.d)
                {
                    if (value.name.ToUpper().Contains(textBox1.Text.ToUpper()))
                    {
                        dataGridView1.Rows.Add(value.name, value.samaccountname, value.mail);
                    }
                }
            }
            else
            {
                foreach (Form1.userLADP value in Form1.d)
                {
                    dataGridView1.Rows.Add(value.name, value.samaccountname, value.mail);
                }
            }
        }

        public static string mailnickname;
        public static string mail2;
        public static string name2;
        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            mailnickname = dataGridView1.Rows[e.RowIndex].Cells["user"].Value.ToString();
            mail2 = dataGridView1.Rows[e.RowIndex].Cells["mail"].Value.ToString();
            name2 = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();

            try
            {
                using (SqlConnection conexion = new SqlConnection(conexionsqllast))
                {
                    NewPass NewPass = new NewPass();
                    conexion.Open();
                    SqlCommand comm = new SqlCommand("SELECT * FROM [InfEq].[dbo].[GetName] WHERE nombre='"+ name2 + "' AND correo='"+ mail2 + "' ORDER BY id DESC", conexion);
                    SqlDataReader nwReader = comm.ExecuteReader();
                    while (nwReader.Read())
                    {
                        if(!String.IsNullOrEmpty(nwReader["password"].ToString()))
                        {
                            DialogResult preguntar = MessageBox.Show("Ya existen los datos de\n" + nwReader["nombre"].ToString() + ".\nQuieres resetear la contraseña?", "Resetear Contraseña", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if(preguntar == DialogResult.Yes)
                            {
                                NewPass.ShowDialog();
                            }
                        }
                        return;
                    }
                    NewPass.ShowDialog();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error en la busqueda\n\nMensaje: " + ex.Message, "Información del Equipo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ListaUsuarios_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void crearUsuarioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListaNOM listanom = new ListaNOM();
            listanom.ShowDialog();
        }
    }
}
