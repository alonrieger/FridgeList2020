using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class RowInLastActivity : TableEntity
    {
        public string content { get; set; }
    }
}