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
    public partial class MainPage : Form
    {
        private string currentUserId; // Store the logged-in user's ID
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public MainPage(String userId)
        {
            InitializeComponent();
            currentUserId = userId;
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
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
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

                string query = "SELECT id_jogo, titulo, capa, rating_medio FROM projeto.jogo";
                SqlCommand command = new SqlCommand(query, cn);
                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("ID", 0);
                listView1.Columns.Add("Title", 200);
                listView1.Columns.Add("Rating", 60);
                listView1.Columns.Add("Cover", 100);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());

                    // Handle cover image
                    if (reader["capa"] != DBNull.Value)
                    {
                        string imagePath = reader["capa"].ToString();
                        if (System.IO.File.Exists(imagePath))
                        {
                            item.ImageKey = imagePath;
                        }
                    }

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

        private void LoadUserLists()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Primeiro carrega as listas dos Mods (Administradores)
                string query = @"SELECT l.id_lista, l.titulo_lista, u.nome AS criador
                        FROM projeto.lista l
                        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
                        WHERE u.nome = 'admin'
                        ORDER BY l.titulo_lista";

                SqlCommand command = new SqlCommand(query, cn);
                SqlDataReader reader = command.ExecuteReader();

                // Configurar a ListView
                listView2.Items.Clear();
                listView2.View = View.Details;
                listView2.Columns.Clear();
                listView2.Columns.Add("ID", 0); // Coluna oculta
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
                MessageBox.Show("Erro ao carregar listas: " + ex.Message);
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

                string query = "SELECT u.id_utilizador, u.nome FROM projeto.utilizador u " +
                              "INNER JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido " +
                              "WHERE s.id_utilizador_seguidor = @userId";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);

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

        // Abre perfil
        private void pictureBox2_Click_2(object sender, EventArgs e)
        {

        }

        // Mostrar os amigos
        private void listView3_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        // Botão para abrir pop-up filtros dos jogos
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Create a new form for filters
            Form filtersForm = new Form();
            filtersForm.Text = "Filtrar Jogos";
            filtersForm.Size = new Size(300, 400);

            // Add filter controls
            Label genreLabel = new Label() { Text = "Gênero:", Left = 20, Top = 20 };
            ComboBox genreComboBox = new ComboBox() { Left = 20, Top = 50, Width = 200 };

            Label platformLabel = new Label() { Text = "Plataforma:", Left = 20, Top = 90 };
            ComboBox platformComboBox = new ComboBox() { Left = 20, Top = 120, Width = 200 };

            Label ratingLabel = new Label() { Text = "Rating Mínimo:", Left = 20, Top = 160 };
            NumericUpDown ratingNumeric = new NumericUpDown() { Left = 20, Top = 190, Width = 200, Minimum = 0, Maximum = 5 };

            Button applyButton = new Button() { Text = "Aplicar Filtros", Left = 20, Top = 250, Width = 100 };

            // Populate combo boxes from database
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
                string genreFilter = genreComboBox.SelectedIndex > 0 ? genreComboBox.SelectedItem.ToString() : null;
                string platformFilter = platformComboBox.SelectedIndex > 0 ? platformComboBox.SelectedItem.ToString() : null;
                int minRating = (int)ratingNumeric.Value;

                FilterGames(genreFilter, platformFilter, minRating);
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

        private void FilterGames(string genre, string platform, int minRating)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT j.id_jogo, j.titulo, j.capa, j.rating_medio 
                        FROM projeto.jogo j";

                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(genre))
                {
                    query += " JOIN projeto.genero g ON j.id_jogo = g.id_jogo";
                    conditions.Add("g.nome = @genre");
                    parameters.Add(new SqlParameter("@genre", genre));
                }

                if (!string.IsNullOrEmpty(platform))
                {
                    query += " JOIN projeto.plataforma p ON j.id_jogo = p.id_jogo";
                    conditions.Add("p.sigla = @platform");
                    parameters.Add(new SqlParameter("@platform", platform));
                }

                if (minRating > 0)
                {
                    conditions.Add("j.rating_medio >= @minRating");
                    parameters.Add(new SqlParameter("@minRating", minRating));
                }

                if (conditions.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", conditions);
                }

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddRange(parameters.ToArray());

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());

                    if (reader["capa"] != DBNull.Value)
                    {
                        string imagePath = reader["capa"].ToString();
                        if (System.IO.File.Exists(imagePath))
                        {
                            item.ImageKey = imagePath;
                        }
                    }

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

                string query = @"SELECT u.id_utilizador, u.nome 
                                FROM projeto.utilizador u
                                INNER JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
                                WHERE s.id_utilizador_seguidor = @userId AND u.nome LIKE @searchText";
        
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);
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

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT id_jogo, titulo, capa, rating_medio 
                        FROM projeto.jogo
                        WHERE titulo LIKE @searchText";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_jogo"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating_medio"].ToString());

                    if (reader["capa"] != DBNull.Value)
                    {
                        string imagePath = reader["capa"].ToString();
                        if (System.IO.File.Exists(imagePath))
                        {
                            item.ImageKey = imagePath;
                        }
                    }

                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao pesquisar jogos: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Mostrar as listas
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Pesquisar por lista
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        // Filtrar listas
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Pesquisar por utilizador
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        // Mostra utilizadores conforme a pesquisa
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {

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
    }
}
