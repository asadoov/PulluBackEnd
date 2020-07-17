using System;
using System.Collections.Generic;

namespace PulluBackEnd.Model.App.server
{
    public class ResponseStruct<T>
    {
        public int status { get; set; }
        public List<T> data { get; set; }/*any type now*/

    }
}
