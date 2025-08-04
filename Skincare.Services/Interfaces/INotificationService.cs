using Skincare.BusinessObjects.DTOs;

namespace Skincare.Services.Interfaces
{
    public interface INotificationService
    {
        // Email notifications
        Task<bool> SendEmailAsync(EmailNotificationDto emailNotification);
        Task<bool> SendEmailWithTemplateAsync(string templateId, string recipientEmail, Dictionary<string, string> variables);
        Task<bool> SendBulkEmailAsync(List<EmailNotificationDto> emailNotifications);
        Task<bool> SendBulkEmailWithTemplateAsync(string templateId, List<string> recipientEmails, Dictionary<string, string> variables);

        // Push notifications
        Task<bool> SendPushNotificationAsync(PushNotificationDto pushNotification);
        Task<bool> SendBulkPushNotificationAsync(List<PushNotificationDto> pushNotifications);
        Task<bool> SendPushNotificationToUserAsync(string userId, string title, string body, Dictionary<string, object> data = null);

        // Order notifications
        Task<bool> SendOrderConfirmationAsync(int orderId);
        Task<bool> SendOrderStatusUpdateAsync(int orderId, string status);
        Task<bool> SendOrderShippedAsync(int orderId, string trackingNumber);
        Task<bool> SendOrderDeliveredAsync(int orderId);
        Task<bool> SendOrderCancelledAsync(int orderId, string reason);

        // User notifications
        Task<bool> SendWelcomeEmailAsync(string email, string username);
        Task<bool> SendPasswordResetAsync(string email, string resetToken);
        Task<bool> SendEmailVerificationAsync(string email, string verificationToken);
        Task<bool> SendAccountLockedAsync(string email, string reason);
        Task<bool> SendAccountUnlockedAsync(string email);

        // Marketing notifications
        Task<bool> SendNewsletterAsync(string subject, string content, List<string> recipientEmails);
        Task<bool> SendPromotionalEmailAsync(string subject, string content, List<string> recipientEmails);
        Task<bool> SendProductRecommendationAsync(string userId, List<int> productIds);
        Task<bool> SendAbandonedCartReminderAsync(string userId, int cartId);

        // System notifications
        Task<bool> SendSystemMaintenanceAsync(string message, List<string> recipientEmails);
        Task<bool> SendSecurityAlertAsync(string userId, string alertType, string details);
        Task<bool> SendBackInStockNotificationAsync(string userId, int productId);

        // Template management
        Task<NotificationTemplateDto> GetTemplateAsync(string templateId);
        Task<List<NotificationTemplateDto>> GetAllTemplatesAsync();
        Task<bool> CreateTemplateAsync(NotificationTemplateDto template);
        Task<bool> UpdateTemplateAsync(string templateId, NotificationTemplateDto template);
        Task<bool> DeleteTemplateAsync(string templateId);

        // Notification preferences
        Task<NotificationPreferencesDto> GetUserPreferencesAsync(string userId);
        Task<bool> UpdateUserPreferencesAsync(string userId, NotificationPreferencesDto preferences);
        Task<bool> UnsubscribeFromCategoryAsync(string userId, string category);
        Task<bool> ResubscribeToCategoryAsync(string userId, string category);

        // Notification tracking
        Task<NotificationDto> GetNotificationAsync(int notificationId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);

        // Analytics and reporting
        Task<NotificationStatsDto> GetNotificationStatsAsync(DateTime date);
        Task<NotificationStatsDto> GetNotificationStatsAsync(DateTime startDate, DateTime endDate);
        Task<List<NotificationTemplateStatsDto>> GetTemplateStatsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetNotificationCountsByTypeAsync(DateTime startDate, DateTime endDate);

        // Bulk operations
        Task<bool> SendBulkNotificationsAsync(BulkNotificationDto bulkNotification);
        Task<bool> ScheduleNotificationsAsync(List<CreateNotificationDto> notifications);
        Task<bool> CancelScheduledNotificationsAsync(List<int> notificationIds);

        // Health and monitoring
        Task<bool> TestEmailConnectionAsync();
        Task<bool> TestPushConnectionAsync();
        Task<Dictionary<string, object>> GetNotificationServiceHealthAsync();
    }
} 