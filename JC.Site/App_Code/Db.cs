using System.Configuration;
using System.Data.SqlClient;

namespace JC.Site
{
    public class Db
    {
        public SqlCommand     cmd = new SqlCommand();
        public SqlDataAdapter sda;
        public SqlDataReader  sdr;
        public SqlConnection  con = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ToString());

        public bool IsExist(string query)
        {
            var check = false;

            using (cmd = new SqlCommand(query, con))
            {
                con.Open();
                sdr = cmd.ExecuteReader();

                if (sdr.HasRows)
                    check = true;
            }

            sdr.Close();
            con.Close();
            return check;
        }

        public bool ExecuteQuery(string query)
        {
            int j;

            using (cmd = new SqlCommand(query, con))
            {
                con.Open();
                j = cmd.ExecuteNonQuery();
                con.Close();
            }

            return j > 0;
        }

        public string GetColumnVal(string query, string columnName)
        {
            var retVal = "";

            using (cmd = new SqlCommand(query, con))
            {
                con.Open();
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    retVal = sdr[columnName].ToString();
                    break;
                }

                sdr.Close();
                con.Close();
            }

            return retVal;
        }
    }
}