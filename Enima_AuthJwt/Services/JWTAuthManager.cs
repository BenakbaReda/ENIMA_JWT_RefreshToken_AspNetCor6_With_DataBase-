using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Enima_AuthJwt
{
    public interface IJWTAuthManager
    {
        AuthResponse Authenticate(string username, string password);
        AuthResponse Refresh(RefreshCred refreshCred);
    }
    public class JWTAuthManager : IJWTAuthManager
    {  
        private   AppDbContext dbContext;
 

        public JWTAuthManager(AppDbContext context)
        {
            dbContext = context;
        }
 
        public AuthResponse Authenticate(string mail, string password)
        {
            var user  = dbContext.Users.Where(b => b.Email == mail && b.Password == password).FirstOrDefault();

            if ( user ==null)
            {
                return null;
            }
             
            var token = GenerateTokenString(user.Email);

            
            string refreshToken  = GenerateRefreshToken();
 
            RefreshToken refreshDb = dbContext.RefreshTokens.Where(b => b.UserId == user.uuid).FirstOrDefault();
 


            if (refreshDb !=  null)  
            {
                refreshDb.TokenHash = refreshToken;
                dbContext.RefreshTokens.Update(refreshDb)  ;
            }
            else
            {
                RefreshToken refreshDbnew = new RefreshToken()
                {
                    TokenHash = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(1), // for test , u can use   AddMinutes(2)
                    UserId = (Guid)user.uuid


                };
                dbContext.RefreshTokens.Add(refreshDbnew);
            }
            dbContext.SaveChanges();
            return new AuthResponse
            {
                JwtToken = token,
                RefreshToken = refreshToken
            };
        }


        public AuthResponse Refresh(RefreshCred refreshCred)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            var pricipal = tokenHandler.ValidateToken(
                refreshCred.JwtToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtSetting.key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
                },
                out validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;

          

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token passed!");
            }

            var userMail = pricipal.FindFirst(ClaimTypes.Email)?.Value;
            if (userMail == null)
            {
                throw new SecurityTokenException("Invalid token passed!");
            }
            var user = dbContext.Users.Where(b => b.Email == userMail).FirstOrDefault();

            if (user == null)
            {
                throw new SecurityTokenException("Invalid token passed!");
            }


            RefreshToken refreshDb = dbContext.RefreshTokens.Where(b => b.UserId == user.uuid).FirstOrDefault();



            if (refreshCred.RefreshToken != refreshDb.TokenHash )
            {
                throw new SecurityTokenException("Invalid token passed!");
            }

            if ( refreshDb.ExpiryDate < DateTime.UtcNow  )
            {
                throw new SecurityTokenException("expire refreshtoken ");
            }

            return new AuthResponse
            {
                JwtToken = this.GenerateTokenString(user.Email),
                RefreshToken = refreshCred.RefreshToken
            };
          
        }

        string GenerateTokenString(string usermail,   Claim[] claims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSetting.tokenKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                 claims ?? new Claim[]
                {
                    new Claim(ClaimTypes.Email, usermail)
                }),
                //NotBefore = expires,
                Expires = DateTime.UtcNow.AddMinutes(15),// for test , u can use   AddSecondes(30)
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }


         string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

    }



}