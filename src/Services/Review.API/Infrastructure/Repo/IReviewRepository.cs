namespace Review.API.Infrastructure.Repo;

public interface IReviewRepository
{
    public Task<ReviewGroup> GetReviewGroup(int listingId);
}