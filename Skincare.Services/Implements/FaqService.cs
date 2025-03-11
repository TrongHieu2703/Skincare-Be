using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class FaqService : IFaqService
    {
        private readonly IFaqRepository _faqRepository;
        private readonly ILogger<FaqService> _logger;

        public FaqService(IFaqRepository faqRepository, ILogger<FaqService> logger)
        {
            _faqRepository = faqRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<FaqDTO>> GetAllFaqsAsync()
        {
            try
            {
                return await _faqRepository.GetAllFaqsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FAQs");
                throw new ApplicationException("An error occurred while fetching FAQs.");
            }
        }

        public async Task<FaqDTO> GetFaqByIdAsync(int id)
        {
            try
            {
                var faq = await _faqRepository.GetFaqByIdAsync(id);
                if (faq == null)
                    throw new NotFoundException("FAQ not found.");
                return faq;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching FAQ with ID {id}");
                throw new ApplicationException("An error occurred while fetching the FAQ.");
            }
        }

        public async Task<FaqDTO> CreateFaqAsync(FaqDTO faq)
        {
            try
            {
                return await _faqRepository.CreateFaqAsync(faq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FAQ");
                throw new ApplicationException("An error occurred while creating the FAQ.");
            }
        }

        public async Task UpdateFaqAsync(FaqDTO faq)
        {
            try
            {
                // Kiểm tra tồn tại trước khi update
                var existingFaq = await _faqRepository.GetFaqByIdAsync(faq.Id);
                if (existingFaq == null)
                    throw new NotFoundException("FAQ not found for update.");
                await _faqRepository.UpdateFaqAsync(faq);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ");
                throw new ApplicationException("An error occurred while updating the FAQ.");
            }
        }

        public async Task DeleteFaqAsync(int id)
        {
            try
            {
                var faq = await _faqRepository.GetFaqByIdAsync(id);
                if (faq == null)
                    throw new NotFoundException("FAQ not found for deletion.");
                await _faqRepository.DeleteFaqAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FAQ");
                throw new ApplicationException("An error occurred while deleting the FAQ.");
            }
        }
    }
}
