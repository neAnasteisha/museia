using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MockQueryable.Moq;
using museia.Controllers;
using museia.IRepository;
using museia.IService;
using museia.Models;
using Xunit;
using MockQueryable;

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

        [Fact]
        public async Task ApproveComplaint_ShouldChangeStatusToProcessing()
        {
            var mockComplaintService = new Mock<IComplaintService>();
            var complaint = new Complaint { ComplaintID = 1, ComplaintStatus = ComplaintStatus.Sent };
            mockComplaintService.Setup(s => s.GetComplaintByIdAsync(1)).ReturnsAsync(complaint);

            mockComplaintService
                .Setup(s => s.ApproveComplaint(1))
                .ReturnsAsync(true)
                .Callback(() => complaint.ComplaintStatus = ComplaintStatus.Processing);

            bool result = await mockComplaintService.Object.ApproveComplaint(1);

            Assert.True(result); 
            Assert.Equal(ComplaintStatus.Processing, complaint.ComplaintStatus); 
        }

        [Fact]
        public async Task WhenWarningsLessThanTwo_ShouldShowWarningButton()
        {
            var complaints = new List<ComplaintViewModel>
            {
                new ComplaintViewModel { ComplaintID = 1, ComplaintStatus = ComplaintStatus.Processing, UserCountOfWarnings = 1 }
            };

            bool showWarning = complaints.Any(c => c.UserCountOfWarnings < 2);

            Assert.True(showWarning);
        }

        [Fact]
        public async Task WhenWarningsAtLeastTwo_ShouldShowBlockButton()
        {
            var complaints = new List<ComplaintViewModel>
            {
                new ComplaintViewModel { ComplaintID = 1, ComplaintStatus = ComplaintStatus.Processing, UserCountOfWarnings = 2 }
            };

            bool showBlock = complaints.Any(c => c.UserCountOfWarnings >= 2);

            Assert.True(showBlock);
        }

        [Fact]
        public async Task Analytics_ReturnsViewResult_WithCorrectViewModelData()
        {
            var users = new List<User>
            {
                new User { Id = "user1", UserName = "User One" },
                new User { Id = "user2", UserName = "User Two With Complaints" },
                new User { Id = "user3", UserName = "User Three" },
                new User { Id = "user4", UserName = "User Four With Complaints" }
            };


            var topUsers = new List<User>
            {
                users[1],
                users[3] 
            };

            var mockUsersQueryable = users.AsQueryable().BuildMock();
            _mockUserManager.Setup(um => um.Users).Returns(mockUsersQueryable);

            _mockComplaintService.Setup(cs => cs.GetAcceptedComplaintsCountForUser("user1")).ReturnsAsync(0);
            _mockComplaintService.Setup(cs => cs.GetAcceptedComplaintsCountForUser("user2")).ReturnsAsync(5); 
            _mockComplaintService.Setup(cs => cs.GetAcceptedComplaintsCountForUser("user3")).ReturnsAsync(0);
            _mockComplaintService.Setup(cs => cs.GetAcceptedComplaintsCountForUser("user4")).ReturnsAsync(3); 

            _mockComplaintService.Setup(cs => cs.GetTopUsersByComplaintsAsync()).ReturnsAsync(topUsers);

            var result = await _userController.Analytics();

            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsAssignableFrom<AnalyticsViewModel>(viewResult.Model);

            Assert.NotNull(model.FilteredUsers);
            Assert.Equal(2, model.FilteredUsers.Count); 
            Assert.Contains(model.FilteredUsers, u => u.Id == "user2");
            Assert.Contains(model.FilteredUsers, u => u.Id == "user4");
            Assert.DoesNotContain(model.FilteredUsers, u => u.Id == "user1");
            Assert.DoesNotContain(model.FilteredUsers, u => u.Id == "user3");

            var user2FromModel = model.FilteredUsers.First(u => u.Id == "user2");
            var user4FromModel = model.FilteredUsers.First(u => u.Id == "user4");
            Assert.Equal(5, user2FromModel.CountOfWarnings); 
            Assert.Equal(3, user4FromModel.CountOfWarnings); 
            Assert.NotNull(model.TopTen);
            Assert.Equal(topUsers.Count, model.TopTen.Count); 
            Assert.Equal(topUsers, model.TopTen); 

        }
    }
}
