﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace CounterFunctions
{
    public class UserInFridge : TableEntity
    {
        public string Identifier { get; set; }
    }
}