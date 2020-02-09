using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatChallenge.Models
{
    public class Conversation
    {
        public Conversation()
        {
            Status = MessageStatus.Sent;
        }

        public enum MessageStatus
        {
            Sent,
            Delivered
        }

        public int Id { get; set; }
        public int Sender_id { get; set; }
        public int Receiver_id { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime? Created_at { get; set; }
    }
}