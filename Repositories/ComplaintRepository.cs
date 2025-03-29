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

        public async Task<List<Complaint>> GetAllUnconsideredComplaints()
        {
            return await _context.Complaints
                .Where(c => c.ComplaintStatus == ComplaintStatus.Sent || c.ComplaintStatus == ComplaintStatus.Processing)
                .Include(c => c.User)
                .Include(c => c.Post)
                .ToListAsync();
        }

        public async Task ApproveComplaint(uint id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.ComplaintStatus = ComplaintStatus.Processing;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectComplaint(uint id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.ComplaintStatus = ComplaintStatus.Declined;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetAcceptedComplaintsCountForUser(string userId)
        {
            int count = await _context.Complaints
                .Where(c => c.Post.UserID == userId && c.ComplaintStatus == ComplaintStatus.Accepted)
                .Select(c => c.UserID)
                .Distinct()
                .CountAsync();
            return count;

        }

        public async Task AcceptComplaint(uint id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.ComplaintStatus = ComplaintStatus.Accepted;
                await _context.SaveChangesAsync();
            }
        }

    }
}
