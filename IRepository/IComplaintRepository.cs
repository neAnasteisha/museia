using museia.Models;

namespace museia.IRepository
{
    public interface IComplaintRepository
    {
        Task AddAsync(Complaint complaint);
        Task<List<Complaint>> GetComplaintsByPostIdAsync(uint postId);
        Task<List<Complaint>> GetComplaintsByUserIdAsync(string userId);
        Task<Complaint> GetComplaintByIdAsync(uint id);
        Task UpdateComplaintAsync(Complaint complaint);
        Task DeleteComplaintAsync(uint id);
        Task<List<Complaint>> GetAllUnconsideredComplaints();
        Task ApproveComplaint(uint id);
        Task RejectComplaint(uint id);
        Task<int> GetAcceptedComplaintsCountForUser(string userId);
    }
}
