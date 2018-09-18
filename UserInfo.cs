using System;
using System.Collections.Generic;
using System.Text;

namespace TSClientInfo
{
    public class UserInfo
    {
        public string Domain { get; set; }
        public string User { get; set; }

        public int SessionID { get; set; }
        public string ClientIP { get; set; }
        public string ClientName { get; set; }


        public override string ToString()
        {
            return string.Format("{0}\\{1}: {2}", Domain, User, SessionID);
        }
    }
}
