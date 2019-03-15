using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Storefront.Infrastructure;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.CustomerReviews;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [StorefrontApiRoute]
    public class ApiCustomerReviewsController : StorefrontControllerBase
    {
        private readonly ICustomerReviewService _customerReviewService;

        public ApiCustomerReviewsController(IWorkContextAccessor workContextAccessor, IStorefrontUrlBuilder urlBuilder, ICustomerReviewService customerReviewService)
            : base(workContextAccessor, urlBuilder)
        {
            _customerReviewService = customerReviewService;
        }

        [HttpPost("product/{productId}/customerReviews")]
        public ActionResult SaveCustomerReview([FromRoute] string productId, [FromBody] CustomerReviewCreateModel customerReviewCreateModel)
        {
            _customerReviewService.CreateReview(productId, customerReviewCreateModel);

            return NoContent();
        }

        [HttpPut("product/{productId}/customerReviews/{customerReviewId}")]
        public ActionResult EditCustomerReview(string productId, string customerReviewId, [FromBody] CustomerReviewUpdateModel customerReviewUpdateMoodel)
        {
            _customerReviewService.UpdateReview(productId, customerReviewId, customerReviewUpdateMoodel);

            return NoContent();
        }

        [HttpDelete("product/{productId}/customerReviews/{customerReviewId}")]
        public ActionResult DeleteCustomerReview(string productId, string customerReviewId)
        {
            _customerReviewService.DeleteReview(productId, customerReviewId);

            return NoContent();
        }

        [HttpPost("product/{productId}/customerReviews/{customerReviewId}/assessments")]
        public ActionResult SaveCustomerReviewAssessment(string productId, string customerReviewId, [FromBody] CustomerReviewAssessmentCreateModel customerReviewAssessmentCreateModel)
        {
            _customerReviewService.CreateReviewAssessment(productId, customerReviewId, customerReviewAssessmentCreateModel);

            return NoContent();
        }
    }
}
