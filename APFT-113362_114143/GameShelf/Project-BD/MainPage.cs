using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class MainPage : Form
    {
        private readonly string currentUserId; // Store the logged-in user's ID
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();
        private string currentGenreFilter = null;
        private string currentPlatformFilter = null;
        private int currentMinRating = 0;


        public MainPage(String userId)
        {
            InitializeComponent();
            currentUserId = userId;

            comboBox1.Items.Clear(); // Clear existing items if any
            comboBox1.Items.AddRange(new string[] { "All", "Friends", "Mine", "MadeByMods" });
            comboBox1.SelectedIndex = 0;

            listView4.FullRowSelect = true;
            listView4.View = View.Details;
            listView4.MultiSelect = false;

            listView3.FullRowSelect = true;
            listView3.MultiSelect = false;

            ApplyListFilters();
            LoadUserData();
            LoadAllGames();
            LoadUserLists();
            LoadFriends();
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private bool verifySGBDConnection()
        {
            try
            {
                if (cn == null)
                    cn = getSGBDConnection();

                if (cn.State != ConnectionState.Open)
                    cn.Open();

                return cn.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                return false;
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


        private void LoadAllGames()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_SearchGames", cn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@searchText", DBNull.Value);
                command.Parameters.AddWithValue("@genre", DBNull.Value);
                command.Parameters.AddWithValue("@platform", DBNull.Value);
                command.Parameters.AddWithValue("@minRating", 0);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("ID", 0);
                listView1.Columns.Add("Title", 200);
                listView1.Columns.Add("Rating", 60);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading games: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }


        private void LoadFriends()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT u.id_utilizador, u.nome 
                              FROM projeto.utilizador u
                              WHERE projeto.fn_IsFriend(@currentUserId, u.id_utilizador) = 1";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@currentUserId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView3.Items.Clear();
                listView3.View = View.Details;
                listView3.Columns.Clear();
                listView3.Columns.Add("ID", 0);
                listView3.Columns.Add("Name", 200);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_utilizador"].ToString());
                    item.SubItems.Add(reader["nome"].ToString());
                    listView3.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading friends: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        // Mostrar os jogos (por default mostra todos)
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count > 0)
            {
                string selectedGameId = listView1.SelectedItems[0].Text;

                this.Hide();
                GamePage gamePageForm = new GamePage(currentUserId, selectedGameId);
                gamePageForm.Show();
            }
        }

        // Abre perfil
        private void pictureBox2_Click_2(object sender, EventArgs e)
        {
            this.Hide();
            UserPage userPageForm = new UserPage(currentUserId, currentUserId);
            userPageForm.Show();
        }

        // Mostrar os amigos
        private void listView3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                string selectedFriendId = listView3.SelectedItems[0].Text;
                string selectedFriendName = listView3.SelectedItems[0].SubItems[1].Text;

                this.Hide();
                UserPage friendProfile = new UserPage(currentUserId, selectedFriendId);
                friendProfile.Show();
            }
        }

        // Botão para abrir pop-up filtros dos jogos
        private void button1_Click_1(object sender, EventArgs e)
        {
            Form filtersForm = new Form();
            filtersForm.Text = "Filtrar Jogos";
            filtersForm.Size = new Size(300, 400);

            Label genreLabel = new Label() { Text = "Gênero:", Left = 20, Top = 20 };
            ComboBox genreComboBox = new ComboBox() { Left = 20, Top = 50, Width = 200 };

            Label platformLabel = new Label() { Text = "Plataforma:", Left = 20, Top = 90 };
            ComboBox platformComboBox = new ComboBox() { Left = 20, Top = 120, Width = 200 };

            Label ratingLabel = new Label() { Text = "Rating Mínimo:", Left = 20, Top = 160 };
            NumericUpDown ratingNumeric = new NumericUpDown() { Left = 20, Top = 190, Width = 200, Minimum = 0, Maximum = 5 };

            Button applyButton = new Button() { Text = "Aplicar Filtros", Left = 20, Top = 250, Width = 100 };

            try
            {
                cn = getSGBDConnection();
                if (verifySGBDConnection())
                {
                    // Load genres
                    SqlCommand genreCmd = new SqlCommand("SELECT DISTINCT nome FROM projeto.genero", cn);
                    SqlDataReader genreReader = genreCmd.ExecuteReader();
                    genreComboBox.Items.Add("Todos");
                    while (genreReader.Read())
                    {
                        genreComboBox.Items.Add(genreReader["nome"].ToString());
                    }
                    genreReader.Close();
                    genreComboBox.SelectedIndex = 0;

                    // Load platforms
                    SqlCommand platformCmd = new SqlCommand("SELECT DISTINCT sigla FROM projeto.plataforma", cn);
                    SqlDataReader platformReader = platformCmd.ExecuteReader();
                    platformComboBox.Items.Add("Todas");
                    while (platformReader.Read())
                    {
                        platformComboBox.Items.Add(platformReader["sigla"].ToString());
                    }
                    platformReader.Close();
                    platformComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar filtros: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }

            // Apply filters button click
            applyButton.Click += (s, args) => {
                currentGenreFilter = genreComboBox.SelectedIndex > 0 ? genreComboBox.SelectedItem.ToString() : null;
                currentPlatformFilter = platformComboBox.SelectedIndex > 0 ? platformComboBox.SelectedItem.ToString() : null;
                currentMinRating = (int)ratingNumeric.Value;

                FilterGames(textBox1.Text.Trim(), currentGenreFilter, currentPlatformFilter, currentMinRating);
                filtersForm.Close();
            };

            // Add controls to form
            filtersForm.Controls.Add(genreLabel);
            filtersForm.Controls.Add(genreComboBox);
            filtersForm.Controls.Add(platformLabel);
            filtersForm.Controls.Add(platformComboBox);
            filtersForm.Controls.Add(ratingLabel);
            filtersForm.Controls.Add(ratingNumeric);
            filtersForm.Controls.Add(applyButton);

            filtersForm.ShowDialog();
        }

        private void FilterGames(string searchText, string genre, string platform, int minRating)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_SearchGames", cn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@searchText",
                    string.IsNullOrWhiteSpace(searchText) || searchText == "Search for game..." ?
                    (object)DBNull.Value : searchText);
                command.Parameters.AddWithValue("@genre",
                    string.IsNullOrEmpty(genre) ? (object)DBNull.Value : genre);
                command.Parameters.AddWithValue("@platform",
                    string.IsNullOrEmpty(platform) ? (object)DBNull.Value : platform);
                command.Parameters.AddWithValue("@minRating", minRating);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao filtrar jogos: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Pesquisar por amigo
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox2.Text.Trim();

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Usando a UDF fn_IsFriend na pesquisa
                string query = @"SELECT u.id_utilizador, u.nome 
                              FROM projeto.utilizador u
                              WHERE projeto.fn_IsFriend(@currentUserId, u.id_utilizador) = 1
                              AND u.nome LIKE @searchText";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@currentUserId", currentUserId);
                command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                SqlDataReader reader = command.ExecuteReader();

                listView3.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_utilizador"].ToString());
                    item.SubItems.Add(reader["nome"].ToString());
                    listView3.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao pesquisar amigos: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Pesquisar por jogo
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();
            FilterGames(searchText, currentGenreFilter, currentPlatformFilter, currentMinRating);
        }

        // Mostrar as listas
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems.Count > 0)
                {
                    string selectedListId = listView2.SelectedItems[0].Text;
                    string listTitle = listView2.SelectedItems[0].SubItems[1].Text;
                    string creatorName = listView2.SelectedItems[0].SubItems[2].Text;

                    this.Hide();

                    Lista listaForm = new Lista(currentUserId, selectedListId, listTitle, creatorName);
                    listaForm.Show(); 
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ApplyListFilters()
        {
            
            string searchText = textBox4.Text.Trim();
            string selectedFilter = comboBox1.SelectedItem?.ToString() ?? "All";


            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                SqlCommand command = new SqlCommand("projeto.sp_SearchLists", cn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@currentUserId", currentUserId);
                command.Parameters.AddWithValue("@searchText",
                    string.IsNullOrEmpty(searchText) ? DBNull.Value : (object)searchText);
                command.Parameters.AddWithValue("@filterType", selectedFilter);


                SqlDataReader reader = command.ExecuteReader();

                listView2.Items.Clear();
                listView2.View = View.Details;
                listView2.Columns.Clear();
                listView2.Columns.Add("ID", 0);
                listView2.Columns.Add("Título", 180);
                listView2.Columns.Add("Criador", 120);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_lista"].ToString());
                    item.SubItems.Add(reader["titulo_lista"].ToString());
                    item.SubItems.Add(reader["criador"].ToString());
                    listView2.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao filtrar listas: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void LoadUserLists()
        {
            ApplyListFilters();
        }

        // Pesquisar por lista
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            ApplyListFilters();
        }

        // Filtrar listas
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyListFilters();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Pesquisar por utilizador
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox3.Text.Trim();

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT u.id_utilizador, u.nome, p.foto 
                        FROM projeto.utilizador u
                        LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
                        WHERE u.nome LIKE @searchText AND u.id_utilizador != @currentUserId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");
                command.Parameters.AddWithValue("@currentUserId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView4.Items.Clear();
                listView4.View = View.Details;
                listView4.Columns.Clear();
                listView4.Columns.Add("ID", 0); 
                listView4.Columns.Add("Name", 200);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_utilizador"].ToString());
                    item.SubItems.Add(reader["nome"].ToString());

                    listView4.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao pesquisar utilizadores: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Mostra utilizadores conforme a pesquisa
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                string selectedUserId = listView4.SelectedItems[0].Text;
                string selectedUserName = listView4.SelectedItems[0].SubItems[1].Text;

                this.Hide();
                UserPage userPageForm = new UserPage(currentUserId, selectedUserId);
                userPageForm.Show();
            }
        }

        // Adiciona o utilizador selecionado no listView4_SelectedIndexChanged
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Search for game...")
            {
                textBox1.Text = "";
                textBox1.ForeColor = SystemColors.WindowText; // Reset to default text color
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Search for game...";
                textBox1.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBox1_Enter_1(object sender, EventArgs e)
        {
            if (textBox1.Text == "Search for game...")
            {
                textBox1.Text = "";
                textBox1.ForeColor = SystemColors.WindowText; 
            }
        }

        private void textBox1_Leave_1(object sender, EventArgs e)
        {

        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Search for friend...")
            {
                textBox2.Text = "";
                textBox2.ForeColor = SystemColors.WindowText; 
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {

        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "Search for user...")
            {
                textBox3.Text = "";
                textBox3.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {

        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "Search for list...")
            {
                textBox4.Text = "";
                textBox4.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {

        }
    }
}
