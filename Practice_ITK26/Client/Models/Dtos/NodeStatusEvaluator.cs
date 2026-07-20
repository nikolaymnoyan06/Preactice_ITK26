using Client.Dtos;
using Client.Models.Dtos;
using System;

namespace Client.Models
{
    /// <summary>
    /// Отвечает за вычисление статуса узла на основе его типа и значений In/Out.
    /// </summary>
    public static class NodeStatusEvaluator
    {
        // Допустимая погрешность для сравнения double (чтобы 100.0000001 и 100 считались равными)
        private const double Epsilon = 0.0000001;

        /// <summary>
        /// Вычисляет статус узла.
        /// </summary>
        /// <returns>Строка с эмодзи и статусом (OK, NOT OK, NOT STATED)</returns>
        public static string EvaluateStatus(NodeDto node)
        {
            if (node == null) return "❓ NOT STATED";

            // Проверяем, заданы ли значения. 
            // Если оба значения равны 0 (или очень близки к 0), считаем, что данные не заданы.
            if (Math.Abs(node.InValue) < Epsilon && Math.Abs(node.OutValue) < Epsilon)
            {
                return "❓ NOT STATED";
            }

            // Логика по типам
            switch (node.Type)
            {
                case NodeType.Source:
                    // Источник: Выходящее >= Входящее
                    return node.OutValue >= node.InValue - Epsilon ? "✅ OK" : "❌ NOT OK";

                case NodeType.Consumer:
                    // Потребитель: Входящее >= Выходящее
                    return node.InValue >= node.OutValue - Epsilon ? "✅ OK" : "❌ NOT OK";

                case NodeType.Transitive:
                    // Транзитивный: значения равны (с учетом погрешности)
                    return Math.Abs(node.InValue - node.OutValue) < Epsilon ? "✅ OK" : "❌ NOT OK";

                default:
                    return "❓ NOT STATED";
            }
        }
    }
}