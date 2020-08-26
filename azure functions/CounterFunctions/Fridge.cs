using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class Fridge : TableEntity
    {
        public string Id { get; set; }
    }
}