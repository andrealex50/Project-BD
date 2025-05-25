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
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            listView4.MouseDoubleClick += listView4_MouseDoubleClick;
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

                // Load user info using stored procedure
                SqlCommand command = new SqlCommand("projeto.sp_GetUserProfile", cn);
                command.CommandType = CommandType.StoredProcedure;
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
                            bioBox.Dock = DockStyle.Fill;
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
                                pictureBox4.Image = null;
                            }
                        }
                        else
                        {
                            pictureBox4.Image = null;
                        }
                    }
                }

                // Check follow status if viewing another user's profile
                if (currentUserId != profileUserId)
                {
                    // Use the existing fn_IsFriend UDF
                    SqlCommand followCmd = new SqlCommand(
                        "SELECT projeto.fn_IsFriend(@currentUserId, @profileUserId)", cn);
                    followCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    followCmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                    bool isFollowing = Convert.ToBoolean(followCmd.ExecuteScalar());

                    if (isFollowing)
                    {
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

                SqlCommand command = new SqlCommand("projeto.sp_GetUserReviews", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView1.Items.Clear();
                listView1.View = View.Details;
                listView1.Columns.Clear();
                listView1.Columns.Add("Game", 150);
                listView1.Columns.Add("Rating", 60);
                listView1.Columns.Add("Review", 300);
                listView1.Columns.Add("Date", 100);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["game_title"].ToString());
                    item.SubItems.Add(reader["rating"].ToString());
                    item.SubItems.Add(reader["review_text"].ToString());
                    item.SubItems.Add(Convert.ToDateTime(reader["review_date"]).ToString("dd/MM/yyyy"));
                    item.Tag = reader["id_review"].ToString();
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

                SqlCommand command = new SqlCommand("projeto.sp_GetUserLists", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userId", profileUserId);
                command.Parameters.AddWithValue("@currentUserId", currentUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView2.Items.Clear();
                listView2.View = View.Details;
                listView2.Columns.Clear();
                listView2.Columns.Add("Title", 150);
                listView2.Columns.Add("Description", 200);
                listView2.Columns.Add("Visibility", 80);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["titulo_lista"].ToString());
                    item.SubItems.Add(reader["descricao_lista"]?.ToString() ?? "");
                    item.SubItems.Add(reader["visibilidade_lista"].ToString());
                    item.Tag = reader["id_lista"].ToString();
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

                SqlCommand command = new SqlCommand("projeto.sp_GetUserFollowing", cn);
                command.CommandType = CommandType.StoredProcedure;
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

                SqlCommand command = new SqlCommand("projeto.sp_GetUserReviewReactions", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userId", profileUserId);

                SqlDataReader reader = command.ExecuteReader();

                listView4.Items.Clear();
                listView4.View = View.Details;
                listView4.Columns.Clear();
                listView4.Columns.Add("User", 100);
                listView4.Columns.Add("Game", 150);
                listView4.Columns.Add("Reaction", 200);
                listView4.Columns.Add("Date", 100);

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["reactor_name"].ToString());
                    item.SubItems.Add(reader["game_title"].ToString());
                    item.SubItems.Add(reader["reaction_text"].ToString());
                    item.SubItems.Add(Convert.ToDateTime(reader["reaction_date"]).ToString("dd/MM/yyyy"));
                    item.Tag = reader["id_review"].ToString();
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

                SqlCommand command = new SqlCommand("projeto.sp_GetUserGameStats", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userId", profileUserId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Best reviewed game
                    if (reader.Read())
                    {
                        panel6.Controls.Clear();
                        Label bestReviewedLabel = new Label();
                        bestReviewedLabel.Text = reader["best_reviewed_game"]?.ToString() ?? "None";
                        bestReviewedLabel.AutoSize = true;
                        panel6.Controls.Add(bestReviewedLabel);
                    }

                    // Most played game
                    if (reader.NextResult() && reader.Read())
                    {
                        panel7.Controls.Clear();
                        Label mostPlayedLabel = new Label();
                        mostPlayedLabel.Text = reader["most_played_game"]?.ToString() ?? "None";
                        mostPlayedLabel.AutoSize = true;
                        panel7.Controls.Add(mostPlayedLabel);
                    }

                    // Most reviewed genre
                    if (reader.NextResult() && reader.Read())
                    {
                        panel8.Controls.Clear();
                        Label mostReviewedGenreLabel = new Label();
                        mostReviewedGenreLabel.Text = reader["most_reviewed_genre"]?.ToString() ?? "None";
                        mostReviewedGenreLabel.AutoSize = true;
                        panel8.Controls.Add(mostReviewedGenreLabel);
                    }

                    // Average review score
                    if (reader.NextResult() && reader.Read())
                    {
                        panel9.Controls.Clear();
                        Label avgScoreLabel = new Label();
                        avgScoreLabel.Text = reader["avg_rating"] != DBNull.Value ?
                            Math.Round(Convert.ToDouble(reader["avg_rating"]), 2).ToString() : "None";
                        avgScoreLabel.AutoSize = true;
                        panel9.Controls.Add(avgScoreLabel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading statistics: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
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
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string reviewId = listView1.SelectedItems[0].Tag.ToString();
                ReviewDetails reviewDetails = new ReviewDetails(currentUserId, reviewId);
                reviewDetails.ShowDialog();

                LoadUserReviews();
                LoadUserStatistics();
                LoadReviewReactions();
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

                        // Use the existing stored procedure
                        SqlCommand cmd = new SqlCommand("projeto.sp_GetListOwner", tempCn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@listId", listId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string creatorName = reader["nome"].ToString();
                                string listTitle = listView2.SelectedItems[0].Text; // We already have the title in the ListView

                                this.Close();
                                Lista listPage = new Lista(currentUserId, listId, listTitle, creatorName);
                                listPage.ShowDialog();

                                LoadUserLists();
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
                this.Hide();
                UserPage friendPage = new UserPage(currentUserId, friendId);
                friendPage.Closed += (s, args) => this.Close(); // Close this form when friend page closes
                friendPage.Show();
            }
        }


        // Show reactions to other reviews
        private void listView4_MouseDoubleClick(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count == 0)
                return;

            var selectedItem = listView4.SelectedItems[0];
            string reactedReviewId = selectedItem.Tag?.ToString();

            if (string.IsNullOrEmpty(reactedReviewId))
            {
                MessageBox.Show("No review ID found for this reaction.");
                return;
            }

            ReviewDetails reviewDetailsForm = new ReviewDetails(currentUserId, reactedReviewId);
            reviewDetailsForm.ShowDialog();

            LoadUserReviews();
            LoadUserStatistics();
            LoadReviewReactions();
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

        // Botão para adicionar amigo (só deve aparecer se o perfil não for nosso)
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                if (button3.Text == "Follow")
                {
                    // Use the existing fn_IsFriend UDF to check first
                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT projeto.fn_IsFriend(@currentUserId, @profileUserId)", cn);
                    checkCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    checkCmd.Parameters.AddWithValue("@profileUserId", profileUserId);
                    bool alreadyFollowing = Convert.ToBoolean(checkCmd.ExecuteScalar());

                    if (alreadyFollowing)
                    {
                        MessageBox.Show("You are already following this user!");
                        return;
                    }

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
                        button3.Text = "Unfollow";
                        LoadUserFriends();
                    }
                }
                else if (button3.Text == "Unfollow")
                {
                    // Remove follow relationship
                    string deleteQuery = @"DELETE FROM projeto.segue 
                WHERE id_utilizador_seguidor = @currentUserId 
                AND id_utilizador_seguido = @profileUserId";

                    SqlCommand deleteCmd = new SqlCommand(deleteQuery, cn);
                    deleteCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    deleteCmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("You have unfollowed this user.");
                        button3.Text = "Follow";
                        LoadUserFriends();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing follow/unfollow: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void UserPage_Load(object sender, EventArgs e)
        {
            CheckFollowStatus();
        }

        private void CheckFollowStatus()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT projeto.fn_IsFriend(@currentUserId, @profileUserId)";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                cmd.Parameters.AddWithValue("@profileUserId", profileUserId);

                object result = cmd.ExecuteScalar();

                if (result != null && Convert.ToBoolean(result))
                {
                    // Já é amigo (segue)
                    button3.Text = "Unfollow";
                    button3.Enabled = true;
                }
                else
                {
                    // Não é amigo
                    button3.Text = "Follow";
                    button3.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao verificar status de amizade: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

    }
}