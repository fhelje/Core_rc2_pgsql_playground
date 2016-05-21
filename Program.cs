using System;
using System.Collections.Generic;
using Npgsql;
using Dapper;
using System.Diagnostics;
using System.Linq;

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
        private static Random random = new Random();

        public static void Main(string[] args)
        {
            IEnumerable<User> users = null;
            var quit = false;            
            using (var conn = new NpgsqlConnection(cs))
            {
                Console.WriteLine($"Conncting to {cs}");

                conn.Open();

                while(!quit) {
                    Console.WriteLine("q for query, I for inserts, U random new status for all users");
                    var command = Console.ReadKey();
                    quit = ExecuteCommand(users, conn, command);
                }
            }
        }

        private static bool ExecuteCommand(IEnumerable<User> users, NpgsqlConnection conn, ConsoleKeyInfo command)
        {
            var retval = false;
            var sw = new Stopwatch();
            sw.Start();
            switch (command.KeyChar)
            {
                case 'I':
                    InsertData(conn);
                    break;
                case 'U':
                    RandomUpdate(conn);
                    break;
                case 'c':
                    retval = true;
                    break;
                default:
                    users = QueryPg(conn);
                    Print(users);
                    break;
            }
            sw.Stop();
            System.Console.WriteLine($"Elapsed time: {sw.Elapsed.TotalMilliseconds} ms");
            return retval;
        }

        private static void RandomUpdate(NpgsqlConnection conn)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Random change to all users status: pending, verified, closed!");
            System.Console.WriteLine("==========================");
            var users = QueryPg(conn);
            foreach (var user in users)
            {
                var status = GetStatus();
                conn.Execute("UPDATE membership.users SET status = @status WHERE id = @id;", new { id = user.Id, status});
            }
            users = QueryPg(conn);
            Print(users);
            
        }

        private static string GetStatus()
        {
            var rn = random.Next(0, 3);
            switch (rn)
            {
                case 0:
                    return "pending";
                case 1:
                    return "verified";
                case 2:
                    return "closed";
                default:
                    return "unknown";
            }
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
            var max = conn.Query<int>("SELECT max(id) FROM membership.users;").First();
            
            for (int i = 0; i < 100; i++)
            {                
                var c = max + i;
                conn.Execute(@"
                INSERT INTO membership.users (email, first, last)
                    VALUES (@email, @first, @last);",new { email = $"email{c}@test.com", first=$"first {c}", last = $"last {c}" });
            }
        }
    }
}
