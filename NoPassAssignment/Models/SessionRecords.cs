using System;
using System.ComponentModel.DataAnnotations;

namespace NoPassAssignment.Models
{
    public class SessionRecords
    {
        [Key]
        public String SessionId { get; set; }
        public String UserId { get; set; }
        public int SessionExpired { get; set; }
    }
}