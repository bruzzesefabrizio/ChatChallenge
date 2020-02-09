using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatChallenge.Models
{
    public class User
    {
        public User()
        {
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Created_at { get; set; }
    }
}