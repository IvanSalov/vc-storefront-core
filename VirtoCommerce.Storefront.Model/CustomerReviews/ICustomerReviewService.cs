using System.Threading.Tasks;
using PagedList.Core;

namespace VirtoCommerce.Storefront.Model.CustomerReviews
{
    public interface ICustomerReviewService
    {
        IPagedList<CustomerReview> SearchReviews(CustomerReviewSearchCriteria criteria);
        Task<IPagedList<CustomerReview>> SearchReviewsAsync(CustomerReviewSearchCriteria criteria);

        double? GetProductRating(string productId);
        Task<double?> GetProductRatingAsync(string productId);

        void CreateReview(string productId, CustomerReviewCreateModel customerReviewCreateModel);
        Task CreateReviewAsync(string productId, CustomerReviewCreateModel customerReviewCreateModel);

        void UpdateReview(string productId, string customerReviewId, CustomerReviewUpdateModel customerReviewUpdateModel);
        Task UpdateReviewAsync(string productId, string customerReviewId, CustomerReviewUpdateModel customerReviewUpdateModel);

        void DeleteReview(string productId, string customerReviewId);
        Task DeleteReviewAsync(string productId, string customerReviewId);

        void CreateReviewAssessment(string productId, string customerReviewId, CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel);

    }
}
