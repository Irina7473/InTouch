using System;
using DataBaseActions;

namespace testDB
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new DBConnection();
            var user=db.FindUserName(3);
            Console.WriteLine(user);
        }
    }
}
