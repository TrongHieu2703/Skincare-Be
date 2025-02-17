using Skincare.BusinessObjects.Entities;
using Skincare.Services.Dtos.Request;
using Skincare.Services.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IAccountService
    {
        Task<LoginResponse> LoginAsync(LoginRequest? request);
    }
}
