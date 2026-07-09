using System;
using System.Collections.Generic;
using System.Text;

namespace Generator_Service26.Model.Dtos
{
    public class EdgeDto
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public double Weight { get; set; }
    }
}
