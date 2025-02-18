using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return await _accountRepository.GetAllAccountsAsync();
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            return await _accountRepository.GetAccountByIdAsync(id);
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _accountRepository.GetByEmailAsync(email);
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            return await _accountRepository.CreateAccountAsync(account);
        }

        public async Task UpdateAccountAsync(Account account)
        {
            await _accountRepository.UpdateAccountAsync(account);
        }

        public async Task DeleteAccountAsync(int id)
        {
            await _accountRepository.DeleteAccountAsync(id);
        }
    }
}
