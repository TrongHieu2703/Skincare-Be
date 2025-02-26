using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;

public class FaqRepository : IFaqRepository
{
    private readonly SWP391Context _context;
    private readonly ILogger<FaqRepository> _logger;

    public FaqRepository(SWP391Context context, ILogger<FaqRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FaqDTO>> GetAllFaqsAsync()
    {
        try
        {
            return await _context.Faqs.Select(f => new FaqDTO
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                LastUpdateAt = f.LastUpdateAt,
                IsVisible = f.IsVisible
            }).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching FAQs from database");
            throw;
        }
    }

    public async Task<FaqDTO> GetFaqByIdAsync(int id)
    {
        try
        {
            var faq = await _context.Faqs.Where(f => f.Id == id).Select(f => new FaqDTO
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                LastUpdateAt = f.LastUpdateAt,
                IsVisible = f.IsVisible
            }).FirstOrDefaultAsync();

            if (faq == null)
                throw new KeyNotFoundException($"FAQ with ID {id} not found.");

            return faq;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching FAQ with ID {id}");
            throw;
        }
    }

    public async Task<FaqDTO> CreateFaqAsync(FaqDTO faq)
    {
        try
        {
            var newFaq = new Faq
            {
                Question = faq.Question,
                Answer = faq.Answer,
                LastUpdateAt = DateTime.UtcNow,
                IsVisible = faq.IsVisible
            };

            _context.Faqs.Add(newFaq);
            await _context.SaveChangesAsync();

            faq.Id = newFaq.Id;
            return faq;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FAQ");
            throw;
        }
    }

    public async Task UpdateFaqAsync(FaqDTO faq)
    {
        try
        {
            var existingFaq = await _context.Faqs.FindAsync(faq.Id);
            if (existingFaq == null)
                throw new KeyNotFoundException($"FAQ with ID {faq.Id} not found.");

            existingFaq.Question = faq.Question;
            existingFaq.Answer = faq.Answer;
            existingFaq.LastUpdateAt = DateTime.UtcNow;
            existingFaq.IsVisible = faq.IsVisible;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ");
            throw;
        }
    }

    public async Task DeleteFaqAsync(int id)
    {
        try
        {
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null)
                throw new KeyNotFoundException($"FAQ with ID {id} not found.");

            _context.Faqs.Remove(faq);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FAQ");
            throw;
        }
    }
}
