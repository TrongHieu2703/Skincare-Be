using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class SkinTypeService : ISkinTypeService
    {
        private readonly ISkinTypeRepository _skinTypeRepository;
        private readonly ILogger<SkinTypeService> _logger;

        public SkinTypeService(
            ISkinTypeRepository skinTypeRepository,
            ILogger<SkinTypeService> logger)
        {
            _skinTypeRepository = skinTypeRepository ?? throw new ArgumentNullException(nameof(skinTypeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<SkinTypeDto>> GetAllSkinTypesAsync()
        {
            try
            {
                _logger.LogInformation("GetAllSkinTypesAsync called");
                var skinTypes = await _skinTypeRepository.GetAllAsync();
                _logger.LogInformation($"Retrieved {skinTypes.Count()} skin types");
                return skinTypes.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all skin types");
                throw;
            }
        }

        public async Task<SkinTypeDto> GetSkinTypeByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"GetSkinTypeByIdAsync called with id {id}");
                var skinType = await _skinTypeRepository.GetByIdAsync(id);
                if (skinType == null)
                {
                    _logger.LogWarning($"Skin type with ID {id} not found");
                    return null;
                }
                _logger.LogInformation($"Retrieved skin type with ID {id}");
                return MapToDto(skinType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching skin type with ID {id}");
                throw;
            }
        }

        private SkinTypeDto MapToDto(SkinType skinType)
        {
            if (skinType == null) return null;
            
            return new SkinTypeDto
            {
                Id = skinType.Id,
                Name = skinType.Name,
                Score = skinType.Score
            };
        }
    }
} 