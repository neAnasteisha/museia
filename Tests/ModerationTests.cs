using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using museia.Controllers;
using museia.IService;
using museia.Models;
using Xunit;

namespace museia.Tests
{
    public class ModerationTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IComplaintService> _mockComplaintService;
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<IReactionService> _mockReactionService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _userController;

        public ModerationTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null
            );
            _mockComplaintService = new Mock<IComplaintService>();
            _mockPostService = new Mock<IPostService>();
            _mockReactionService = new Mock<IReactionService>();
            _mockUserService = new Mock<IUserService>();

            _userController = new UserController(null, _mockUserManager.Object, _mockComplaintService.Object,
                _mockPostService.Object, _mockReactionService.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task BlockUser_ShouldCallServicesAndRedirectToComplaints()
        {
            string userId = "user-123";
            uint postId = 10;

            _mockPostService.Setup(ps => ps.GetUserIdByPostIdAsync(postId)).ReturnsAsync(userId);

            var userPosts = new List<Post> { new Post { PostID = 100 } };
            var userReactions = new List<Reaction> { new Reaction { ReactionID = 200 } };
            var userComplaints = new List<Complaint> { new Complaint { ComplaintID = 300 } };

            _mockPostService.Setup(ps => ps.GetPostsOfUserAsync(userId)).ReturnsAsync(userPosts);
            _mockReactionService.Setup(rs => rs.GetReactionsByUserIdAsync(userId)).ReturnsAsync(userReactions);
            _mockComplaintService.Setup(cs => cs.GetComplaintsByUserIdAsync(userId)).ReturnsAsync(userComplaints);
            var result = await _userController.BlockUser(complaintId: 1, postId: postId, postUserId: userId);
            _mockUserService.Verify(us => us.BlockUserAsync(userId), Times.Once);
            foreach (var post in userPosts)
            {
                _mockPostService.Verify(ps => ps.DeletePost(post.PostID), Times.Once);
            }
            foreach (var reaction in userReactions)
            {
                _mockReactionService.Verify(rs => rs.DeleteReactionAsync(reaction.ReactionID), Times.Once);
            }
            foreach (var complaint in userComplaints)
            {
                _mockComplaintService.Verify(cs => cs.DeleteComplaintAsync(complaint.ComplaintID), Times.Once);
            }
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Complaints", redirectResult.ActionName);
            Assert.Equal("Complaint", redirectResult.ControllerName);
        }

        [Fact]
        public async Task SendWarning_ShouldCallServicesAndRedirectToComplaints()
        {
            uint complaintId = 1;
            uint postId = 5;
            string postUserId = "user-456"; 

            var result = await _userController.SendWarning(complaintId, postId, postUserId);

            _mockComplaintService.Verify(cs => cs.AcceptComplaint(complaintId), Times.Once);
            _mockPostService.Verify(ps => ps.MakePostHiddenAsync(postId), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Complaints", redirectResult.ActionName);
            Assert.Equal("Complaint", redirectResult.ControllerName);
        }
    }
}
