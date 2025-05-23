using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

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

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            string id_utilizador = GenerateUniqueID();
            string hashedPassword = ComputeSha256Hash(password);

            try
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO projeto.utilizador (id_utilizador, nome, email, password) " +
                    "VALUES (@Id, @Nome, @Email, @Password)", cn);

                cmd.Parameters.AddWithValue("@Id", id_utilizador);
                cmd.Parameters.AddWithValue("@Nome", name);
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
                if (ex.Number == 2627) // Unique constraint violation
                {
                    MessageBox.Show("Name already in use.");
                }
                else
                {
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
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
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM projeto.utilizador", cn);
            int count = (int)cmd.ExecuteScalar();
            return "U" + (count + 1).ToString("D3");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        //Hello
        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}