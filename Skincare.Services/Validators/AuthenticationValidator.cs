using FluentValidation;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System.Linq;

namespace Skincare.Services.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private readonly IAccountService _accountService;

        public RegisterRequestValidator(IAccountService accountService)
        {
            _accountService = accountService;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores")
                .MustAsync(async (username, cancellation) =>
                {
                    if (string.IsNullOrEmpty(username)) return false;
                    var existingAccount = await _accountService.GetAccountByUsernameAsync(username);
                    return existingAccount == null;
                }).WithMessage("Username is already taken");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .MustAsync(async (email, cancellation) =>
                {
                    if (string.IsNullOrEmpty(email)) return false;
                    var existingAccount = await _accountService.GetAccountByEmailAsync(email);
                    return existingAccount == null;
                }).WithMessage("Email is already registered");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Length(8, 100).WithMessage("Password must be between 8 and 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[+]?[\d\s\-()]+$").WithMessage("Phone number contains invalid characters")
                .Length(10, 15).WithMessage("Phone number must be between 10 and 15 characters")
                .MustAsync(async (phoneNumber, cancellation) =>
                {
                    if (string.IsNullOrEmpty(phoneNumber)) return false;
                    var existingAccount = await _accountService.GetAccountByPhoneNumberAsync(phoneNumber);
                    return existingAccount == null;
                }).WithMessage("Phone number is already registered");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .Length(10, 200).WithMessage("Address must be between 10 and 200 characters");

            RuleFor(x => x.FullName)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FullName))
                .WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").When(x => !string.IsNullOrEmpty(x.FullName))
                .WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.Age)
                .InclusiveBetween(1, 120).When(x => x.Age.HasValue)
                .WithMessage("Age must be between 1 and 120");

            RuleFor(x => x.Gender)
                .Must(BeValidGender).When(x => !string.IsNullOrEmpty(x.Gender))
                .WithMessage("Gender must be male, female, or other");

            RuleFor(x => x.Bio)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Bio))
                .WithMessage("Bio cannot exceed 500 characters");

            // Business rules
            RuleFor(x => x)
                .MustAsync(async (request, cancellation) =>
                {
                    if (!string.IsNullOrEmpty(request.Username) && !string.IsNullOrEmpty(request.Email))
                    {
                        // Username should not be too similar to email
                        var username = request.Username.ToLower();
                        var emailUsername = request.Email.Split('@')[0].ToLower();
                        return !username.Contains(emailUsername) && !emailUsername.Contains(username);
                    }
                    return true;
                }).WithMessage("Username should not be too similar to email address");
        }

        private bool BeValidGender(string gender)
        {
            return new[] { "male", "female", "other" }.Contains(gender?.ToLower());
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(1).WithMessage("Password cannot be empty");
        }
    }

    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .Length(32, 256).WithMessage("Refresh token must be between 32 and 256 characters")
                .Matches(@"^[a-zA-Z0-9\-_]+$").WithMessage("Refresh token contains invalid characters");
        }
    }
} 