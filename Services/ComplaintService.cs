using museia.IRepository;
using museia.Models;
using museia.Repositories;

namespace museia.Services
{
    public class ComplaintService
    {
        private ComplaintRepository _complaintRepository;
        public ComplaintService(ComplaintRepository complaintRepository)
        {
            _complaintRepository = complaintRepository;
        }
        public async Task CreateComplaintAsync(string complaintReason, string userId, uint postId)
        {
            if (string.IsNullOrWhiteSpace(complaintReason))
            {
                throw new ArgumentException("Причина скарги не може бути порожньою.");
            }
            var complaint = new Complaint
            {
                ComplaintReason = complaintReason,
                UserID = userId,
                PostID = postId,
                ComplaintStatus = ComplaintStatus.Accepted
            };
            await _complaintRepository.AddAsync(complaint);
        }
        public async Task<List<Complaint>> GetComplaintsByPostId(uint postId)
        {
            return await _complaintRepository.GetComplaintsByPostIdAsync(postId);
        }
        public async Task<List<Complaint>> GetComplaintsByUserId(string userId)
        {
            return await _complaintRepository.GetComplaintsByUserIdAsync(userId);
        }
        public async Task<Complaint> GetComplaintById(uint id)
        {
            return await _complaintRepository.GetComplaintByIdAsync(id);
        }
        public async Task UpdateComplaint(Complaint complaint)
        {
            await _complaintRepository.UpdateComplaintAsync(complaint);
        }
        public async Task DeleteComplaint(uint id)
        {
            await _complaintRepository.DeleteComplaintAsync(id);
        }

        public async Task<List<Complaint>> GetAllUnconsideredComplaintsAsync()
        {
            return await _complaintRepository.GetAllUnconsideredComplaints();
        }
    }
}
