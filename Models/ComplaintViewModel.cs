﻿namespace museia.Models
{
    public class ComplaintViewModel
    {
        public uint ComplaintID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ComplaintReason { get; set; }
        public ComplaintStatus ComplaintStatus { get; set; }
        public string PostText { get; set; }
        public string PostTag { get; set; }
        public string? PostPhoto { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
