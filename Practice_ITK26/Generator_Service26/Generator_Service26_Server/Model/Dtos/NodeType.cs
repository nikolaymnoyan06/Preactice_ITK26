using System;
using System.Collections.Generic;
using System.Text;

namespace Generator_Service26_Server.Model.Dtos
{
    /// <summary>Типы узлов графа.</summary>
    public enum NodeType
    {
        // Узел-потребитель данных.
        Consumer,
        // Узел-источник данных.
        Source,
        // Транзитный (промежуточный) узел.
        Transitive
    }
}