using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class productInShoppingList : TableEntity
    {
        public int count { get; set; }
    }

}