using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Basics.Transformer
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        /// <summary>
        /// Called before every authorization-check
        /// Use to transpose/transform claims
        /// Used in a hacky way here to ensure a certain claim is set
        /// NOTE: DOES NOT WRITE TO SESSION, ONLY TRANSFORMS FOR LATER USE (a refresh will NOT keep the claim added here)
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var hasFriendClaim = principal.Claims.Any(x => x.Type == "Friend");

            if (!hasFriendClaim)
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("Friend", "New"));

            return Task.FromResult(principal);
        }
    }
}
