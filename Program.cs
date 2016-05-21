using System;
using System.Collections.Generic;
using Npgsql;
using Dapper;

namespace ConsoleApplication
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
    }
    public class Program
    {
        private const string cs = "Host=localhost;Username=fhe2;Password=four5SIX;Database=frank";

        public static void Main(string[] args)
        {
            IEnumerable<User> users = null;
            
            using (var conn = new NpgsqlConnection(cs))
            {
                Console.WriteLine($"Conncting to {cs}");
                conn.Open();
                Console.WriteLine("q for query, I for inserts, U random new status for all users");
                var command = Console.ReadKey();
                switch (command.KeyChar)
                {
                    case 'I':
                        InsertData(conn);
                        break;
                    case 'U':
                        RandomUpdate(conn);
                        break;
                    default:
                        users = QueryPg(conn);
                        break;
                }
                Print(users);

            }
        }

        private static void RandomUpdate(NpgsqlConnection conn)
        {
            throw new NotImplementedException();
        }

        private static void Print(System.Collections.Generic.IEnumerable<User> users)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Print all users!");
            System.Console.WriteLine("==========================");
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Status}> {user.Id}, {user.First} {user.Last}: {user.Email}");
            }
        }

        private static System.Collections.Generic.IEnumerable<User> QueryPg(NpgsqlConnection conn)
        {
            return conn.Query<User>(@"
                    SELECT id as Id
                           ,email as Email
                           ,first as First
                           ,last as Last
                           ,created as Created
                           ,status as Status FROM membership.users");
        }

        private static void InsertData(NpgsqlConnection conn)
        {
            for (int i = 0; i < 100; i++)
            {                
                conn.Execute(@"
                INSERT INTO membership.users (email, first, last)
                    VALUES (@email, @first, @last);",new { email = $"email{i}@test.com", first=$"first {i}", last = $"last {i}" });
            }
        }
    }
}
