using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PagedList.Core;
using VirtoCommerce.Storefront.AutoRestClients.CustomerReviews.WebModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CustomerReviews.WebModuleApi.Models;
using VirtoCommerce.Storefront.Domain.CustomerReview;
using VirtoCommerce.Storefront.Extensions;
using VirtoCommerce.Storefront.Infrastructure;
using VirtoCommerce.Storefront.Model.Caching;
using VirtoCommerce.Storefront.Model.Common.Caching;
using VirtoCommerce.Storefront.Model.CustomerReviews;

namespace VirtoCommerce.Storefront.Domain
{
    public class CustomerReviewService : ICustomerReviewService
    {
        private readonly ICustomerReviews _customerReviewsApi;
        private readonly IProductRatingOperations _productRatingOperations;
        private readonly IStorefrontMemoryCache _memoryCache;
        private readonly IApiChangesWatcher _apiChangesWatcher;
        public CustomerReviewService(ICustomerReviews customerReviewsApi, IProductRatingOperations productRatingOperations, IStorefrontMemoryCache memoryCache, IApiChangesWatcher apiChangesWatcher)
        {
            _customerReviewsApi = customerReviewsApi;
            _productRatingOperations = productRatingOperations;
            _memoryCache = memoryCache;
            _apiChangesWatcher = apiChangesWatcher;
        }

        public IPagedList<Model.CustomerReviews.CustomerReview> SearchReviews(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            return SearchReviewsAsync(criteria).GetAwaiter().GetResult();
        }

        public async Task<IPagedList<Model.CustomerReviews.CustomerReview>> SearchReviewsAsync(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(),nameof(SearchReviewsAsync), criteria.GetCacheKey());
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CustomerReviewCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(_apiChangesWatcher.CreateChangeToken());

                var result = await _customerReviewsApi.SearchCustomerReviewsAsync(criteria.ToSearchCriteriaDto());
                return new StaticPagedList<Model.CustomerReviews.CustomerReview>(result.Results.Select(x => x.ToCustomerReview()),
                                                         criteria.PageNumber, criteria.PageSize, result.TotalCount.Value);
            });
        }

        public double? GetProductRating(string productId)
        {
            return GetProductratingAsync(productId).GetAwaiter().GetResult();
        }

        public async Task<double?> GetProductratingAsync(string productId)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetProductratingAsync), productId);
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CustomerReviewCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(_apiChangesWatcher.CreateChangeToken());

                var result = await _productRatingOperations.GetProductRatingAsync(productId);
                return result.Rating;
            });
        }

        public void CreateReview(string productId, CustomerReviewCreateModel customerReviewCreateModel)
        {
            CreateReviewAsync(productId, customerReviewCreateModel).Wait();
        }

        public async Task CreateReviewAsync(string productId, CustomerReviewCreateModel customerReviewCreateModel)
        {
            var model = customerReviewCreateModel.ToCustomerReviewRequest();
            model.ProductId = productId;
            await _customerReviewsApi.UpdateWithHttpMessagesAsync(new List<CustomerReviewRequest> { model });
        }

        public void UpdateReview(string productId, string customerReviewId, CustomerReviewUpdateModel customerReviewUpdateModel)
        {
            UpdateReviewAsync(productId, customerReviewId, customerReviewUpdateModel).Wait();
        }

        public async Task UpdateReviewAsync(string productId, string customerReviewId, CustomerReviewUpdateModel customerReviewUpdateModel)
        {
            var model = customerReviewUpdateModel.ToCustomerReviewRequest();
            model.Id = customerReviewId;
            model.ProductId = productId;
            await _customerReviewsApi.UpdateWithHttpMessagesAsync(new List<CustomerReviewRequest> { model });
        }

        public void DeleteReview(string productId, string customerReviewId)
        {
            DeleteReviewAsync(productId, customerReviewId).Wait();
        }

        public async Task DeleteReviewAsync(string productId, string customerReviewId)
        {
            await _customerReviewsApi.DeleteAsync(new List<string> { customerReviewId });
        }

        public void CreateReviewAssessment(string productId, string customerReviewId, CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel)
        {
            CreateReviewAssessmentAsync(productId, customerReviewId, customerReviewAssessmentCreateModel).Wait();
        }

        public async Task CreateReviewAssessmentAsync(string productId, string customerReviewId, CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel)
        {
            var model = customerReviewAssessmentCreateModel.ToCustomerReviewAssessmentRequest();

            await _customerReviewsApi.AddAssessmentAsync(customerReviewId, model);
        }
    }
}
