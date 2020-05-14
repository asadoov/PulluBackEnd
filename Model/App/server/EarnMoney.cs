using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App.server
{
    public class EarnMoney
    {
       public int statusCode { get; set; } // 1 - ok || 2 - 24 hour limit or not inserted || 3 - Timeout || 4 - server problem

    }
}
