using System;
using System.Collections.Generic;
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

                string query = @"SELECT horas_jogadas, rating, descricao_review 
                               FROM projeto.review 
                               WHERE id_utilizador = @userId AND id_jogo = @gameId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@gameId", gameId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
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
                            (NEWID(), @hours, @rating, @review, GETDATE(), @userId, @gameId)";
                }

                SqlCommand command = new SqlCommand(query, cn);
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
        // Go back to MainPage.cs
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Close();
            MainPage mainPageForm = new MainPage(currentUserId);
            mainPageForm.Show();
        }
        // Go to Own UserPage.cs
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
            UserPage userPageForm = new UserPage(currentUserId, currentUserId);
            userPageForm.Show();
        }
        // Gera algo tipo "J001", "J002"
        private string GenerateUniqueID()
        {
            try
            {
                // Count entries for this specific list to avoid conflicts
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM projeto.jogo WHERE id_jogo = @gameId",
                    cn);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                int count = (int)cmd.ExecuteScalar();
                return "J" + (count + 1).ToString("D3"); // Example: "E001"
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating ID: " + ex.Message);
                return "E" + DateTime.Now.Ticks.ToString().Substring(0, 10); // Fallback with timestamp
            }
        }
    }

}