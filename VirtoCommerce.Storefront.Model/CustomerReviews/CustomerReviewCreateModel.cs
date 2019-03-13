namespace VirtoCommerce.Storefront.Model.CustomerReviews
{
    public class CustomerReviewCreateModel
    {
        public string Id { get; set; }

        public string AuthorNickname { get; set; }

        public string Content { get; set; }

        public int Value { get; set; }
    }
}
