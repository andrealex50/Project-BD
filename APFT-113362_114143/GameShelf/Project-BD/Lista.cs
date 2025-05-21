using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class Lista : Form
    {
        private string currentUserId;
        private string listId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public Lista(string userId, string listId)
        {
            InitializeComponent();
            currentUserId = userId;
            this.listId = listId;
            LoadListData();
            LoadListEntries();
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }

        private void LoadListData()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT l.titulo_lista, l.descricao_lista, l.visibilidade_lista, u.nome as creator_name
                              FROM projeto.lista l
                              JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
                              WHERE l.id_lista = @listId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // Nome da lista
                    label2.Text = reader["titulo_lista"].ToString();

                    // Descrição da lista
                    panel4.Controls.Clear();
                    Label descLabel = new Label();
                    descLabel.Text = reader["descricao_lista"].ToString();
                    descLabel.AutoSize = true;
                    descLabel.MaximumSize = new Size(panel4.Width - 20, 0);
                    panel4.Controls.Add(descLabel);

                    // De quem é a lista
                    panel3.Controls.Clear();
                    Label creatorLabel = new Label();
                    creatorLabel.Text = "Created by: " + reader["creator_name"].ToString();
                    creatorLabel.AutoSize = true;
                    panel3.Controls.Add(creatorLabel);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading list data: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void LoadListEntries()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT el.id_item, j.titulo as game_title, el.estado, el.posicao, el.notas_adicionais
                              FROM projeto.entrada_lista el
                              JOIN projeto.jogo j ON el.id_jogo = j.id_jogo
                              WHERE el.id_lista = @listId
                              ORDER BY el.posicao";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("ID", 0);
                listView1.Columns.Add("Position", 50);
                listView1.Columns.Add("Game Title", 200);
                listView1.Columns.Add("Status", 100);
                listView1.Columns.Add("Notes", 150);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_item"].ToString());
                    item.SubItems.Add(reader["posicao"]?.ToString() ?? "");
                    item.SubItems.Add(reader["game_title"].ToString());
                    item.SubItems.Add(reader["estado"].ToString());
                    item.SubItems.Add(reader["notas_adicionais"]?.ToString() ?? "");
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading list entries: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void Lista_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        // Nome da lista
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        // Entradas de lista
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                // You can add functionality when a list entry is selected
            }
        }

        // Voltar para a pagina inicial
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // De quem é a lista
        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        // Descrição da lista
        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        // Vai para o meu perfil
        /*
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            UserPage profileForm = new UserPage(currentUserId);
            profileForm.Show();
            this.Hide();
        } 
        */

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // Visitar o perfil de quem fez a lista
        /*
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT id_utilizador FROM projeto.lista WHERE id_lista = @listId";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    string listOwnerId = result.ToString();
                    if (listOwnerId != currentUserId)
                    {
                        UserPage profileForm = new UserPage(listOwnerId);
                        profileForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("This is your own list.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting list owner: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
        */

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}