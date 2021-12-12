using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ApiDiagnostics.Model;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Mvc;

    namespace JwtTokenDemo.Controllers
    {
        [Route("api/[controller]")]
        public class TokenController : Controller
        {
            // POST api/Token
            [HttpPost]
            public IActionResult GetToken([FromBody] TokenRequest tokenRequest)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var verifiedUser = VerifyCredentials(tokenRequest.Username, tokenRequest.Password);

                if (verifiedUser == null)
                {
                    return Unauthorized();
                }

                //L'utente ha fornito credenziali valide
                //creiamo per lui una ClaimsIdentity
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                //Aggiungiamo uno o più claim relativi all'utente loggato
                identity.AddClaim(new Claim(ClaimTypes.Name, verifiedUser.Username));
                //Incapsuliamo l'identità in una ClaimsPrincipal e associamola alla richiesta corrente
                HttpContext.User = new ClaimsPrincipal(identity);

                //Restituiamo l'utente, senza pw. Il token verrà prodotto dal JwtTokenMiddleware e verrà messo nello header
                return Ok(verifiedUser);
            }

            private User VerifyCredentials(string username, string password)
            {
                User onlyOneUser = new User() { Id = 1, Username = "admin", Password = "123" };

                User[] users = new User[] { onlyOneUser };

                foreach (var user in users)
                {
                    if (username == user.Username && password == user.Password)
                        return user;
                }
                return null;
            }

        }
    }
}
