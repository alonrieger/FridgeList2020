using System;
using System.Collections.Generic;
using System.Text;

namespace CounterApp
{
    public class ProductInFridge
    {
        public string Name { get; set; }
        public string Count { get; set; }

        public ProductInFridge(string N, string C)
        {
            Name = N;
            Count = C;
        }
    }
    
}
