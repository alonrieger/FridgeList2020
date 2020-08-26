using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class UserInUsersTable : TableEntity
    {
        public string Password { get; set; }
    }
}