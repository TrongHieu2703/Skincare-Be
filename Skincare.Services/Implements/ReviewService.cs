using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
        {
            // Get raw reviews
            var reviewDtos = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            
            // Enhance each review with customer information
            var enhancedReviews = new List<ReviewDto>();
            foreach (var review in reviewDtos)
            {
                var customer = await _reviewRepository.GetCustomerAsync(review.CustomerId);
                // Get OrderItem with Order to access OrderId
                var orderItem = await _reviewRepository.GetOrderItemWithOrderAsync(review.OrderDetailId);
                
                var enhancedReview = new ReviewDto
                {
                    Id = review.Id,
                    OrderDetailId = review.OrderDetailId,
                    OrderId = orderItem?.Order?.Id ?? 0, // Add OrderId from the related Order
                    CustomerId = review.CustomerId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    CustomerName = customer?.Username ?? "Khách hàng" // Add customer name
                };
                
                enhancedReviews.Add(enhancedReview);
            }
            
            // Return reviews sorted by newest first
            return enhancedReviews.OrderByDescending(r => r.CreatedAt);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                throw new NotFoundException($"Review with ID {id} not found.");
            }
            
            // Enhance with customer information
            var customer = await _reviewRepository.GetCustomerAsync(review.CustomerId);
            review.CustomerName = customer?.Username ?? "Khách hàng";
            
            // Get and set OrderId
            var orderItem = await _reviewRepository.GetOrderItemWithOrderAsync(review.OrderDetailId);
            review.OrderId = orderItem?.Order?.Id ?? 0;
            
            return review;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            // Validate OrderItem exists
            if (!await ValidateOrderItemExistsAsync(createReviewDto.OrderDetailId))
            {
                throw new NotFoundException($"OrderDetail with ID {createReviewDto.OrderDetailId} does not exist.");
            }
            
            // Validate order status is completed or delivered
            if (!await ValidateOrderStatusAsync(createReviewDto.OrderDetailId))
            {
                throw new BusinessLogicException("You can only review products from completed or delivered orders.");
            }
            
            // Check for existing review
            if (await CheckExistingReviewAsync(createReviewDto.OrderDetailId, createReviewDto.CustomerId, createReviewDto.ProductId))
            {
                throw new BusinessLogicException("You have already reviewed this product from this order.");
            }
            
            // Validate product exists
            bool productExists = await _reviewRepository.ProductExistsAsync(createReviewDto.ProductId);
            if (!productExists)
            {
                throw new NotFoundException($"Product with ID {createReviewDto.ProductId} does not exist.");
            }
            
            // Validate customer exists
            var customer = await _reviewRepository.GetCustomerAsync(createReviewDto.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID {createReviewDto.CustomerId} does not exist.");
            }

            var reviewDto = await _reviewRepository.CreateReviewAsync(createReviewDto);
            
            // Add customer name to response
            reviewDto.CustomerName = customer.Username;
            
            return reviewDto;
        }

        public async Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
        {
            var updatedReview = await _reviewRepository.UpdateReviewAsync(id, updateReviewDto);
            if (updatedReview == null)
            {
                throw new NotFoundException($"Review with ID {id} not found for update.");
            }
            
            // Add customer name to response
            var customer = await _reviewRepository.GetCustomerAsync(updatedReview.CustomerId);
            updatedReview.CustomerName = customer?.Username ?? "Khách hàng";
            
            return updatedReview;
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                throw new NotFoundException($"Review with ID {id} not found for deletion.");
            }
            await _reviewRepository.DeleteReviewAsync(id);
        }
        
        // Implementation of validation methods
        public async Task<bool> ValidateOrderItemExistsAsync(int orderItemId)
        {
            return await _reviewRepository.OrderItemExistsAsync(orderItemId);
        }
        
        public async Task<bool> ValidateOrderStatusAsync(int orderItemId)
        {
            var orderItem = await _reviewRepository.GetOrderItemWithOrderAsync(orderItemId);
                
            if (orderItem?.Order == null)
            {
                return false;
            }
            
            var status = orderItem.Order.Status;
            return !string.IsNullOrEmpty(status) &&
                   (status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Delivered", StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<bool> CheckExistingReviewAsync(int orderItemId, int customerId, int productId)
        {
            return await _reviewRepository.ReviewExistsAsync(orderItemId, customerId, productId);
        }

        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            
            if (reviews == null || !reviews.Any())
                return 0;
            
            var reviewsWithRating = reviews.Where(r => r.Rating.HasValue);
            
            if (!reviewsWithRating.Any())
                return 0;
            
            return Math.Round(reviewsWithRating.Average(r => r.Rating.Value), 1);
        }

        public async Task<ReviewDto> GetReviewByOrderItemAsync(int orderItemId, int customerId)
        {
            var review = await _reviewRepository.GetReviewByOrderItemAsync(orderItemId, customerId);
            
            if (review == null)
            {
                return null;
            }
            
            // Enhance with customer information
            var customer = await _reviewRepository.GetCustomerAsync(review.CustomerId);
            review.CustomerName = customer?.Username ?? "Khách hàng";
            
            // Get and set OrderId
            var orderItem = await _reviewRepository.GetOrderItemWithOrderAsync(review.OrderDetailId);
            review.OrderId = orderItem?.Order?.Id ?? 0;
            
            return review;
        }
    }
}
