using Refit;
using Shopanizer.DataObjects;
using Shopanizer.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopanizer.Client
{
    public interface IAuthenticationClient
    {
        [Post("/api/CustomLogin")]
        IObservable<CustomLoginResult> Login([Body] LoginRequest paylod);



        [Post("/api/User")]
        IObservable<User> UserPost([Body] User user);

    }
}
