using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class User   // было internal, стало public
    {
        public int Id { get; set; }      // добавили
        public string Name { get; set; } // добавили
    }
}
