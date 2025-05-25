using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class RegisterForm : Form
    {
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public RegisterForm()
        {
            InitializeComponent();
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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!verifySGBDConnection())
                return;

            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (name == "adminmod")
            {
                MessageBox.Show("That username is not available");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            string id_utilizador = GenerateUniqueID();
            string hashedPassword = ComputeSha256Hash(password);

            try
            {
                // Check if username or email already exists using a stored procedure
                SqlCommand checkCmd = new SqlCommand("projeto.sp_CheckUserExists", cn);
                checkCmd.CommandType = CommandType.StoredProcedure;
                checkCmd.Parameters.AddWithValue("@Name", name);
                checkCmd.Parameters.AddWithValue("@Email", email);

                SqlParameter existsParam = new SqlParameter("@Exists", SqlDbType.Bit);
                existsParam.Direction = ParameterDirection.Output;
                checkCmd.Parameters.Add(existsParam);

                checkCmd.ExecuteNonQuery();

                if ((bool)existsParam.Value)
                {
                    MessageBox.Show("Username or email already in use.");
                    return;
                }

                // Use stored procedure for registration
                SqlCommand cmd = new SqlCommand("projeto.sp_RegisterUser", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id_utilizador);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    MessageBox.Show("Account created successfully!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error creating account.");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                return builder.ToString();
            }
        }

        private string GenerateUniqueID()
        {
            
            SqlCommand cmd = new SqlCommand("SELECT projeto.fn_GenerateUserId()", cn);
            return cmd.ExecuteScalar().ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}