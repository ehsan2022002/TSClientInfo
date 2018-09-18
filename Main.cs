using System;
using System.Collections.Generic;
using System.Text;
using TSClientInfo;


namespace TerminalServices
{
    class TSClientIPClass
    {
        [STAThread]
        static void Main(string[] args)
        {
            string srvName = string.Empty;
            Console.WriteLine( "current user is: " +Environment.UserDomainName + "\\" +  Environment.UserName );
 
            Console.WriteLine("Enter server name:[ex localhost] ");
            try
            {
                srvName = Console.ReadLine();                
                var users = UserLogins.GetUsers(srvName);

                foreach (var user in users)
                {
                    Console.WriteLine("User: " + user + "  " + user.SessionID + "  " + user.ClientIP + "  " + user.ClientName   );
                }

                Console.ReadKey();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace ); 
            }
            Console.ReadLine();

        }
    }
}
