using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFiy.Application.Comman
{
    public enum ErrorType
    {
        Validation,
        NotFound,
        Unauthorized,
        Conflict,
        Forbidden,
        ServerError
    }
}
