using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public class ReviewService : EntityService<Review, ReviewFilter>
    {
        public ReviewService(IServiceProvider services) : base(services)
        {
        }

        public override Task CreateAsync(Review review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            review.CreatedOn = DateTimeOffset.UtcNow;

            return base.CreateAsync(review);
        }

        public override IQueryable<Review> GetQuery(ReviewFilter filter = null)
        {
            var query = _unitOfWork.Query<Review>();
            query = query.Include(x => x.Customer).ThenInclude(x => x.UserImage);

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.ProductId != null)
                {
                    query = query.Where(x => x.ProductId == filter.ProductId);
                }

                if (filter.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == filter.CustomerId);
                }

                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.Product != null && x.Product.SellerId == filter.SellerId);
                }

                if (filter.Rating != null)
                {
                    query = query.Where(x => x.Rating == filter.Rating);
                }

                if (filter.Approved != null)
                {
                    query = query.Where(x => x.Approved == filter.Approved);
                }

                if (filter.ReviewId != null)
                {
                    query = query.Where(x => x.Id == filter.ReviewId);
                }

                if (filter.Title != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Title) && EF.Functions.Like(x.Title, $"%{filter.Title}%"));
                }
            }

            return query;
        }

        public async Task<ReviewEvaluation> EvaluateAsync(ReviewFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));


            async Task<ReviewEvaluation> acquire()
            {
                var query = GetQuery(filter);

                var evaluation = new ReviewEvaluation
                {
                    TotalCount = await query.CountAsync(),
                    TotalRating1 = await query.CountAsync(x => x.Rating == 1),
                    TotalRating2 = await query.CountAsync(x => x.Rating == 2),
                    TotalRating3 = await query.CountAsync(x => x.Rating == 3),
                    TotalRating4 = await query.CountAsync(x => x.Rating == 4),
                    TotalRating5 = await query.CountAsync(x => x.Rating == 5)
                };

                evaluation.AverageRating = CalculateRatingAverage(
                    evaluation.TotalCount,
                    evaluation.TotalRating1,
                    evaluation.TotalRating2,
                    evaluation.TotalRating3,
                    evaluation.TotalRating4,
                    evaluation.TotalRating5);

                evaluation.PercentRating1 = CalculateRatingPercent(evaluation.TotalCount, evaluation.TotalRating1);
                evaluation.PercentRating2 = CalculateRatingPercent(evaluation.TotalCount, evaluation.TotalRating2);
                evaluation.PercentRating3 = CalculateRatingPercent(evaluation.TotalCount, evaluation.TotalRating3);
                evaluation.PercentRating4 = CalculateRatingPercent(evaluation.TotalCount, evaluation.TotalRating4);
                evaluation.PercentRating5 = CalculateRatingPercent(evaluation.TotalCount, evaluation.TotalRating5);

                return evaluation;
            }

            string key = await _cacheManager.ComposeKeyAsync($"{EntityHelper.GetCachePrefix<Review>()}.{nameof(EvaluateAsync)}", filter);
            var result = await _cacheManager.GetAsync(key, acquire);
            return result;
        }

        private double CalculateRatingAverage(int totalReviews, int totalRating1, int totalRating2, int totalRating3, int totalRating4, int totalRating5)
        {
            double averageRating = Math.Round(totalReviews > 0 ? ((1 * totalRating1) + (2 * totalRating2) + (3 * totalRating3) + (4 * totalRating4) + (5 * totalRating5)) / (double)totalReviews : 0, 1, MidpointRounding.AwayFromZero);
            return averageRating;
        }

        private decimal CalculateRatingPercent(int totalReviews, int totalRating)
        {
            return totalReviews > 0 ? Math.Round(((decimal)totalRating / totalReviews * 100), MidpointRounding.AwayFromZero) : 0;
        }
    }

    public class ReviewEvaluation : CountEvaluation
    {
        public int TotalRating1 { get; set; }

        public int TotalRating2 { get; set; }

        public int TotalRating3 { get; set; }

        public int TotalRating4 { get; set; }

        public int TotalRating5 { get; set; }

        public int[] TotalRatings => new[] {
            TotalRating1,
            TotalRating2,
            TotalRating3,
            TotalRating4,
            TotalRating5
        };

        public decimal[] PercentRatings => new[] {
            PercentRating1,
            PercentRating2,
            PercentRating3,
            PercentRating4,
            PercentRating5
        };

        public double AverageRating { get; set; }

        public decimal PercentRating1 { get; set; }

        public decimal PercentRating2 { get; set; }

        public decimal PercentRating3 { get; set; }

        public decimal PercentRating4 { get; set; }

        public decimal PercentRating5 { get; set; }
    }
}
