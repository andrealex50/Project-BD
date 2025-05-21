using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class ReviewPage : Form
    {
        private string currentUserId;
        private string gameId;
        private string reviewId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public ReviewPage(string userId, string gameId)
        {
            InitializeComponent();
            currentUserId = userId;
            this.gameId = gameId;
            LoadGameInfo();
            LoadExistingReview();
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

        private string GenerateReviewId()
        {
            try
            {
                using (var tempCn = getSGBDConnection())
                {
                    tempCn.Open();

                    // Get the maximum existing review ID
                    string query = "SELECT MAX(id_review) FROM projeto.review";
                    SqlCommand cmd = new SqlCommand(query, tempCn);
                    object result = cmd.ExecuteScalar();

                    if (result == DBNull.Value || result == null)
                    {
                        return "R001"; // First review
                    }

                    string maxId = result.ToString();
                    if (maxId.StartsWith("R") && int.TryParse(maxId.Substring(1), out int number))
                    {
                        return $"R{(number + 1):D3}"; // Increment and format
                    }

                    // Fallback if ID format is unexpected
                    return "R" + Guid.NewGuid().ToString("N").Substring(0, 3);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating review ID: " + ex.Message);
                return "R" + DateTime.Now.Ticks.ToString().Substring(0, 10); // Fallback
            }
        }

        private void LoadGameInfo()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = "SELECT titulo FROM projeto.jogo WHERE id_jogo = @gameId";
                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    lblGameTitle.Text = result.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading game info: " + ex.Message);
            }
            finally
            {
                cn?.Close();
            }
        }

        private void LoadExistingReview()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT id_review, horas_jogadas, rating, descricao_review 
                               FROM projeto.review 
                               WHERE id_utilizador = @userId AND id_jogo = @gameId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@gameId", gameId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    reviewId = reader["id_review"].ToString();
                    numHoursPlayed.Value = Convert.ToDecimal(reader["horas_jogadas"]);
                    numRating.Value = Convert.ToDecimal(reader["rating"]);
                    txtReview.Text = reader["descricao_review"].ToString();
                    btnSubmit.Text = "Update Review";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading existing review: " + ex.Message);
            }
            finally
            {
                cn?.Close();
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (numRating.Value == 0)
            {
                MessageBox.Show("Please select a rating");
                return;
            }

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Check if review already exists
                string checkQuery = "SELECT COUNT(*) FROM projeto.review WHERE id_utilizador = @userId AND id_jogo = @gameId";
                SqlCommand checkCmd = new SqlCommand(checkQuery, cn);
                checkCmd.Parameters.AddWithValue("@userId", currentUserId);
                checkCmd.Parameters.AddWithValue("@gameId", gameId);
                int reviewCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                string query;
                if (reviewCount > 0)
                {
                    // Update existing review
                    query = @"UPDATE projeto.review 
                            SET horas_jogadas = @hours, rating = @rating, 
                                descricao_review = @review, data_review = GETDATE()
                            WHERE id_utilizador = @userId AND id_jogo = @gameId";
                }
                else
                {
                    // Insert new review
                    query = @"INSERT INTO projeto.review 
                            (id_review, horas_jogadas, rating, descricao_review, 
                             data_review, id_utilizador, id_jogo)
                            VALUES 
                            (@id_review, @hours, @rating, @review, GETDATE(), @userId, @gameId)";
                    reviewId = GenerateReviewId();
                }

                SqlCommand command = new SqlCommand(query, cn);
                if (reviewCount == 0)
                {
                    command.Parameters.AddWithValue("@id_review", reviewId);
                }
                command.Parameters.AddWithValue("@hours", numHoursPlayed.Value);
                command.Parameters.AddWithValue("@rating", numRating.Value);
                command.Parameters.AddWithValue("@review", txtReview.Text);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@gameId", gameId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Review submitted successfully!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to submit review");
                }

                // Update game's average rating
                UpdateGameRating();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting review: " + ex.Message);
            }
            finally
            {
                cn?.Close();
            }
        }

        private void UpdateGameRating()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"UPDATE projeto.jogo 
                               SET rating_medio = (
                                   SELECT AVG(rating) 
                                   FROM projeto.review 
                                   WHERE id_jogo = @gameId
                               )
                               WHERE id_jogo = @gameId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@gameId", gameId);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating game rating: " + ex.Message);
            }
            finally
            {
                cn?.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Close();
            GamePage gamePage = new GamePage(currentUserId, gameId);
            gamePage.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
            UserPage userPage = new UserPage(currentUserId, currentUserId);
            userPage.Show();
        }
    }
}