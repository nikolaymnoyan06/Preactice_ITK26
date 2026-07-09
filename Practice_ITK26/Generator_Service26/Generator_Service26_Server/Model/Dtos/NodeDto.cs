using System;
using System.Collections.Generic;
using System.Text;

namespace Generator_Service26.Model.Dtos
{
    public class NodeDto
    {
        public int Id { get; set; }
        public object Value { get; set; } // может быть строка, число и т.д.
    }
}
