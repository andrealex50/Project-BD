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
    public partial class UserPage : Form
    {
        private string currentUserId; // Store the logged-in user's ID
        private string profileUserId; // Store the profile being viewed user's ID
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public UserPage(string currentUserId, string profileUserId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            this.profileUserId = profileUserId;
            LoadUserData();
            LoadUserReviews();
            LoadUserLists();
            LoadUserFriends();
            LoadReviewReactions();
            LoadUserStatistics();

            // Only show edit buttons if viewing own profile
            button1.Visible = (currentUserId == profileUserId);
            button2.Visible = (currentUserId == profileUserId);

            // Only show add friend button if viewing someone else's profile
            button3.Visible = (currentUserId != profileUserId);
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

                // Clear existing controls first to avoid duplicates
                panel4.Controls.Clear();
                panel2.Controls.Clear();

                // Load basic user info
                string query = "SELECT u.nome, p.bio, p.foto FROM projeto.utilizador u " +
                               "LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador " +
                               "WHERE u.id_utilizador = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", profileUserId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Create a label for name
                        Label nameLabel = new Label();
                        nameLabel.Text = reader["nome"].ToString();
                        nameLabel.AutoSize = true;
                        nameLabel.Font = new Font("Microsoft Sans Serif", 10);
                        panel4.Controls.Add(nameLabel);

                        // Create a label for bio
                        if (reader["bio"] != DBNull.Value)
                        {
                            TextBox bioBox = new TextBox();
                            bioBox.Text = reader["bio"].ToString();
                            bioBox.Multiline = true;
                            bioBox.ReadOnly = true;
                            bioBox.ScrollBars = ScrollBars.Vertical;
                            bioBox.Font = new Font("Microsoft Sans Serif", 9);
                            bioBox.Dock = DockStyle.Fill; // Fills the panel2 area
                            panel2.Controls.Add(bioBox);
                        }


                        // Load profile picture if exists
                        if (reader["foto"] != DBNull.Value)
                        {
                            string imagePath = reader["foto"].ToString();
                            if (System.IO.File.Exists(imagePath))
                            {
                                pictureBox4.Image = Image.FromFile(imagePath);
                            }
                            else
                            {
                                pictureBox4.Image = null; // Clear image if file doesn't exist
                            }
                        }
                        else
                        {
                            pictureBox4.Image = null; // Clear image if no path in database
                        }
                    }
                }

                // Check follow status if viewing another user's profile
                if (currentUserId != profileUserId)
                {
                    // Need a new connection for the second query
                    using (SqlConnection followCn = getSGBDConnection())
                    {
                        if (followCn.State != ConnectionState.Open)
                            followCn.Open();

                        // Check if already following this user
                        string checkFollowQuery = @"SELECT 1 FROM projeto.segue 
                                     WHERE id_utilizador_seguidor = @currentUserId 
                                     AND id_utilizador_seguido = @profileUserId";

                        using (SqlCommand checkFollowCmd = new SqlCommand(checkFollowQuery, followCn))
                        {
                            checkFollowCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                            checkFollowCmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                            object followResult = checkFollowCmd.ExecuteScalar();

                            if (followResult != null)
                            {
                                // Use Invoke only if required
                                if (button3.InvokeRequired)
                                {
                                    button3.Invoke((MethodInvoker)delegate {
                                        button3.Enabled = false;
                                        button3.Text = "Following";
                                    });
                                }
                                else
                                {
                                    button3.Enabled = false;
                                    button3.Text = "Following";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user data: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }

        private void LoadUserReviews()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT r.id_review, j.titulo, r.rating, r.descricao_review 
                              FROM projeto.review r
                              JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
                              WHERE r.id_utilizador = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("Game", 150);
                listView1.Columns.Add("Rating", 60);
                listView1.Columns.Add("Review", 300);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["titulo"].ToString());
                    item.SubItems.Add(reader["rating"].ToString());
                    item.SubItems.Add(reader["descricao_review"].ToString());
                    item.Tag = reader["id_review"].ToString(); // Store review ID in Tag
                    listView1.Items.Add(item);
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

        private void LoadUserLists()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT id_lista, titulo_lista, descricao_lista 
                              FROM projeto.lista 
                              WHERE id_utilizador = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView2.Items.Clear();
                listView2.View = View.Details;
                listView2.Columns.Clear();
                listView2.Columns.Add("Title", 150);
                listView2.Columns.Add("Description", 300);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["titulo_lista"].ToString());
                    item.SubItems.Add(reader["descricao_lista"].ToString());
                    item.Tag = reader["id_lista"].ToString(); // Store list ID in Tag
                    listView2.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading lists: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void LoadUserFriends()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT u.id_utilizador, u.nome 
                              FROM projeto.utilizador u
                              JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
                              WHERE s.id_utilizador_seguidor = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView3.Items.Clear();
                listView3.View = View.Details;
                listView3.Columns.Clear();
                listView3.FullRowSelect = true;
                listView3.Columns.Add("ID", 0);
                listView3.Columns.Add("Name", 200);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["id_utilizador"].ToString());
                    item.SubItems.Add(reader["nome"].ToString());
                    listView3.Items.Add(item);
                }
                reader.Close();

                // Update friends count label
                panel5.Controls.Clear();
                Label friendsCountLabel = new Label();
                friendsCountLabel.Text = listView3.Items.Count.ToString();
                friendsCountLabel.AutoSize = true;
                panel5.Controls.Add(friendsCountLabel);
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

        private void LoadReviewReactions()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT r.reacao_texto, u.nome, j.titulo 
                              FROM projeto.reage_a r
                              JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
                              JOIN projeto.review rev ON r.id_review = rev.id_review
                              JOIN projeto.jogo j ON rev.id_jogo = j.id_jogo
                              WHERE rev.id_utilizador = @userId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView4.Items.Clear();
                listView4.View = View.Details;
                listView4.Columns.Clear();
                listView4.Columns.Add("User", 100);
                listView4.Columns.Add("Game", 150);
                listView4.Columns.Add("Reaction", 200);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["nome"].ToString());
                    item.SubItems.Add(reader["titulo"].ToString());
                    item.SubItems.Add(reader["reacao_texto"].ToString());
                    listView4.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading review reactions: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void LoadUserStatistics()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Best reviewed game
                string bestReviewedQuery = @"SELECT TOP 1 j.titulo 
                                           FROM projeto.review r
                                           JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
                                           WHERE r.id_utilizador = @userId
                                           ORDER BY r.rating DESC";
                SqlCommand bestReviewedCmd = new SqlCommand(bestReviewedQuery, cn);
                bestReviewedCmd.Parameters.AddWithValue("@userId", profileUserId);
                string bestReviewed = bestReviewedCmd.ExecuteScalar()?.ToString() ?? "None";

                Label bestReviewedLabel = new Label();
                bestReviewedLabel.Text = bestReviewed;
                bestReviewedLabel.AutoSize = true;
                panel6.Controls.Add(bestReviewedLabel);

                // Most played game
                string mostPlayedQuery = @"SELECT TOP 1 j.titulo 
                                        FROM projeto.review r
                                        JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
                                        WHERE r.id_utilizador = @userId
                                        ORDER BY r.horas_jogadas DESC";
                SqlCommand mostPlayedCmd = new SqlCommand(mostPlayedQuery, cn);
                mostPlayedCmd.Parameters.AddWithValue("@userId", profileUserId);
                string mostPlayed = mostPlayedCmd.ExecuteScalar()?.ToString() ?? "None";

                Label mostPlayedLabel = new Label();
                mostPlayedLabel.Text = mostPlayed;
                mostPlayedLabel.AutoSize = true;
                panel7.Controls.Add(mostPlayedLabel);

                // Most reviewed genre
                string mostReviewedGenreQuery = @"SELECT TOP 1 g.nome
                                               FROM projeto.review r
                                               JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
                                               JOIN projeto.genero g ON j.id_jogo = g.id_jogo
                                               WHERE r.id_utilizador = @userId
                                               GROUP BY g.nome
                                               ORDER BY COUNT(*) DESC";
                SqlCommand mostReviewedGenreCmd = new SqlCommand(mostReviewedGenreQuery, cn);
                mostReviewedGenreCmd.Parameters.AddWithValue("@userId", profileUserId);
                string mostReviewedGenre = mostReviewedGenreCmd.ExecuteScalar()?.ToString() ?? "None";

                Label mostReviewedGenreLabel = new Label();
                mostReviewedGenreLabel.Text = mostReviewedGenre;
                mostReviewedGenreLabel.AutoSize = true;
                panel8.Controls.Add(mostReviewedGenreLabel);

                // Average review score
                string avgScoreQuery = @"SELECT AVG(CAST(r.rating AS FLOAT))
                                      FROM projeto.review r
                                      WHERE r.id_utilizador = @userId";
                SqlCommand avgScoreCmd = new SqlCommand(avgScoreQuery, cn);
                avgScoreCmd.Parameters.AddWithValue("@userId", profileUserId);
                object avgScoreObj = avgScoreCmd.ExecuteScalar();
                string avgScore = avgScoreObj != DBNull.Value ?
                    Math.Round(Convert.ToDouble(avgScoreObj), 2).ToString() : "None";

                Label avgScoreLabel = new Label();
                avgScoreLabel.Text = avgScore;
                avgScoreLabel.AutoSize = true;
                panel9.Controls.Add(avgScoreLabel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading statistics: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        // Go back to MainPage.cs
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
            MainPage mainPage = new MainPage(currentUserId);
            mainPage.Show();
        }

        // Show user foto
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (currentUserId == profileUserId)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox4.Image = Image.FromFile(openFileDialog.FileName);

                    // Update photo in database
                    try
                    {
                        cn = getSGBDConnection();
                        if (!verifySGBDConnection())
                            return;

                        string updateQuery = @"IF EXISTS (SELECT 1 FROM projeto.perfil WHERE utilizador = @userId)
                                            UPDATE projeto.perfil SET foto = @photo WHERE utilizador = @userId
                                            ELSE
                                            INSERT INTO projeto.perfil (foto, utilizador) VALUES (@photo, @userId)";

                        SqlCommand command = new SqlCommand(updateQuery, cn);
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        command.Parameters.AddWithValue("@photo", openFileDialog.FileName);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating photo: " + ex.Message);
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
        }

        // Show Personal reviews
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string reviewId = listView1.SelectedItems[0].Tag.ToString();
                ReviewDetails reviewDetails = new ReviewDetails(currentUserId, reviewId);
                reviewDetails.ShowDialog();
            }
        }

        // Show personal Lists
        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string listId = listView2.SelectedItems[0].Tag.ToString();

                try
                {
                    using (SqlConnection tempCn = getSGBDConnection())
                    {
                        tempCn.Open();
                        string query = @"SELECT titulo_lista, u.nome 
                                 FROM projeto.lista l
                                 JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
                                 WHERE id_lista = @listId";

                        SqlCommand cmd = new SqlCommand(query, tempCn);
                        cmd.Parameters.AddWithValue("@listId", listId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string listTitle = reader["titulo_lista"].ToString();
                                string creatorName = reader["nome"].ToString();

                                this.Close();
                                Lista listPage = new Lista(currentUserId, listId, listTitle, creatorName);
                                listPage.Show();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening list: " + ex.Message);
                }
            }
        }

        // Show personal friends
        private void listView3_DoubleClick(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                string friendId = listView3.SelectedItems[0].Text;
                this.Hide(); // Better to hide than close
                UserPage friendPage = new UserPage(currentUserId, friendId);
                friendPage.Closed += (s, args) => this.Close(); // Close this form when friend page closes
                friendPage.Show();
            }
        }



        // Show reactions to other reviews
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: Add functionality to view the review that was reacted to
        }

        // Show name
        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserData()
        }

        // Show bio
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserData()
        }

        // Calculate and show best reviewed game
        private void panel6_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserStatistics()
        }

        // Calculate and show most played game through number of hours played stated in the reaction to said game
        private void panel7_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserStatistics()
        }

        // Calculate and show most reviewed genre
        private void panel8_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserStatistics()
        }

        // Calculate and show average review score (rating that is given in a review)
        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            // Handled in LoadUserStatistics()
        }

        // Button to create a list. Go to Lista.cs. Only the user that owns the user account being visited can see this button and therefore create a new list from this page.
        private void button1_Click(object sender, EventArgs e)
        {
            CreateListForm createListForm = new CreateListForm(currentUserId);
            if (createListForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh the lists after creating a new one
                LoadUserLists();
            }
        }

        // Button to edit the username and bio. Only the user that owns the user account being visited can see this button and therefore update this information.
        private void button2_Click(object sender, EventArgs e)
        {
            // Extract name from panel4
            string currentName = "";
            if (panel4.Controls.Count > 0 && panel4.Controls[0] is Label nameLabel)
                {
                currentName = nameLabel.Text;
                }

            // Extract bio from panel2
            string currentBio = "";
            if (panel2.Controls.Count > 0 && panel2.Controls[0] is Label bioLabel)
            {
                currentBio = bioLabel.Text;
            }

            EditProfileForm editForm = new EditProfileForm(currentUserId, currentName, currentBio);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh user data after edit
                panel4.Controls.Clear();
                panel2.Controls.Clear();
                LoadUserData();
            }
        }

        // This is to calculate the number of friends the user has, and display them
        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        // Botão para adicionar amigo (só deve aparecer se o perfil não for nosso)
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Check if already following this user
                string checkQuery = @"SELECT 1 FROM projeto.segue 
                          WHERE id_utilizador_seguidor = @currentUserId 
                          AND id_utilizador_seguido = @profileUserId";

                SqlCommand checkCmd = new SqlCommand(checkQuery, cn);
                checkCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                checkCmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                object result = checkCmd.ExecuteScalar();

                if (result != null)
                {
                    MessageBox.Show("You are already following this user!");
                    return;
                }

                // Insert follow relationship
                string insertQuery = @"INSERT INTO projeto.segue 
                            (id_utilizador_seguidor, id_utilizador_seguido, data_seguir) 
                            VALUES (@currentUserId, @profileUserId, GETDATE())";

                SqlCommand insertCmd = new SqlCommand(insertQuery, cn);
                insertCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                insertCmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                int rowsAffected = insertCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("You are now following this user!");
                    button3.Enabled = false;
                    button3.Text = "Following";

                    // Refresh friends list to show the new relationship
                    LoadUserFriends();
                }
                else
                {
                    MessageBox.Show("Failed to follow user.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error following user: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
    }
}