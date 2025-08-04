using FluentValidation;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System.Linq;

namespace Skincare.Services.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        private readonly IProductTypeService _productTypeService;
        private readonly IBranchService _branchService;

        public CreateProductDtoValidator(IProductTypeService productTypeService, IBranchService branchService)
        {
            _productTypeService = productTypeService;
            _branchService = branchService;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 100).WithMessage("Product name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_.,()]+$").WithMessage("Product name contains invalid characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .Length(10, 1000).WithMessage("Product description must be between 10 and 1000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Product price must be greater than 0")
                .PrecisionScale(10, 2, false).WithMessage("Price must have maximum 2 decimal places");

            RuleFor(x => x.ProductTypeId)
                .GreaterThan(0).WithMessage("Invalid product type ID")
                .MustAsync(async (productTypeId, cancellation) =>
                {
                    var productType = await _productTypeService.GetProductTypeByIdAsync(productTypeId);
                    return productType != null;
                }).WithMessage("Product type does not exist");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Invalid branch ID")
                .MustAsync(async (branchId, cancellation) =>
                {
                    var branch = await _branchService.GetBranchByIdAsync(branchId);
                    return branch != null;
                }).WithMessage("Branch does not exist");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Invalid image URL format");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be active, inactive, or discontinued");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue)
                .WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.Size)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Size))
                .WithMessage("Size cannot exceed 50 characters");

            RuleFor(x => x.Brand)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Brand))
                .WithMessage("Brand name cannot exceed 100 characters");

            RuleFor(x => x.Ingredients)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Ingredients))
                .WithMessage("Ingredients cannot exceed 500 characters");

            RuleFor(x => x.UsageInstructions)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.UsageInstructions))
                .WithMessage("Usage instructions cannot exceed 200 characters");

            RuleFor(x => x.SkinTypeRecommendation)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.SkinTypeRecommendation))
                .WithMessage("Skin type recommendation cannot exceed 100 characters");

            // Business rules
            RuleFor(x => x)
                .MustAsync(async (product, cancellation) =>
                {
                    if (product.IsOrganic == true && !string.IsNullOrEmpty(product.Ingredients))
                    {
                        // Check if ingredients contain non-organic indicators
                        var nonOrganicIndicators = new[] { "paraben", "sulfate", "phthalate", "formaldehyde" };
                        return !nonOrganicIndicators.Any(indicator => 
                            product.Ingredients.ToLower().Contains(indicator));
                    }
                    return true;
                }).WithMessage("Organic products cannot contain non-organic ingredients");

            RuleFor(x => x)
                .MustAsync(async (product, cancellation) =>
                {
                    if (product.Price > 1000 && product.StockQuantity.HasValue && product.StockQuantity.Value > 100)
                    {
                        return false; // High-value products should have limited stock
                    }
                    return true;
                }).WithMessage("High-value products should have limited stock for security");
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidStatus(string status)
        {
            return new[] { "active", "inactive", "discontinued" }.Contains(status?.ToLower());
        }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        private readonly IProductTypeService _productTypeService;
        private readonly IBranchService _branchService;

        public UpdateProductDtoValidator(IProductTypeService productTypeService, IBranchService branchService)
        {
            _productTypeService = productTypeService;
            _branchService = branchService;

            RuleFor(x => x.Name)
                .Length(2, 100).When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Product name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_.,()]+$").When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Product name contains invalid characters");

            RuleFor(x => x.Description)
                .Length(10, 1000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Product description must be between 10 and 1000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).When(x => x.Price.HasValue)
                .WithMessage("Product price must be greater than 0")
                .PrecisionScale(10, 2, false).When(x => x.Price.HasValue)
                .WithMessage("Price must have maximum 2 decimal places");

            RuleFor(x => x.ProductTypeId)
                .GreaterThan(0).When(x => x.ProductTypeId.HasValue)
                .WithMessage("Invalid product type ID")
                .MustAsync(async (productTypeId, cancellation) =>
                {
                    if (!productTypeId.HasValue) return true;
                    var productType = await _productTypeService.GetProductTypeByIdAsync(productTypeId.Value);
                    return productType != null;
                }).When(x => x.ProductTypeId.HasValue)
                .WithMessage("Product type does not exist");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).When(x => x.BranchId.HasValue)
                .WithMessage("Invalid branch ID")
                .MustAsync(async (branchId, cancellation) =>
                {
                    if (!branchId.HasValue) return true;
                    var branch = await _branchService.GetBranchByIdAsync(branchId.Value);
                    return branch != null;
                }).When(x => x.BranchId.HasValue)
                .WithMessage("Branch does not exist");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Invalid image URL format");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Status must be active, inactive, or discontinued");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue)
                .WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.Size)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Size))
                .WithMessage("Size cannot exceed 50 characters");

            RuleFor(x => x.Brand)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Brand))
                .WithMessage("Brand name cannot exceed 100 characters");

            RuleFor(x => x.Ingredients)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Ingredients))
                .WithMessage("Ingredients cannot exceed 500 characters");

            RuleFor(x => x.UsageInstructions)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.UsageInstructions))
                .WithMessage("Usage instructions cannot exceed 200 characters");

            RuleFor(x => x.SkinTypeRecommendation)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.SkinTypeRecommendation))
                .WithMessage("Skin type recommendation cannot exceed 100 characters");
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidStatus(string status)
        {
            return new[] { "active", "inactive", "discontinued" }.Contains(status?.ToLower());
        }
    }
} 