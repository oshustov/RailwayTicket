﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Abstract;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Claim = System.Security.Claims.Claim;

namespace BLL.Concrete
{
    public class UserService : IUserService
    {
        private IUnitOfWork _unitOfWork;

        public UserService (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task CreateUser (User user)
        {
            Guard.ArgumentNotNull(user, "User should not be null");

            _unitOfWork.UserRepository.Add(user);

            return _unitOfWork.SaveChangesAsync();
        }

        public Task DeleteAsync(User user)
        {
            Guard.ArgumentNotNull(user, "User should not be null");

            _unitOfWork.UserRepository.Remove(user);

            return _unitOfWork.SaveChangesAsync();
        }

        public async Task<User> FindByIdAsync(Guid userId)
        {
            return await _unitOfWork.UserRepository.FindByIdAsync(userId);
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            return await _unitOfWork.UserRepository.FindByUserNameAsync(userName);
        }

        public Task UpdateAsync(User user)
        {
            _unitOfWork.UserRepository.Update(user);

            return _unitOfWork.SaveChangesAsync();
        }

        public async Task AddClaimsAsync(Guid userId, Claim claim)
        {
            var user = await FindByIdAsync(userId);
            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            var newClaim = new Domain.Entities.Claim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                User = user
            };
            user.Claims.Add(newClaim);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<Claim>> GetClaimsAsync(Guid identityUserId)
        {
            var user = await FindByIdAsync(identityUserId);

            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            return user.Claims.Select(x => new System.Security.Claims.Claim(x.ClaimType, x.ClaimValue)).ToList();
        }

        public async Task RemoveClaimAsync(Guid identityUserId, Claim claim)
        {
            var user = await FindByIdAsync(identityUserId);

            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            var claims = user.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            user.Claims.Remove(claims);

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddLoginAsync(Guid identityUserId, UserLoginInfo userloginInfo)
        {
            var user = await FindByIdAsync(identityUserId);
            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            var login = new Domain.Entities.ExternalLogin
            {
                LoginProvider = userloginInfo.LoginProvider,
                ProviderKey = userloginInfo.ProviderKey,
                User = user
            };
            user.Logins.Add(login);

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExternalLogin> FindAsync(UserLoginInfo userLoginInfo)
        {           
            return _unitOfWork.ExternalLoginRepository.GetByProviderAndKey(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(Guid userId)
        {
            var user = _unitOfWork.UserRepository.FindById(userId);
            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            return
                Task.FromResult<IList<UserLoginInfo>>(
                    user.Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList());
        }

        public async Task RemoveLoginAsync(Guid userId, UserLoginInfo userLoginInfo)
        {
            var user = _unitOfWork.UserRepository.FindById(userId);
            Guard.ArgumentNotNull(user, "IdentityUser does not correspond to a User entity.");

            var login = user.Logins.FirstOrDefault(x => x.LoginProvider == userLoginInfo.LoginProvider && x.ProviderKey == userLoginInfo.ProviderKey);
            user.Logins.Remove(login);

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddToRoleAsync(Guid userId, string roleName)
        {
            Guard.ArgumentNotWhiteSpaceOrNull(roleName, "RoleName should not be null or white space.");

            var user = await FindByIdAsync(userId);

            user.Roles.Add(_unitOfWork.RoleRepository.FindByName(roleName));

            await UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveFromRoleAsync(Guid userId, string roleName)
        {
            Guard.ArgumentNotWhiteSpaceOrNull(roleName);

            var user = await FindByIdAsync(userId);

            user.Roles.Remove(_unitOfWork.RoleRepository.FindByName(roleName));

            await UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<string>> GetRolesAsync(Guid userId)
        {
            var user = await FindByIdAsync(userId);
            Guard.ArgumentNotNull(user, "User should not be null.");

            var roles = user.Roles.Select(r => r.Name).ToList();

            return roles;
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            var user = await FindByIdAsync(userId);
            Guard.ArgumentNotNull(user, "User should not be null.");
            Guard.ArgumentNotWhiteSpaceOrNull(roleName);

            return user.Roles.Select(x => x.Name).Contains(roleName);
        }
    }
}