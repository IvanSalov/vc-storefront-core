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
using VirtoCommerce.Storefront.Model;
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
        private readonly IWorkContextAccessor _workContextAccessor;
        public CustomerReviewService(ICustomerReviews customerReviewsApi, IProductRatingOperations productRatingOperations, IStorefrontMemoryCache memoryCache, IApiChangesWatcher apiChangesWatcher, IWorkContextAccessor workContextAccessor)
        {
            _customerReviewsApi = customerReviewsApi;
            _productRatingOperations = productRatingOperations;
            _memoryCache = memoryCache;
            _apiChangesWatcher = apiChangesWatcher;
            _workContextAccessor = workContextAccessor;
        }

        public IPagedList<Model.CustomerReviews.CustomerReview> SearchReviews(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            return SearchReviewsAsync(criteria).GetAwaiter().GetResult();
        }

        public async Task<IPagedList<Model.CustomerReviews.CustomerReview>> SearchReviewsAsync(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchReviewsAsync), criteria.GetCacheKey());
            var user = _workContextAccessor.WorkContext.CurrentUser;
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CustomerReviewCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(_apiChangesWatcher.CreateChangeToken());
                cacheEntry.AddExpirationToken(CustomerReviewCacheRegion.CreateCustomerCustomerReviewChangeToken(_workContextAccessor.WorkContext.CurrentUser.Id));

                var result = await _customerReviewsApi.SearchCustomerReviewsAsync(criteria.ToSearchCriteriaDto());
                return new StaticPagedList<Model.CustomerReviews.CustomerReview>(result.Results.Select(x => x.ToCustomerReview(user)),
                                                         criteria.PageNumber, criteria.PageSize, result.TotalCount.Value);
            });
        }

        public double? GetProductRating(string productId)
        {
            return GetProductRatingAsync(productId).GetAwaiter().GetResult();
        }

        public async Task<double?> GetProductRatingAsync(string productId)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetProductRatingAsync), productId);
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
            CustomerReviewCacheRegion.ExpireCustomerCustomerReview(_workContextAccessor.WorkContext.CurrentUser.Id);
            var model = customerReviewCreateModel.ToCustomerReviewRequest();
            model.ProductId = productId;
            await _customerReviewsApi.UpdateAsync(new List<CustomerReviewRequest> { model });
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
            CustomerReviewCacheRegion.ExpireCustomerCustomerReview(_workContextAccessor.WorkContext.CurrentUser.Id);
            await _customerReviewsApi.UpdateWithHttpMessagesAsync(new List<CustomerReviewRequest> { model });
        }

        public void DeleteReview(string productId, string customerReviewId)
        {
            DeleteReviewAsync(productId, customerReviewId).Wait();
        }

        public async Task DeleteReviewAsync(string productId, string customerReviewId)
        {
            CustomerReviewCacheRegion.ExpireCustomerCustomerReview(_workContextAccessor.WorkContext.CurrentUser.Id);
            await _customerReviewsApi.DeleteAsync(new List<string> { customerReviewId });
        }

        public void CreateReviewAssessment(string productId, string customerReviewId, CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel)
        {
            CreateReviewAssessmentAsync(productId, customerReviewId, customerReviewAssessmentCreateModel).Wait();
        }

        public async Task CreateReviewAssessmentAsync(string productId, string customerReviewId, CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel)
        {
            var model = customerReviewAssessmentCreateModel.ToCustomerReviewAssessmentRequest();
            CustomerReviewCacheRegion.ExpireCustomerCustomerReview(_workContextAccessor.WorkContext.CurrentUser.Id);
            await _customerReviewsApi.AddAssessmentAsync(customerReviewId, model);
        }
    }
}
