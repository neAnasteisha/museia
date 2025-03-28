using museia.Models;
using System.Threading.Tasks;

namespace museia.IService
{
    public interface IComplaintService
    {
        Task CreateComplaintAsync(string complaintReason, string userId, uint postId);
        Task<List<Complaint>> GetComplaintsByPostId(uint postId);
        Task<List<Complaint>> GetComplaintsByUserId(string userId);
        Task<Complaint> GetComplaintByIdAsync(uint id);
        Task UpdateComplaintAsync(Complaint complaint);
        Task DeleteComplaintAsync(uint id);
        Task<List<ComplaintViewModel>> GetAllUnconsideredComplaintsAsync();
        Task<bool> ApproveComplaint(uint id);
        Task<bool> RejectComplaint(uint id);
        Task<bool> AcceptComplaint(uint id);
        Task<int> GetAcceptedComplaintsCountForUser(string userId);

        
    }
}
