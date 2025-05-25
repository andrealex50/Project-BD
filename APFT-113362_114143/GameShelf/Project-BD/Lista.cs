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
        private bool listUsesPositions = true;

        public Lista(string userId, string listId, string listTitle, string creatorName)
        {
            InitializeComponent();
            currentUserId = userId;
            this.listId = listId;
            this.listTitle = listTitle;
            this.creatorName = creatorName;
            Text = listTitle; 
            LoadListData();
            LoadListEntries();
            LoadUserData();
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
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
               l.usa_posicoes,  -- Add this
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

                    listUsesPositions = Convert.ToBoolean(reader["usa_posicoes"]);

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

        private void LoadUserData()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = "SELECT u.nome, p.foto FROM projeto.utilizador u " +
                               "LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador " +
                               "WHERE u.id_utilizador = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    label1.Text = reader["nome"].ToString();

                    // Load profile picture if exists
                    if (reader["foto"] != DBNull.Value)
                    {
                        string imagePath = reader["foto"].ToString();
                        if (System.IO.File.Exists(imagePath))
                        {
                            pictureBox2.Image = Image.FromFile(imagePath);
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user data: " + ex.Message);
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
                ORDER BY " + (listUsesPositions ? "el.posicao" : "j.titulo");

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("ID", 0);
                listView1.Columns.Add("Game ID", 0);
                if (listUsesPositions)
                {
                    listView1.Columns.Add("Position", 50);
                }
                listView1.Columns.Add("Game Title", 200);
                listView1.Columns.Add("Status", 100);
                listView1.Columns.Add("Notes", 150);


                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_item"].ToString());
                    item.SubItems.Add(reader["id_jogo"].ToString());
                    if (listUsesPositions)
                    {
                        item.SubItems.Add(reader["posicao"]?.ToString() ?? "");
                    }
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

        

        private void label4_Click(object sender, EventArgs e)
        {

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

                string query = @"SELECT projeto.fn_IsListOwner(@userId, @listId)";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@listId", listId);

                bool isOwner = Convert.ToBoolean(command.ExecuteScalar());
                if (isOwner)
                {
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

            NumericUpDown positionControl = null;

            if (listUsesPositions)
            {
                Label positionLabel = new Label()
                {
                    Text = "Position:",
                    Left = 20,
                    Top = 350,
                    Width = 60,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                };

                positionControl = new NumericUpDown()
                {
                    Left = 85,
                    Top = 350,
                    Width = 60,
                    Minimum = 1,
                    Maximum = 1000,
                    Value = listView1.Items.Count + 1,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                };

                popupForm.Controls.Add(positionLabel);
                popupForm.Controls.Add(positionControl);
            }

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
                    int? position = listUsesPositions ? (int)positionControl.Value : (int?)null;

                    AddGameToList(gameId, status, notes, position);
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

        private void AddGameToList(string gameId, string status, string notes, int? position = null)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_AddGameToList", cn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@listId", listId);
                command.Parameters.AddWithValue("@gameId", gameId);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                command.Parameters.AddWithValue("@position", position.HasValue ? (object)position.Value : DBNull.Value);

                command.ExecuteNonQuery();
                MessageBox.Show("Game added to list successfully!");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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

        // Visitar o perfil de quem fez a lista
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_GetListOwner", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@listId", listId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string listOwnerId = reader["id_utilizador"].ToString();
                    reader.Close();

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
                else
                {
                    reader.Close();
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

        //Entradas das listas
        private void listView1_MouseDoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    string gameId = listView1.SelectedItems[0].SubItems[1].Text;
                    if (!string.IsNullOrEmpty(gameId))
                    {
                        this.Hide();

                        GamePage gamePage = new GamePage(currentUserId, gameId);
                        gamePage.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening game: " + ex.Message);
            }
        }

        // Vai para o meu perfil
        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            UserPage profileForm = new UserPage(currentUserId, currentUserId);
            profileForm.Show();
            this.Hide();
        }

    }
}