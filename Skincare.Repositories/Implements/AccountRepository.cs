using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(SWP391Context context, ILogger<AccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            try
            {
                return await _context.Accounts.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                throw;
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            try
            {
                return await _context.Accounts.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account with ID {id}");
                throw;
            }
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Accounts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account with email {email}");
                throw;
            }
        }

        public async Task<Account> GetByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                return await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account with phone number: {phoneNumber}");
                throw;
            }
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            try
            {
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                throw;
            }
        }

        public async Task UpdateAccountAsync(Account account)
        {
            try
            {
                var existingAccount = await _context.Accounts.FindAsync(account.Id);
                if (existingAccount == null)
                {
                    _logger.LogWarning($"Account with ID {account.Id} not found.");
                    return;
                }

                // Map lại các trường cần update
                existingAccount.Username = account.Username;
                existingAccount.Email = account.Email;
                existingAccount.Avatar = account.Avatar;
                existingAccount.Address = account.Address;
                existingAccount.PhoneNumber = account.PhoneNumber;
                existingAccount.Status = account.Status;
                existingAccount.PasswordHash = account.PasswordHash;
                existingAccount.Role = account.Role;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating account with ID {account.Id}");
                throw;
            }
        }

        public async Task DeleteAccountAsync(int id)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(id);
                if (account != null)
                {
                    // 1. Xoá các Review liên quan
                    var relatedReviews = _context.Reviews.Where(r => r.CustomerId == id);
                    if (relatedReviews.Any())
                    {
                        _context.Reviews.RemoveRange(relatedReviews);
                        _logger.LogInformation($"Removed {relatedReviews.Count()} review(s) for account ID {id}");
                    }

                    // 2. Xoá các SkinCareRoutine liên quan
                    var relatedSkinCareRoutines = _context.SkinCareRoutines.Where(s => s.CustomerId == id);
                    if (relatedSkinCareRoutines.Any())
                    {
                        _context.SkinCareRoutines.RemoveRange(relatedSkinCareRoutines);
                        _logger.LogInformation($"Removed {relatedSkinCareRoutines.Count()} skin care routine(s) for account ID {id}");
                    }

                    // 3. Xoá các BlogPost liên quan (blog_owner_id = account id)
                    var relatedBlogPosts = _context.BlogPosts.Where(bp => bp.BlogOwnerId == id);
                    if (relatedBlogPosts.Any())
                    {
                        _context.BlogPosts.RemoveRange(relatedBlogPosts);
                        _logger.LogInformation($"Removed {relatedBlogPosts.Count()} blog post(s) for account ID {id}");
                    }

                    // 4. Xoá các CustomerTest liên quan
                    var relatedCustomerTests = _context.CustomerTests.Where(ct => ct.CustomerId == id);
                    if (relatedCustomerTests.Any())
                    {
                        _context.CustomerTests.RemoveRange(relatedCustomerTests);
                        _logger.LogInformation($"Removed {relatedCustomerTests.Count()} customer test(s) for account ID {id}");
                    }

                    // 5. Xoá các Order liên quan và OrderItem của chúng
                    var relatedOrders = _context.Orders.Where(o => o.CustomerId == id).ToList();
                    if (relatedOrders.Any())
                    {
                        // Lấy danh sách Order IDs
                        var orderIds = relatedOrders.Select(o => o.Id).ToList();
                        // Xoá các OrderItem liên quan
                        var relatedOrderItems = _context.OrderItems.Where(oi => orderIds.Contains(oi.OrderId));
                        if (relatedOrderItems.Any())
                        {
                            _context.OrderItems.RemoveRange(relatedOrderItems);
                            _logger.LogInformation($"Removed {relatedOrderItems.Count()} order item(s) for account ID {id}");
                        }

                        // Sau đó, xoá các Order
                        _context.Orders.RemoveRange(relatedOrders);
                        _logger.LogInformation($"Removed {relatedOrders.Count} order(s) for account ID {id}");
                    }

                    // 6. Cuối cùng, xoá Account
                    _context.Accounts.Remove(account);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account with ID {id}");
                throw;
            }
        }
    }
}
