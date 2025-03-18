﻿using museia.IRepository;
using museia.IService;
using museia.Models;
using museia.Repositories;

namespace museia.Services
{
    public class ReactionService : IReactionService
    {
        private readonly IReactionRepository _reactionRepository;

        public ReactionService(IReactionRepository reactionRepository)
        {
            _reactionRepository = reactionRepository;
        }

        public async Task AddReactionAsync(Emoji reactionType, string userId, uint postId)
        {
            var reaction = new Reaction
            {
                ReactionType = reactionType,
                UserID = userId,
                PostID = postId
            };
            await _reactionRepository.AddOrUpdateReactionAsync(reaction);
        }
    }

}
