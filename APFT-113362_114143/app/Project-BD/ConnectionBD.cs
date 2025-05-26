using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_BD
{
    public class ConnectionBD
    {
        private string user = "p2g3";
        private string password = "tiagoandre@BD2004";

        public SqlConnection getSGBDConnection()
        {
            return new SqlConnection("Data Source=tcp:mednat.ieeta.pt\\SQLSERVER,8101;User ID=" + this.user + ";Password=" + this.password);
        }
    }
}
