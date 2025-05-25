using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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

                string query = @"SELECT j.titulo AS GameTitle, u.nome AS UserName, 
                               r.rating, r.horas_jogadas AS HoursPlayed, 
                               r.descricao_review AS ReviewText, r.data_review AS ReviewDate
                               FROM projeto.review r
                               JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
                               JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
                               WHERE r.id_review = @reviewId";

                SqlCommand command = new SqlCommand(query, cn);
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

                // Load reactions to this review
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
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string query = @"SELECT u.nome AS UserName, r.reacao_texto AS ReactionText
                               FROM projeto.reage_a r
                               JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
                               WHERE r.id_review = @reviewId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@reviewId", reviewId);

                listReactions.Items.Clear();
                listReactions.View = View.Details;
                listReactions.Columns.Clear();
                listReactions.Columns.Add("User", 150);
                listReactions.Columns.Add("Reaction", 300);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["UserName"].ToString());
                    item.SubItems.Add(reader["ReactionText"].ToString());
                    listReactions.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reactions: " + ex.Message);
            }
            finally
            {
                cn.Close();
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

                // First, check if the current user is the author of the review
                string checkAuthorQuery = @"SELECT id_utilizador, id_jogo FROM projeto.review WHERE id_review = @reviewId";
                SqlCommand checkCmd = new SqlCommand(checkAuthorQuery, cn);
                checkCmd.Parameters.AddWithValue("@reviewId", reviewId);
                SqlDataReader reader = checkCmd.ExecuteReader();

                string authorId = null;
                string gameId = null;

                if (reader.Read())
                {
                    authorId = reader["id_utilizador"].ToString();
                    gameId = reader["id_jogo"].ToString();
                }
                reader.Close();

                if (authorId == null || authorId != currentUserId)
                {
                    MessageBox.Show("You can only delete your own reviews.");
                    return;
                }

                // Begin delete transaction
                SqlTransaction transaction = cn.BeginTransaction();

                try
                {
                    // Delete reactions first
                    SqlCommand deleteReactions = new SqlCommand(@"DELETE FROM projeto.reage_a WHERE id_review = @reviewId", cn, transaction);
                    deleteReactions.Parameters.AddWithValue("@reviewId", reviewId);
                    deleteReactions.ExecuteNonQuery();

                    // Delete the review itself
                    SqlCommand deleteReview = new SqlCommand(@"DELETE FROM projeto.review WHERE id_review = @reviewId", cn, transaction);
                    deleteReview.Parameters.AddWithValue("@reviewId", reviewId);
                    deleteReview.ExecuteNonQuery();

                    // Update the game's average rating
                    SqlCommand updateRating = new SqlCommand(@"
                UPDATE projeto.jogo
                SET rating_medio = (
                    SELECT ISNULL(AVG(CAST(rating AS FLOAT)), 0)
                    FROM projeto.review
                    WHERE id_jogo = @gameId
                )
                WHERE id_jogo = @gameId", cn, transaction);
                    updateRating.Parameters.AddWithValue("@gameId", gameId);
                    updateRating.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Review and its reactions deleted successfully.");
                    this.Close(); // Close the form
                }
                catch (Exception innerEx)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error during deletion: " + innerEx.Message);
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