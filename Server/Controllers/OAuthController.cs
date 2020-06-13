using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class OAuthController : Controller
    {
        [HttpGet]
        public IActionResult Login(
            string response_type, // Authorization Flow Type
            string client_id,     // Identifier for Client
            string redirect_uri,
            string scope,         // Scope for Claims (e.g. email, tel)
            string state)         // Random String generated to persist state to client
        {
            // Create GET-query (?a=foo&b=bar)
            QueryBuilder query = new QueryBuilder();
            query.Add("redirectUri", redirect_uri);
            query.Add("state", state);
            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Login(
            string username,
            string redirectUri,
            string state)
        {
            // Generate a random code.
            // Store code in DB, Expire in short time (code should only be used to grab token)
            // If code expires, and user has no token, re-authentication is required
            const string code = "BAAABABBABAA";

            QueryBuilder query = new QueryBuilder
            {
                { "code", code },
                { "state", state }
            };


            return Ok($"{redirectUri}{query.ToString()}");
        }

        public async Task<IActionResult> Token(
            string grant_type,     // Access flow (Token-request)
            string code,           // Code from Login
            string redirect_uri,
            string client_id)
        {
            // Example Response: https://tools.ietf.org/html/rfc6749#section-4.1.4
            // Validate Code from DB
            bool expired = false;
            string dbCode = "BAAABABBABAA";
            if (expired || code != dbCode)
            {
                return BadRequest();
            }
            // Gen Token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")
            };

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.Secret)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Constants.Issuer,
                Constants.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(1),
                signingCredentials
                );

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token = access_token,
                token_type = "Bearer"
            };

            var jsonResponse = JsonConvert.SerializeObject(responseObject);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonResponse);
            await Response.Body.WriteAsync(bytes, 0, bytes.Length);
            // Send Code and Redirect back
            return Redirect(redirect_uri);
        }

        [Authorize]
        public async Task<IActionResult> Validate()
        {
            // Grab from Request. Change to get from header
            if(HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                // Check Claims, do whatever.. Validation has already been done through Authorize
                return Ok();
            }

            return BadRequest();
        }
    }
}
