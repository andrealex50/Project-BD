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
        private string listTitle;
        private string creatorName;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public Lista(string userId, string listId, string listTitle, string creatorName)
        {
            InitializeComponent();
            this.currentUserId = userId;
            this.listId = listId;
            this.listTitle = listTitle;
            this.creatorName = creatorName;
            this.Text = listTitle; 
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

                string query = @"SELECT l.descricao_lista, l.visibilidade_lista, 
                               CASE WHEN l.id_utilizador = @currentUserId THEN 1 ELSE 0 END AS is_owner
                               FROM projeto.lista l
                               WHERE l.id_lista = @listId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);
                command.Parameters.AddWithValue("@currentUserId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // Set list title
                    panel2.Controls.Clear();
                    Label titleLable = new Label();
                    titleLable.Text = listTitle;
                    titleLable.AutoSize = true;
                    panel2.Controls.Add(titleLable);

                    // Set list description
                    panel4.Controls.Clear();
                    Label descLabel = new Label();
                    descLabel.Text = reader["descricao_lista"].ToString();
                    descLabel.AutoSize = true;
                    descLabel.MaximumSize = new Size(panel4.Width - 20, 0);
                    panel4.Controls.Add(descLabel);

                    // Set creator info
                    panel3.Controls.Clear();
                    Label creatorLabel = new Label();
                    creatorLabel.Text = creatorName;
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

                string query = @"SELECT el.id_item, j.titulo as game_title, j.id_jogo, el.estado, 
                               el.posicao, el.notas_adicionais, j.capa
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
                listView1.Columns.Add("Game ID", 0);
                listView1.Columns.Add("Position", 50);
                listView1.Columns.Add("Game Title", 200);
                listView1.Columns.Add("Status", 100);
                listView1.Columns.Add("Notes", 150);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_item"].ToString());
                    item.SubItems.Add(reader["id_jogo"].ToString());
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

        }

        // Voltar para a pagina inicial
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainPage mainPage = new MainPage(currentUserId);
            mainPage.Show();
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
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            UserPage profileForm = new UserPage(currentUserId, currentUserId);
            profileForm.Show();
            this.Hide();
        } 
        

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // Visitar o perfil de quem fez a lista
        
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
                        UserPage profileForm = new UserPage(currentUserId, listOwnerId);
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
        

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Only the owner of this list can edit
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Check if current user is the owner of the list
                string query = @"SELECT id_utilizador FROM projeto.lista WHERE id_lista = @listId";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);

                object result = command.ExecuteScalar();
                if (result != null && result.ToString() == currentUserId)
                {
                    // User is the owner - show add game popup
                    ShowAddGamePopup();
                }
                else
                {
                    MessageBox.Show("Only the list owner can edit this list.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error verifying list ownership: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void ShowAddGamePopup()
        {
            Form popupForm = new Form();
            popupForm.Text = "Add Game to List";
            popupForm.Size = new Size(600, 400);
            popupForm.StartPosition = FormStartPosition.CenterParent;
            popupForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            popupForm.MaximizeBox = false;
            popupForm.MinimizeBox = false;

            // Search controls
            TextBox searchBox = new TextBox()
            {
                Left = 20,
                Top = 20,
                Width = 300,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            Button searchButton = new Button()
            {
                Text = "Search",
                Left = 330,
                Top = 20,
                Width = 80,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            searchButton.FlatAppearance.BorderSize = 1;

            // Games ListView
            ListView gamesListView = new ListView()
            {
                Left = 20,
                Top = 60,
                Width = 560,
                Height = 250,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            gamesListView.Columns.Add("ID", 0);
            gamesListView.Columns.Add("Title", 400);
            gamesListView.Columns.Add("Rating", 120);

            // Bottom controls 
            // Status section
            Label statusLabel = new Label()
            {
                Text = "Status:",
                Left = 20,
                Top = 320,
                Width = 50,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            ComboBox statusComboBox = new ComboBox()
            {
                Left = 75,
                Top = 320,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            statusComboBox.Items.AddRange(new string[] { "Jogado", "Não Jogado", "Planeia Jogar", "Desistiu" });
            statusComboBox.SelectedIndex = 0;

            // Notes section
            Label notesLabel = new Label()
            {
                Text = "Notes:",
                Left = 210,
                Top = 320,
                Width = 45,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            TextBox notesTextBox = new TextBox()
            {
                Left = 260,
                Top = 320,
                Width = 120,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            // Add button
            Button addButton = new Button()
            {
                Text = "Add to List",
                Left = 440,
                Top = 320,
                Width = 120,
                Height = 23,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            addButton.FlatAppearance.BorderSize = 0;

            // Event handlers
            searchButton.Click += (s, e) => {
                SearchGames(searchBox.Text, gamesListView);
            };

            addButton.Click += (s, e) => {
                if (gamesListView.SelectedItems.Count > 0)
                {
                    string gameId = gamesListView.SelectedItems[0].Text;
                    string status = statusComboBox.SelectedItem.ToString();
                    string notes = notesTextBox.Text;

                    AddGameToList(gameId, status, notes);
                    popupForm.Close();
                    LoadListEntries();
                }
                else
                {
                    MessageBox.Show("Please select a game to add.", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Add controls to form
            popupForm.Controls.AddRange(new Control[] {
                searchBox, searchButton, gamesListView,
                statusLabel, statusComboBox,
                notesLabel, notesTextBox,
                addButton
            });

            popupForm.ShowDialog();
        }

        private void SearchGames(string searchText, ListView listView)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_SearchGames", cn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@searchText",
                    string.IsNullOrWhiteSpace(searchText) ? (object)DBNull.Value : searchText);
                command.Parameters.AddWithValue("@genre", DBNull.Value);
                command.Parameters.AddWithValue("@platform", DBNull.Value);
                command.Parameters.AddWithValue("@minRating", 0);

                SqlDataReader reader = command.ExecuteReader();

                listView.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());
                    listView.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching games: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void AddGameToList(string gameId, string status, string notes)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // First get the next position number
                string positionQuery = @"SELECT ISNULL(MAX(posicao), 0) + 1 
              FROM projeto.entrada_lista 
              WHERE id_lista = @listId";
                SqlCommand positionCmd = new SqlCommand(positionQuery, cn);
                positionCmd.Parameters.AddWithValue("@listId", listId);
                int position = (int)positionCmd.ExecuteScalar();

                // Generate the unique ID
                string id_item_ = GenerateUniqueID();

                // Insert the new entry
                string insertQuery = @"INSERT INTO projeto.entrada_lista 
             (id_item, estado, posicao, notas_adicionais, id_jogo, id_lista)
             VALUES 
             (@id_item, @status, @position, @notes, @gameId, @listId)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, cn);
                insertCmd.Parameters.AddWithValue("@id_item", id_item_);
                insertCmd.Parameters.AddWithValue("@status", status);
                insertCmd.Parameters.AddWithValue("@position", position);
                insertCmd.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                insertCmd.Parameters.AddWithValue("@gameId", gameId);
                insertCmd.Parameters.AddWithValue("@listId", listId);

                int rowsAffected = insertCmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Game added to list successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding game to list: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Gera algo tipo "E001", "E002"
        private string GenerateUniqueID()
        {
            try
            {
                // Count entries for this specific list to avoid conflicts
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM projeto.entrada_lista WHERE id_lista = @listId",
                    cn);
                cmd.Parameters.AddWithValue("@listId", listId);
                int count = (int)cmd.ExecuteScalar();
                return "E" + (count + 1).ToString("D3"); // Example: "E001"
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating ID: " + ex.Message);
                return "E" + DateTime.Now.Ticks.ToString().Substring(0, 10); // Fallback with timestamp
            }
        }
    }
}