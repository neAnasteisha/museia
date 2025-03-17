namespace museia.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.IRepository;
    using museia.Models;
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _context;
        public ComplaintRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Complaint>> GetComplaintsByPostIdAsync(uint postId)
        {
            return await _context.Complaints
                .Where(c => c.PostID == postId)
                .ToListAsync();
        }
        public async Task<List<Complaint>> GetComplaintsByUserIdAsync(string userId)
        {
            return await _context.Complaints
                .Where(c => c.UserID == userId)
                .ToListAsync();
        }
        public async Task<Complaint> GetComplaintByIdAsync(uint id)
        {
            return await _context.Complaints.FindAsync(id);
        }
        public async Task UpdateComplaintAsync(Complaint complaint)
        {
            var existingComplaint = await _context.Complaints.FindAsync(complaint.ComplaintID);
            if (existingComplaint != null)
            {
                existingComplaint.ComplaintReason = complaint.ComplaintReason;
                existingComplaint.ComplaintStatus = complaint.ComplaintStatus;
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteComplaintAsync(uint id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
        }
    }
}
