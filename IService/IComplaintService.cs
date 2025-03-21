using museia.Models;
using System.Threading.Tasks;

namespace museia.IService
{
    public interface IComplaintService
    {
        Task<List<Complaint>> GetAllComplaintsAsync();
        Task AddAsync(Complaint complaint);
        Task<Complaint> GetComplaintByIdAsync(uint id);
        Task UpdateComplaintAsync(Complaint complaint);
        Task DeleteComplaintAsync(uint id);
        Task<List<Complaint>> GetComplaintsByStatusAsync(ComplaintStatus status);
        Task<List<Complaint>> GetAllUnconsideredComplaintsAsync();
        Task<bool> ApproveComplaint(uint id);
        Task<bool> RejectComplaint(uint id);
    }
}
