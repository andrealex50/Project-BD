using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class AddReactionForm : Form
    {
        private string userId;
        private string reviewId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public AddReactionForm(string userId, string reviewId)
        {
            InitializeComponent();
            this.userId = userId;
            this.reviewId = reviewId;
            LoadReactionIfExists();
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private void LoadReactionIfExists()
        {
            try
            {
                cn = getSGBDConnection();
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                string query = @"SELECT reacao_texto FROM projeto.reage_a
                                 WHERE id_utilizador = @userId AND id_review = @reviewId";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@reviewId", reviewId);

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    txtReaction.Text = result.ToString();
                    btnSubmit.Text = "Update";
                }
                else
                {
                    btnSubmit.Text = "Add";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reaction: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReaction.Text))
            {
                MessageBox.Show("Please enter your reaction");
                return;
            }

            try
            {
                cn = getSGBDConnection();
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                // Check if reaction already exists
                string checkQuery = @"SELECT COUNT(*) FROM projeto.reage_a 
                                      WHERE id_utilizador = @userId AND id_review = @reviewId";

                SqlCommand checkCommand = new SqlCommand(checkQuery, cn);
                checkCommand.Parameters.AddWithValue("@userId", userId);
                checkCommand.Parameters.AddWithValue("@reviewId", reviewId);

                int reactionExists = (int)checkCommand.ExecuteScalar();

                string query;

                if (reactionExists > 0)
                {
                    // Update existing reaction
                    query = @"UPDATE projeto.reage_a 
                              SET reacao_texto = @reaction, reacao_data = GETDATE()
                              WHERE id_utilizador = @userId AND id_review = @reviewId";
                }
                else
                {
                    // Insert new reaction
                    query = @"INSERT INTO projeto.reage_a 
                              (id_utilizador, id_review, reacao_texto, reacao_data)
                              VALUES (@userId, @reviewId, @reaction, GETDATE())";
                }

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@reviewId", reviewId);
                command.Parameters.AddWithValue("@reaction", txtReaction.Text);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Reaction saved successfully!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving reaction: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
