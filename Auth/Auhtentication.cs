using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using mvc_user_profile.Models.Entities;
using Newtonsoft.Json;
using mvc_user_profile.Data.Repositories;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;


namespace mvc_user_profile.Auth
{
    public class AuhtenticationManager
    {
        private const string Secret = "bs5OIsj+BXE9NZDy7ycW3TcNekrF+2d/1sFnWG4HnV8KOL30iTOdtVWJG8abWvB1GlOgJuQZdcF2Lmvn/hccMo==";  
        public static TokenData GenerateToken(string username, int expireMinutes = 1) 
        {  
            var symmetricKey = Convert.FromBase64String(Secret);  
            var tokenHandler = new JwtSecurityTokenHandler();  
            var now = DateTime.UtcNow;  
            var user = new UserRepository().GetUser(username);
            var calims = new List<Claim> {
                    new Claim("userid", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("firstname", user.FirstName),
                    new Claim("lastname", user.LastName),
            };
            calims.AddRange(user.Roles);

            var tokenDescriptor = new SecurityTokenDescriptor {  
                Subject = new ClaimsIdentity(calims),
                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),  
                Issuer = "SmartStores",
                Audience = "CustomClient",
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)                
            };
            
            var stoken = tokenHandler.CreateToken(tokenDescriptor);  
            
            var tokenData = new TokenData();
            tokenData.Token = tokenHandler.WriteToken(stoken);


            return tokenData;  
        }

        public static bool ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return false;

            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            Claim usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim.Value;

            return !string.IsNullOrEmpty(username);
        }

        public static User GetTokenData(string token)
        {
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return null;

            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            var user = new User {
                Username =  identity.FindFirst(ClaimTypes.Name).Value,
                Email =  identity.FindFirst(ClaimTypes.Email).Value,
                FirstName =  identity.FindFirst("firstname").Value,
                LastName =  identity.FindFirst("lastname").Value,
            };
            return user;        
        }
        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                if (jwtToken == null)
                    return null;

                byte[] key = Convert.FromBase64String(Secret);

                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    //I set this to false to avoid expiration time 
                    //put in production system it may be required
                    RequireExpirationTime = false,
                    ValidateLifetime = false,

                    ValidateIssuer = true,
                    ValidIssuer = "SmartStores",
                    
                    ValidateAudience = true,
                    ValidAudience = "CustomClient",

                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool CheckUser(string username, string password)
        {
            var user  = new UserRepository().GetUser(username, password);
            try {
                if (!user.Equals(null)) {
                    return true;
                }
                return false;
            } catch (Exception ex) {
                return false;
            }
        }
        public static bool IsLoggedIn (string sessionJWT)
        {
            return ValidateToken(sessionJWT);
        }
    }
}