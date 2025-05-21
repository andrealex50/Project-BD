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
    public partial class GamePage : Form
    {
        private string currentUserId;
        private string gameId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public GamePage(string userId, string gameId)
        {
            InitializeComponent();
            currentUserId = userId;
            this.gameId = gameId;
            LoadGameData();
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

        private void LoadGameData()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Load game basic info
                string query = @"SELECT j.titulo, j.data_lancamento, j.sinopse, j.capa, 
                                j.rating_medio, j.tempo_medio_gameplay, j.preco
                                FROM projeto.jogo j
                                WHERE j.id_jogo = @gameId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // Set game title
                    panel2.Controls.Clear();
                    panel2.Controls.Add(new Label()
                    {
                        Text = reader["titulo"].ToString(),
                        Font = new Font("Microsoft Sans Serif", 12F),
                        AutoSize = true
                    });

                    // Set game cover image if exists
                    if (reader["capa"] != DBNull.Value)
                    {
                        string imagePath = reader["capa"].ToString();
                        if (System.IO.File.Exists(imagePath))
                        {
                            pictureBox4.Image = Image.FromFile(imagePath);
                        }
                    }

                    // Set sinopse
                    panel4.Controls.Clear();
                    panel4.Controls.Add(new Label()
                    {
                        Text = reader["sinopse"].ToString(),
                        AutoSize = true,
                        MaximumSize = new Size(panel4.Width - 10, 0)
                    });

                    // Set launch date
                    panel5.Controls.Clear();
                    panel5.Controls.Add(new Label()
                    {
                        Text = ((DateTime)reader["data_lancamento"]).ToString("yyyy-MM-dd"),
                        AutoSize = true
                    });

                    // Set rating
                    panel6.Controls.Clear();
                    panel6.Controls.Add(new Label()
                    {
                        Text = reader["rating_medio"].ToString(),
                        AutoSize = true
                    });

                    // Set gameplay time
                    panel7.Controls.Clear();
                    panel7.Controls.Add(new Label()
                    {
                        Text = reader["tempo_medio_gameplay"].ToString() + " hours",
                        AutoSize = true
                    });

                    // Set price
                    panel8.Controls.Clear();
                    panel8.Controls.Add(new Label()
                    {
                        Text = reader["preco"].ToString() + " €",
                        AutoSize = true
                    });
                }
                reader.Close();

                // Load genres
                query = @"SELECT g.nome FROM projeto.genero g WHERE g.id_jogo = @gameId";
                command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);
                reader = command.ExecuteReader();

                panel10.Controls.Clear();
                while (reader.Read())
                {
                    panel10.Controls.Add(new Label()
                    {
                        Text = reader["nome"].ToString(),
                        AutoSize = true,
                        Margin = new Padding(0, 0, 10, 0)
                    });
                }
                reader.Close();

                // Load developers
                query = @"SELECT d.nome FROM projeto.desenvolvedor d WHERE d.id_jogo = @gameId";
                command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);
                reader = command.ExecuteReader();

                panel11.Controls.Clear();
                while (reader.Read())
                {
                    panel11.Controls.Add(new Label()
                    {
                        Text = reader["nome"].ToString(),
                        AutoSize = true,
                        Margin = new Padding(0, 0, 10, 0)
                    });
                }
                reader.Close();

                // Load platforms
                query = @"SELECT p.sigla FROM projeto.plataforma p WHERE p.id_jogo = @gameId";
                command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);
                reader = command.ExecuteReader();

                panel12.Controls.Clear();
                while (reader.Read())
                {
                    panel12.Controls.Add(new Label()
                    {
                        Text = reader["sigla"].ToString(),
                        AutoSize = true,
                        Margin = new Padding(0, 0, 10, 0)
                    });
                }
                reader.Close();

                // Load reviews
                LoadReviews("All");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading game data: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void LoadReviews(string filter)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT r.descricao_review, u.nome, r.rating, r.data_review
                                FROM projeto.review r
                                JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
                                WHERE r.id_jogo = @gameId";

                if (filter == "Friends")
                {
                    query += @" AND projeto.fn_IsFriend(@currentUserId, r.id_utilizador) = 1";
                }
                else if (filter == "Made by Mods")
                {
                    query += @" AND EXISTS (
                                SELECT 1 FROM projeto.utilizador u 
                                WHERE u.id_utilizador = r.id_utilizador AND u.nome LIKE '%Mod%'
                              )";
                }

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);
                command.Parameters.AddWithValue("@currentUserId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();

                panel9.Controls.Clear();
                panel9.AutoScroll = true;
                int yPos = 10;

                while (reader.Read())
                {
                    GroupBox reviewBox = new GroupBox();
                    reviewBox.Text = $"{reader["nome"]} - Rating: {reader["rating"]}/5 - {((DateTime)reader["data_review"]).ToString("yyyy-MM-dd")}";
                    reviewBox.Width = panel9.Width - 25;
                    reviewBox.Height = 80;
                    reviewBox.Location = new Point(10, yPos);
                    yPos += reviewBox.Height + 10;

                    Label reviewText = new Label();
                    reviewText.Text = reader["descricao_review"].ToString();
                    reviewText.AutoSize = true;
                    reviewText.MaximumSize = new Size(reviewBox.Width - 20, 0);
                    reviewText.Location = new Point(10, 20);

                    reviewBox.Controls.Add(reviewText);
                    panel9.Controls.Add(reviewBox);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reviews: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void GamePage_Load(object sender, EventArgs e)
        {
            // Load username in the header
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = "SELECT nome FROM projeto.utilizador WHERE id_utilizador = @userId";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    label3.Text = result.ToString();
                }
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        // Go to user profile
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
            UserPage userPageForm = new UserPage(currentUserId, currentUserId);
            userPageForm.Show();
        }

        // Game cover load completed
        private void pictureBox4_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // No action needed
        }

        // Go back to MainPage
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
            MainPage mainPageForm = new MainPage(currentUserId);
            mainPageForm.Show();
        }

        // Place a review
        private void button1_Click(object sender, EventArgs e)
        {
            //this.Close();
            //ReviewPage reviewPageForm = new ReviewPage(currentUserId, gameId);
            //reviewPageForm.Show();
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        // Filter reviews
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filter = comboBox1.SelectedItem.ToString();
            LoadReviews(filter);
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            // No action needed
        }
    }
}