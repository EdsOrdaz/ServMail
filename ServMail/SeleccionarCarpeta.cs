using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServMail
{
    public partial class SeleccionarCarpeta : Form
    {
        public SeleccionarCarpeta()
        {
            InitializeComponent();
        }

        public static List<String[]> lista = new List<String[]>();

        private void SeleccionarCarpeta_Load(object sender, EventArgs e)
        {
            lista.Clear();
            dataGridView1.Rows.Clear();
            List<string> orgUnits = new List<string>();

            DirectoryEntry startingPoint = new DirectoryEntry(Form1.path, Form1.dominio + @"\" + Form1.usu, Form1.pass, AuthenticationTypes.Secure);

            DirectorySearcher searcher = new DirectorySearcher(startingPoint);
            searcher.Filter = "(objectCategory=organizationalUnit)";

            int t = 0;
            foreach (SearchResult res in searcher.FindAll())
            {
                String[] urlcompleto = res.Path.Split('/');
                String[] textoconcomas = urlcompleto[3].Split(',');

                if (t > 2)
                {
                    String folder1 = "";
                    String folder2 = "";
                    String[] findfolder = Array.FindAll(textoconcomas, element => element.StartsWith("OU", StringComparison.Ordinal));
                    foreach (String aa in findfolder)
                    {
                        String[] ii = aa.Split('=');
                        if (String.IsNullOrEmpty(folder1))
                        {
                            folder1 = ii[1];
                        }
                        else
                        {
                            folder2 = ii[1];
                        }
                    }
                    if (!String.IsNullOrEmpty(folder2))
                    {
                        String[] n = new String[3];
                        n[0] = folder2;
                        n[1] = folder1;
                        n[2] = res.Path;
                        lista.Add(n);
                        dataGridView1.Rows.Add(folder2, folder1, res.Path);
                    }
                }
                orgUnits.Add(res.Path);
                t++;
            }
            dataGridView1.Sort(dataGridView1.Columns["folder"], ListSortDirection.Ascending);
        }

        public static String folderusuario;
        public static String foldermostrar;
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            folderusuario = dataGridView1.Rows[e.RowIndex].Cells["foldercompleto"].Value.ToString();
            foldermostrar = dataGridView1.Rows[e.RowIndex].Cells["folder2"].Value.ToString();
            CrearUsuario crear = new CrearUsuario();
            crear.ShowDialog();
            this.Hide();
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            dataGridView1.Rows.Clear();
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                foreach (String[] folders in lista)
                {
                    String nmayus = folders[1].ToString().ToUpper();
                    if (nmayus.Contains(textBox1.Text.ToUpper()))
                    {
                        dataGridView1.Rows.Add(folders[0], folders[1], folders[2]);
                        dataGridView1.Sort(dataGridView1.Columns["folder"], ListSortDirection.Ascending);
                    }
                }
            }
            else
            {
                foreach (String[] folders in lista)
                {
                    dataGridView1.Rows.Add(folders[0], folders[1], folders[2]);
                    dataGridView1.Sort(dataGridView1.Columns["folder"], ListSortDirection.Ascending);
                }
            }
        }
    }
}
