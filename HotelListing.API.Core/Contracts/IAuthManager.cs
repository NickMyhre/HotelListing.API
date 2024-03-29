﻿using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Core.Contracts
{
    public interface IAuthManager
    {
        //register users and return errors if unsuccessful, if empty = successful registration
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        Task<IEnumerable<IdentityError>> RegisterAdmin(ApiUserDto userDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);

        Task<string> CreateRefreshToken();
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}
