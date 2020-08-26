using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class Product : TableEntity
    {
        public int count { get; set; }
        public string qr { get; set; }
    }
}
