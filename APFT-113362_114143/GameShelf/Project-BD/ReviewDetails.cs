using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlTypes;

namespace Project_BD
{
    public partial class ReviewDetails : Form
    {
        private string currentUserId;
        private string reviewId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public ReviewDetails(string currentUserId, string reviewId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            this.reviewId = reviewId;
            LoadReviewData();
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

        private void LoadReviewData()
        {
            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Use stored procedure to get review data
                SqlCommand command = new SqlCommand("projeto.sp_GetReviewDetails", cn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@reviewId", reviewId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblGameTitle.Text = reader["GameTitle"].ToString();
                        lblUserName.Text = "By: " + reader["UserName"].ToString();
                        lblRating.Text = "Rating: " + reader["rating"].ToString() + "/10";
                        lblHoursPlayed.Text = "Hours Played: " + reader["HoursPlayed"].ToString();
                        txtReviewText.Text = reader["ReviewText"].ToString();
                        lblReviewDate.Text = "Posted on: " + Convert.ToDateTime(reader["ReviewDate"]).ToString("dd/MM/yyyy");
                    }
                }

                // Load reactions using existing sp
                LoadReviewReactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading review data: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }

        private void LoadReviewReactions()
        {
            try
            {
                using (cn = getSGBDConnection())
                {
                    if (!verifySGBDConnection()) return;

                    // Use stored procedure 
                    using (SqlCommand command = new SqlCommand("projeto.sp_GetReviewReactions", cn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@reviewId", reviewId);

                        listReactions.BeginUpdate();
                        try
                        {
                            listReactions.Items.Clear();
                            listReactions.Columns.Clear();

                            // Set up columns
                            listReactions.View = View.Details;
                            listReactions.Columns.Add("User", 150);
                            listReactions.Columns.Add("Reaction", 250);
                            listReactions.Columns.Add("Date", 100);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ListViewItem item = new ListViewItem(reader["UserName"].ToString());
                                    item.SubItems.Add(reader["ReactionText"].ToString());
                                    item.SubItems.Add(Convert.ToDateTime(reader["ReactionDate"]).ToString("dd/MM/yyyy"));
                                    listReactions.Items.Add(item);
                                }
                            }
                        }
                        finally
                        {
                            listReactions.EndUpdate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reactions: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddReaction_Click(object sender, EventArgs e)
        {
            AddReactionForm reactionForm = new AddReactionForm(currentUserId, reviewId);
            if (reactionForm.ShowDialog() == DialogResult.OK)
            {
                LoadReviewReactions(); // Refresh reactions after adding a new one
            }
        }
        //Delete reviw if the user vistting the page is the own that published the reaction
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult confirmResult = MessageBox.Show("Are you sure you want to delete this review?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Use fn_IsReviewOwner UDF to check ownership
                SqlCommand checkCmd = new SqlCommand(
                    "SELECT projeto.fn_IsReviewOwner(@currentUserId, @reviewId) AS isOwner", cn);
                checkCmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                checkCmd.Parameters.AddWithValue("@reviewId", reviewId);

                bool isOwner = Convert.ToBoolean(checkCmd.ExecuteScalar());

                if (!isOwner)
                {
                    MessageBox.Show("You can only delete your own reviews.");
                    return;
                }

                // Use sp_DeleteReview stored procedure
                SqlCommand deleteCmd = new SqlCommand("projeto.sp_DeleteReview", cn);
                deleteCmd.CommandType = CommandType.StoredProcedure;
                deleteCmd.Parameters.AddWithValue("@reviewId", reviewId);
                deleteCmd.Parameters.AddWithValue("@userId", currentUserId);

                int result = deleteCmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Review deleted successfully.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting review: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }

    }
}