using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // email, push, sms, in-app
        public string Status { get; set; } // pending, sent, failed, delivered
        public string RecipientId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public int RetryCount { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string TemplateId { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; } = new Dictionary<string, string>();
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "email";
        public string RecipientId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public bool SendImmediately { get; set; } = true;
        public DateTime? ScheduledAt { get; set; }
    }

    public class NotificationTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // email, push, sms
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
        public string TextContent { get; set; }
        public List<string> RequiredVariables { get; set; } = new List<string>();
        public Dictionary<string, string> DefaultVariables { get; set; } = new Dictionary<string, string>();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmailNotificationDto
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public List<string> Cc { get; set; } = new List<string>();
        public List<string> Bcc { get; set; } = new List<string>();
        public List<EmailAttachmentDto> Attachments { get; set; } = new List<EmailAttachmentDto>();
        public string ReplyTo { get; set; }
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
    }

    public class EmailAttachmentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public string ContentId { get; set; }
    }

    public class PushNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string UserId { get; set; }
        public string DeviceToken { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string ImageUrl { get; set; }
        public string ActionUrl { get; set; }
        public int? Badge { get; set; }
        public string Sound { get; set; } = "default";
        public int? Ttl { get; set; } = 86400; // 24 hours
        public bool Silent { get; set; } = false;
    }

    public class NotificationPreferencesDto
    {
        public string UserId { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool InAppNotifications { get; set; } = true;
        public bool OrderUpdates { get; set; } = true;
        public bool PromotionalEmails { get; set; } = true;
        public bool Newsletter { get; set; } = true;
        public bool ProductRecommendations { get; set; } = true;
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public List<string> UnsubscribedCategories { get; set; } = new List<string>();
    }

    public class NotificationStatsDto
    {
        public DateTime Date { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> NotificationsByStatus { get; set; } = new Dictionary<string, int>();
        public List<NotificationTemplateStatsDto> TemplateStats { get; set; } = new List<NotificationTemplateStatsDto>();
    }

    public class NotificationTemplateStatsDto
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class BulkNotificationDto
    {
        public string TemplateId { get; set; }
        public List<string> RecipientIds { get; set; } = new List<string>();
        public List<string> RecipientEmails { get; set; } = new List<string>();
        public Dictionary<string, string> TemplateVariables { get; set; } = new Dictionary<string, string>();
        public string Type { get; set; } = "email";
        public DateTime? ScheduledAt { get; set; }
        public int BatchSize { get; set; } = 100;
        public int DelayBetweenBatches { get; set; } = 1000; // milliseconds
    }
} 