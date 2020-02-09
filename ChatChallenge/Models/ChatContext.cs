using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ChatChallenge.Models
{
    public class ChatContext : DbContext
    {
        public ChatContext() : base("MySqlConnection")
        {
            //revisar
            //Database.SetInitializer(new DropCreateDatabaseAlways<ChatContext>());
        }

        public static ChatContext Create()
        {
            return new ChatContext();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
    }
}