using System;
using MySqlConnector;

namespace hotel
{
    internal class dbconnect
    {
        public static MySqlConnection conndb()
        {
            string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
            MySqlConnection conn = new MySqlConnection(connectstring);
            return conn; 
        }
    }
}