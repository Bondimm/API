using Microsoft.AspNetCore.Mvc;


namespace CryptoBot.Models.Entities
{
    public class Commands : Controller
    {
        public bool Exist { get; set; }
        public int Index { get; set; }
        public void DBCheck(Commands checks, UserList user, string id)
        {
            if (user == null)
            {
                Response.StatusCode = 404;
            }
            checks.Exist = false;
            checks.Index = 0;
            for (int i = 0; i < user.users.Count; i++)
            {
                if (user.users[i].Id == id)
                {
                    checks.Index = i;
                    checks.Exist = true;
                    break;
                }
            }
        }
    }
}
