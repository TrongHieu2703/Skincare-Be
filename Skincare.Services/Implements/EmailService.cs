using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Skincare.Services.Implements
{
    public class EmailService : INotificationService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _useSsl;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@skincare.com";
            _fromName = _configuration["Email:FromName"] ?? "Skincare Store";
            _useSsl = bool.Parse(_configuration["Email:UseSsl"] ?? "false");
        }

        public async Task<bool> SendEmailAsync(EmailNotificationDto emailNotification)
        {
            try
            {
                _logger.LogInformation($"Sending email to: {emailNotification.To}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", emailNotification.To));
                message.Subject = emailNotification.Subject;

                if (!string.IsNullOrEmpty(emailNotification.ReplyTo))
                {
                    message.ReplyTo.Add(new MailboxAddress("", emailNotification.ReplyTo));
                }

                // Add CC recipients
                foreach (var cc in emailNotification.Cc)
                {
                    message.Cc.Add(new MailboxAddress("", cc));
                }

                // Add BCC recipients
                foreach (var bcc in emailNotification.Bcc)
                {
                    message.Bcc.Add(new MailboxAddress("", bcc));
                }

                // Create multipart message
                var multipart = new MultipartAlternative();

                // Add text body
                if (!string.IsNullOrEmpty(emailNotification.TextBody))
                {
                    multipart.Add(new TextPart(TextFormat.Plain) { Text = emailNotification.TextBody });
                }

                // Add HTML body
                if (!string.IsNullOrEmpty(emailNotification.HtmlBody))
                {
                    multipart.Add(new TextPart(TextFormat.Html) { Text = emailNotification.HtmlBody });
                }

                // Add attachments
                if (emailNotification.Attachments.Any())
                {
                    var mixed = new MultipartMixed();
                    mixed.Add(multipart);

                    foreach (var attachment in emailNotification.Attachments)
                    {
                        var attachmentPart = new MimePart(attachment.ContentType)
                        {
                            Content = new MimeContent(new MemoryStream(attachment.Content)),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = attachment.FileName
                        };

                        if (!string.IsNullOrEmpty(attachment.ContentId))
                        {
                            attachmentPart.ContentId = attachment.ContentId;
                        }

                        mixed.Add(attachmentPart);
                    }

                    message.Body = mixed;
                }
                else
                {
                    message.Body = multipart;
                }

                // Send email
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, _useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(_smtpUsername))
                {
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to: {emailNotification.To}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to: {emailNotification.To}");
                return false;
            }
        }

        public async Task<bool> SendEmailWithTemplateAsync(string templateId, string recipientEmail, Dictionary<string, string> variables)
        {
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError($"Template not found: {templateId}");
                    return false;
                }

                var htmlContent = ReplaceTemplateVariables(template.HtmlContent, variables);
                var textContent = ReplaceTemplateVariables(template.TextContent, variables);

                var emailNotification = new EmailNotificationDto
                {
                    To = recipientEmail,
                    Subject = template.Subject,
                    HtmlBody = htmlContent,
                    TextBody = textContent
                };

                return await SendEmailAsync(emailNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email with template {templateId} to {recipientEmail}");
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(List<EmailNotificationDto> emailNotifications)
        {
            try
            {
                _logger.LogInformation($"Sending bulk email to {emailNotifications.Count} recipients");

                var successCount = 0;
                foreach (var emailNotification in emailNotifications)
                {
                    if (await SendEmailAsync(emailNotification))
                    {
                        successCount++;
                    }
                }

                _logger.LogInformation($"Bulk email completed: {successCount}/{emailNotifications.Count} sent successfully");
                return successCount == emailNotifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk emails");
                return false;
            }
        }

        public async Task<bool> SendBulkEmailWithTemplateAsync(string templateId, List<string> recipientEmails, Dictionary<string, string> variables)
        {
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError($"Template not found: {templateId}");
                    return false;
                }

                var emailNotifications = new List<EmailNotificationDto>();
                foreach (var email in recipientEmails)
                {
                    var htmlContent = ReplaceTemplateVariables(template.HtmlContent, variables);
                    var textContent = ReplaceTemplateVariables(template.TextContent, variables);

                    emailNotifications.Add(new EmailNotificationDto
                    {
                        To = email,
                        Subject = template.Subject,
                        HtmlBody = htmlContent,
                        TextBody = textContent
                    });
                }

                return await SendBulkEmailAsync(emailNotifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send bulk email with template {templateId}");
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(int orderId)
        {
            try
            {
                // This would typically fetch order details from the database
                var variables = new Dictionary<string, string>
                {
                    ["orderId"] = orderId.ToString(),
                    ["orderDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["orderTotal"] = "$99.99" // This would be fetched from order
                };

                return await SendEmailWithTemplateAsync("order-confirmation", "customer@example.com", variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order confirmation for order {orderId}");
                return false;
            }
        }

        public async Task<bool> SendOrderStatusUpdateAsync(int orderId, string status)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["orderId"] = orderId.ToString(),
                    ["status"] = status,
                    ["updateDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return await SendEmailWithTemplateAsync("order-status-update", "customer@example.com", variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order status update for order {orderId}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string username)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["welcomeDate"] = DateTime.Now.ToString("yyyy-MM-dd")
                };

                return await SendEmailWithTemplateAsync("welcome-email", email, variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
        {
            try
            {
                var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={resetToken}";
                var variables = new Dictionary<string, string>
                {
                    ["resetUrl"] = resetUrl,
                    ["expiryTime"] = DateTime.Now.AddHours(24).ToString("yyyy-MM-dd HH:mm:ss")
                };

                return await SendEmailWithTemplateAsync("password-reset", email, variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                return false;
            }
        }

        public async Task<bool> SendNewsletterAsync(string subject, string content, List<string> recipientEmails)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["content"] = content,
                    ["newsletterDate"] = DateTime.Now.ToString("MMMM yyyy")
                };

                return await SendBulkEmailWithTemplateAsync("newsletter", recipientEmails, variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send newsletter");
                return false;
            }
        }

        // Helper methods
        private string ReplaceTemplateVariables(string template, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(template) || variables == null)
                return template;

            var result = template;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            return result;
        }

        // Placeholder implementations for other notification types
        public async Task<bool> SendPushNotificationAsync(PushNotificationDto pushNotification) => true;
        public async Task<bool> SendBulkPushNotificationAsync(List<PushNotificationDto> pushNotifications) => true;
        public async Task<bool> SendPushNotificationToUserAsync(string userId, string title, string body, Dictionary<string, object> data = null) => true;
        public async Task<bool> SendOrderShippedAsync(int orderId, string trackingNumber) => true;
        public async Task<bool> SendOrderDeliveredAsync(int orderId) => true;
        public async Task<bool> SendOrderCancelledAsync(int orderId, string reason) => true;
        public async Task<bool> SendEmailVerificationAsync(string email, string verificationToken) => true;
        public async Task<bool> SendAccountLockedAsync(string email, string reason) => true;
        public async Task<bool> SendAccountUnlockedAsync(string email) => true;
        public async Task<bool> SendPromotionalEmailAsync(string subject, string content, List<string> recipientEmails) => true;
        public async Task<bool> SendProductRecommendationAsync(string userId, List<int> productIds) => true;
        public async Task<bool> SendAbandonedCartReminderAsync(string userId, int cartId) => true;
        public async Task<bool> SendSystemMaintenanceAsync(string message, List<string> recipientEmails) => true;
        public async Task<bool> SendSecurityAlertAsync(string userId, string alertType, string details) => true;
        public async Task<bool> SendBackInStockNotificationAsync(string userId, int productId) => true;

        // Template management
        public async Task<NotificationTemplateDto> GetTemplateAsync(string templateId)
        {
            // Placeholder implementation - in real scenario, you'd fetch from database
            return new NotificationTemplateDto
            {
                Id = templateId,
                Name = "Order Confirmation",
                Subject = "Order Confirmation - Order #{orderId}",
                HtmlContent = "<h1>Thank you for your order!</h1><p>Order ID: {orderId}</p>",
                TextContent = "Thank you for your order! Order ID: {orderId}",
                Type = "email",
                IsActive = true
            };
        }

        public async Task<List<NotificationTemplateDto>> GetAllTemplatesAsync() => new List<NotificationTemplateDto>();
        public async Task<bool> CreateTemplateAsync(NotificationTemplateDto template) => true;
        public async Task<bool> UpdateTemplateAsync(string templateId, NotificationTemplateDto template) => true;
        public async Task<bool> DeleteTemplateAsync(string templateId) => true;

        // Notification preferences
        public async Task<NotificationPreferencesDto> GetUserPreferencesAsync(string userId) => new NotificationPreferencesDto { UserId = userId };
        public async Task<bool> UpdateUserPreferencesAsync(string userId, NotificationPreferencesDto preferences) => true;
        public async Task<bool> UnsubscribeFromCategoryAsync(string userId, string category) => true;
        public async Task<bool> ResubscribeToCategoryAsync(string userId, string category) => true;

        // Notification tracking
        public async Task<NotificationDto> GetNotificationAsync(int notificationId) => new NotificationDto();
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 10) => new List<NotificationDto>();
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId) => true;
        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId) => true;
        public async Task<bool> DeleteNotificationAsync(int notificationId) => true;

        // Analytics and reporting
        public async Task<NotificationStatsDto> GetNotificationStatsAsync(DateTime date) => new NotificationStatsDto { Date = date };
        public async Task<NotificationStatsDto> GetNotificationStatsAsync(DateTime startDate, DateTime endDate) => new NotificationStatsDto { Date = startDate };
        public async Task<List<NotificationTemplateStatsDto>> GetTemplateStatsAsync(DateTime startDate, DateTime endDate) => new List<NotificationTemplateStatsDto>();
        public async Task<Dictionary<string, int>> GetNotificationCountsByTypeAsync(DateTime startDate, DateTime endDate) => new Dictionary<string, int>();

        // Bulk operations
        public async Task<bool> SendBulkNotificationsAsync(BulkNotificationDto bulkNotification) => true;
        public async Task<bool> ScheduleNotificationsAsync(List<CreateNotificationDto> notifications) => true;
        public async Task<bool> CancelScheduledNotificationsAsync(List<int> notificationIds) => true;

        // Health and monitoring
        public async Task<bool> TestEmailConnectionAsync()
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, _useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(_smtpUsername))
                {
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                }

                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email connection test failed");
                return false;
            }
        }

        public async Task<bool> TestPushConnectionAsync() => true;
        public async Task<Dictionary<string, object>> GetNotificationServiceHealthAsync() => new Dictionary<string, object>();
    }
} 