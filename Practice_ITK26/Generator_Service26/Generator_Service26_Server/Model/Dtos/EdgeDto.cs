using System;
using System.Collections.Generic;
using System.Text;

namespace Generator_Service26.Model.Dtos
{
    /// <summary>DTO для передачи данных о ребре графа.</summary>
    public class EdgeDto
    {
        // Уникальный идентификатор ребра.
        public int Id { get; set; }

        // ID узла, из которого исходит ребро.
        public int SourceId { get; set; }

        // ID узла, в которое входит ребро.
        public int TargetId { get; set; }

        // Вес (или стоимость) ребра.
        public double Weight { get; set; }
    }
}