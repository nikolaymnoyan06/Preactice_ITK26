﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) для представления ребра графа.
    /// Используется для передачи данных о связи между узлами.
    /// </summary>
    public class EdgeDto
    {
        // Уникальный идентификатор ребра.
        public int Id { get; set; }

        // Идентификатор исходного узла (откуда исходит ребро).
        public int SourceId { get; set; }

        // Идентификатор целевого узла (куда направлено ребро).
        public int TargetId { get; set; }

        // Вес ребра (например, расстояние, стоимость или пропускная способность).
        public double Weight { get; set; }

        /// <summary>
        /// Возвращает строковое представление ребра в формате: "SourceId -> TargetId (вес: Weight)".
        /// </summary>
        public override string ToString()
        {
            return $"{SourceId} -> {TargetId} (вес: {Weight})";
        }
    }
}
