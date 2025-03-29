using museia.IRepository;
using museia.IService;
using museia.Models;
using museia.Repositories;

namespace museia.Services
{
    public class ComplaintService: IComplaintService
    {
        private IComplaintRepository _complaintRepository;
        private IPostRepository _postRepository;
        private IUserRepository _userRepository;

        public ComplaintService(IComplaintRepository complaintRepository, IPostRepository postRepository, IUserRepository userRepository)
        {
            _complaintRepository = complaintRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
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
                ComplaintStatus = ComplaintStatus.Sent
            };
            await _complaintRepository.AddAsync(complaint);
        }
        public async Task<List<Complaint>> GetComplaintsByPostIdAsync(uint postId)
        {
            return await _complaintRepository.GetComplaintsByPostIdAsync(postId);
        }
        public async Task<List<Complaint>> GetComplaintsByUserIdAsync(string userId)
        {
            return await _complaintRepository.GetComplaintsByUserIdAsync(userId);
        }
        public async Task<Complaint> GetComplaintByIdAsync(uint id)
        {
            return await _complaintRepository.GetComplaintByIdAsync(id);
        }
        public async Task UpdateComplaintAsync(Complaint complaint)
        {
            await _complaintRepository.UpdateComplaintAsync(complaint);
        }
        public async Task DeleteComplaintAsync(uint id)
        {
            await _complaintRepository.DeleteComplaintAsync(id);
        }

        public async Task<List<ComplaintViewModel>> GetAllUnconsideredComplaintsAsync()
        {
            var complaints = await _complaintRepository.GetAllUnconsideredComplaints();

            var complaintViewModels = await Task.WhenAll(complaints.Select(async c =>
            {
                var postUserId = await _postRepository.GetUserIdByPostIdAsync(c.Post.PostID);
                var userWarnings = await _complaintRepository.GetAcceptedComplaintsCountForUser(postUserId);

                return new ComplaintViewModel
                {
                    ComplaintID = c.ComplaintID,
                    UserId = c.User.Id,
                    UserName = c.User.UserName,
                    ComplaintReason = c.ComplaintReason,
                    ComplaintStatus = c.ComplaintStatus,
                    PostId = c.Post.PostID,
                    PostText = c.Post.PostText,
                    PostTag = c.Post.PostTag.ToString(),
                    PostPhoto = c.Post.PostPhoto,
                    CreatedAt = c.Post.CreatedAt,
                    PostsUserId = postUserId,
                    UserCountOfWarnings = userWarnings
                };
            }));

            return complaintViewModels.ToList();
        }


        public async Task<bool> ApproveComplaint(uint id)
        {
            var complaint = await _complaintRepository.GetComplaintByIdAsync(id);
            if (complaint == null)
                return false;

            await _complaintRepository.ApproveComplaint(id);

            return true;
        }

        public async Task<bool> RejectComplaint(uint id)
        {
            var complaint = await _complaintRepository.GetComplaintByIdAsync(id);
            if (complaint == null)
                return false;

            await _complaintRepository.RejectComplaint(id);
            return true;
        }

        public async Task<int> GetAcceptedComplaintsCountForUser(string userId)
        {
            return await _complaintRepository.GetAcceptedComplaintsCountForUser(userId);
        }

        public async Task<bool> AcceptComplaint(uint id)
        {
            var complaint = await _complaintRepository.GetComplaintByIdAsync(id);
            if (complaint == null)
                return false;

            await _complaintRepository.AcceptComplaint(id);
            return true;
        }


    }
}
