using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendEmail([FromBody] EmailNotificationDto emailNotification)
        {
            try
            {
                var result = await _notificationService.SendEmailAsync(emailNotification);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send email",
                        new List<ApiError> { ApiError.BusinessError("EMAIL_SEND_FAILED", "Email could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Email sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send email",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("email/template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendEmailWithTemplate([FromBody] CreateNotificationDto notificationDto)
        {
            try
            {
                var result = await _notificationService.SendEmailWithTemplateAsync(
                    notificationDto.TemplateId,
                    notificationDto.RecipientEmail,
                    notificationDto.TemplateVariables
                );

                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send email with template",
                        new List<ApiError> { ApiError.BusinessError("TEMPLATE_EMAIL_FAILED", "Template email could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Template email sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template email");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send template email",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("push")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendPushNotification([FromBody] PushNotificationDto pushNotification)
        {
            try
            {
                var result = await _notificationService.SendPushNotificationAsync(pushNotification);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send push notification",
                        new List<ApiError> { ApiError.BusinessError("PUSH_NOTIFICATION_FAILED", "Push notification could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Push notification sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send push notification",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("order/confirmation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendOrderConfirmation([FromQuery] int orderId)
        {
            try
            {
                var result = await _notificationService.SendOrderConfirmationAsync(orderId);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send order confirmation",
                        new List<ApiError> { ApiError.BusinessError("ORDER_CONFIRMATION_FAILED", "Order confirmation could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Order confirmation sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order confirmation");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send order confirmation",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("order/status-update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendOrderStatusUpdate([FromQuery] int orderId, [FromQuery] string status)
        {
            try
            {
                var result = await _notificationService.SendOrderStatusUpdateAsync(orderId, status);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send order status update",
                        new List<ApiError> { ApiError.BusinessError("ORDER_STATUS_UPDATE_FAILED", "Order status update could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Order status update sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order status update");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send order status update",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("welcome")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendWelcomeEmail([FromQuery] string email, [FromQuery] string username)
        {
            try
            {
                var result = await _notificationService.SendWelcomeEmailAsync(email, username);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send welcome email",
                        new List<ApiError> { ApiError.BusinessError("WELCOME_EMAIL_FAILED", "Welcome email could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Welcome email sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send welcome email",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordReset([FromQuery] string email, [FromQuery] string resetToken)
        {
            try
            {
                var result = await _notificationService.SendPasswordResetAsync(email, resetToken);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send password reset email",
                        new List<ApiError> { ApiError.BusinessError("PASSWORD_RESET_FAILED", "Password reset email could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Password reset email sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send password reset email",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("newsletter")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNewsletter([FromBody] NewsletterRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendNewsletterAsync(request.Subject, request.Content, request.RecipientEmails);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to send newsletter",
                        new List<ApiError> { ApiError.BusinessError("NEWSLETTER_FAILED", "Newsletter could not be sent") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Newsletter sent successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending newsletter");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to send newsletter",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("templates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTemplates()
        {
            try
            {
                var templates = await _notificationService.GetAllTemplatesAsync();
                
                var response = ApiResponse<List<NotificationTemplateDto>>.SuccessResult(templates, "Templates retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates");
                var errorResponse = ApiResponse<List<NotificationTemplateDto>>.ErrorResult(
                    "Failed to get templates",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("templates/{templateId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTemplate(string templateId)
        {
            try
            {
                var template = await _notificationService.GetTemplateAsync(templateId);
                
                if (template == null)
                {
                    var errorResponse = ApiResponse<NotificationTemplateDto>.ErrorResult(
                        "Template not found",
                        new List<ApiError> { ApiError.BusinessError("TEMPLATE_NOT_FOUND", "Template with specified ID does not exist") }
                    );
                    return NotFound(errorResponse);
                }

                var response = ApiResponse<NotificationTemplateDto>.SuccessResult(template, "Template retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template");
                var errorResponse = ApiResponse<NotificationTemplateDto>.ErrorResult(
                    "Failed to get template",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("templates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] NotificationTemplateDto template)
        {
            try
            {
                var result = await _notificationService.CreateTemplateAsync(template);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to create template",
                        new List<ApiError> { ApiError.BusinessError("TEMPLATE_CREATION_FAILED", "Template could not be created") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Template created successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to create template",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("preferences/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserPreferences(string userId)
        {
            try
            {
                var preferences = await _notificationService.GetUserPreferencesAsync(userId);
                
                var response = ApiResponse<NotificationPreferencesDto>.SuccessResult(preferences, "User preferences retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences");
                var errorResponse = ApiResponse<NotificationPreferencesDto>.ErrorResult(
                    "Failed to get user preferences",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("preferences/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserPreferences(string userId, [FromBody] NotificationPreferencesDto preferences)
        {
            try
            {
                var result = await _notificationService.UpdateUserPreferencesAsync(userId, preferences);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to update user preferences",
                        new List<ApiError> { ApiError.BusinessError("PREFERENCES_UPDATE_FAILED", "User preferences could not be updated") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "User preferences updated successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to update user preferences",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNotificationStats([FromQuery] DateTime? date = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                NotificationStatsDto stats;
                
                if (date.HasValue)
                {
                    stats = await _notificationService.GetNotificationStatsAsync(date.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    stats = await _notificationService.GetNotificationStatsAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    stats = await _notificationService.GetNotificationStatsAsync(DateTime.Today);
                }

                var response = ApiResponse<NotificationStatsDto>.SuccessResult(stats, "Notification stats retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                var errorResponse = ApiResponse<NotificationStatsDto>.ErrorResult(
                    "Failed to get notification stats",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("test/email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestEmailConnection()
        {
            try
            {
                var result = await _notificationService.TestEmailConnectionAsync();
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Email connection test failed",
                        new List<ApiError> { ApiError.BusinessError("EMAIL_CONNECTION_FAILED", "Email service connection test failed") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Email connection test successful");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing email connection");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to test email connection",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }
    }

    public class NewsletterRequestDto
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<string> RecipientEmails { get; set; } = new List<string>();
    }
} 