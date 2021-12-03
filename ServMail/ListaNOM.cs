using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class ListaNOM : Form
    {
        public ListaNOM()
        {
            InitializeComponent();
        }

        private void ResetearContraseñaToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ListaNOM_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void ListaNOM_Load(object sender, EventArgs e)
        {

        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            dataGridView1.Rows.Clear();
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                foreach (String[] empleado in Form1.lista)
                {
                    String nmayus = empleado[5].ToString().ToUpper();
                    if (nmayus.Contains(textBox1.Text.ToUpper()))
                    {
                        dataGridView1.Rows.Add(empleado[3], empleado[4], empleado[6],empleado[0],empleado[1],empleado[2]);
                    }
                }
            }
            else
            {
                foreach (String[] empleado in Form1.lista)
                {
                    dataGridView1.Rows.Add(empleado[3], empleado[4], empleado[6], empleado[0], empleado[1], empleado[2]);
                }
            }
        }

        public static String mailnickname;
        public static String mail2;
        public static String name2;
        public static String name1;
        public static String ap1;
        public static String am1;
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            mailnickname = dataGridView1.Rows[e.RowIndex].Cells["user"].Value.ToString();
            mail2 = dataGridView1.Rows[e.RowIndex].Cells["mail"].Value.ToString();
            name2 = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();
            name1 = dataGridView1.Rows[e.RowIndex].Cells["n1"].Value.ToString();
            ap1 = dataGridView1.Rows[e.RowIndex].Cells["ap"].Value.ToString();
            am1 = dataGridView1.Rows[e.RowIndex].Cells["am"].Value.ToString();
            SeleccionarCarpeta SeleccionarCarpeta = new SeleccionarCarpeta();
            SeleccionarCarpeta.ShowDialog();
            this.Hide();
        }
    }
}
