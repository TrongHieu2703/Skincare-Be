using Skincare.Repositories.Interfaces;
using Skincare.Services.Dtos.Request;
using Skincare.Services.Dtos.Response;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        //private readonly IProductTypeRepository _repo;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        //public AccountService(IAccountRepository accountRepository, IProductTypeRepository repo)
        //{
        //    _accountRepository = accountRepository;
        //    _repo = repo
        //}

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = new LoginResponse { Token = "đâsdasdasdasd", Email = "Hieu@gmail.com" };
            await Task.Delay(1000);
            return response;
        }
    }
}
