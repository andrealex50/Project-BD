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

        private string GetNewReviewId()
        {
            try
            {
                using (var tempCn = getSGBDConnection())
                {
                    tempCn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT projeto.fn_GenerateReviewId()", tempCn);
                    return cmd.ExecuteScalar().ToString();
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

                // Use the stored procedure
                SqlCommand command = new SqlCommand("projeto.sp_GetGameDetails", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@gameId", gameId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    lblGameTitle.Text = reader["titulo"].ToString();
                }
                reader.Close();
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

                // Use the stored procedure
                SqlCommand command = new SqlCommand("projeto.sp_CheckUserGameReview", cn);
                command.CommandType = CommandType.StoredProcedure;
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

        private bool CheckReviewExists()
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM projeto.review WHERE id_utilizador = @userId AND id_jogo = @gameId", cn))
            {
                cmd.Parameters.AddWithValue("@userId", currentUserId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }


        private void UpdateExistingReview()
        {
            // First get the existing review ID
            string existingReviewId = GetExistingReviewId();

            if (string.IsNullOrEmpty(existingReviewId))
            {
                MessageBox.Show("Could not find existing review to update");
                return;
            }

            using (SqlCommand cmd = new SqlCommand("projeto.sp_UpdateReview", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@reviewId", existingReviewId); // Add this line
                cmd.Parameters.AddWithValue("@hours", numHoursPlayed.Value);
                cmd.Parameters.AddWithValue("@rating", numRating.Value);
                cmd.Parameters.AddWithValue("@review", txtReview.Text);
                cmd.Parameters.AddWithValue("@userId", currentUserId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetExistingReviewId()
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id_review FROM projeto.review WHERE id_utilizador = @userId AND id_jogo = @gameId", cn))
            {
                cmd.Parameters.AddWithValue("@userId", currentUserId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                object result = cmd.ExecuteScalar();
                return result?.ToString();
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
                using (cn = getSGBDConnection())
                {
                    if (!verifySGBDConnection())
                        return;

                    // Check if review exists
                    bool reviewExists = CheckReviewExists();

                    if (reviewExists)
                    {
                        UpdateExistingReview();
                    }
                    else
                    {
                        CreateNewReview();
                    }

                    MessageBox.Show("Review submitted successfully!");
                    this.Close();
                    GamePage gamePage = new GamePage(currentUserId, gameId);
                    gamePage.Show();
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private void CreateNewReview()
        {
            reviewId = GetNewReviewId();

            using (SqlCommand cmd = new SqlCommand("projeto.sp_CreateReview", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@reviewId", reviewId);
                cmd.Parameters.AddWithValue("@hours", numHoursPlayed.Value);
                cmd.Parameters.AddWithValue("@rating", numRating.Value);
                cmd.Parameters.AddWithValue("@review", txtReview.Text);
                cmd.Parameters.AddWithValue("@userId", currentUserId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.ExecuteNonQuery();
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            GamePage gamePage = new GamePage(currentUserId, gameId);
            gamePage.Show();
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