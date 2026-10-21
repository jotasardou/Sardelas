using System.Collections.Generic;
using UnityEngine;

namespace NeuroRun
{
    // O mesmo mapa orienta paredes, movimento e busca de caminhos. Sem caminhos invisíveis.
    public class MazeGrid
    {
        public const int Width = 31, Height = 17;
        public const float Tile = .6f;
        public readonly bool[,] open = new bool[Width, Height];
        public readonly List<Vector2Int> cells = new List<Vector2Int>();
        public static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        public Vector2Int Spawn => new Vector2Int(15, 15);
        public Vector2Int Terminal => new Vector2Int(15, 8);
        public MazeGrid(int sector)
        {
            int[] columns = { 1, 5, 9, 15, 21, 25, 29 };
            int[] rows = { 1, 5, 8, 11, 15 };
            foreach (int y in rows) for (int x = 1; x < Width - 1; x++) open[x, y] = true;
            foreach (int x in columns) for (int y = 1; y < Height - 1; y++) open[x, y] = true;
            // Trechos fechados obrigam a contornar os blocos, mas preservam as rotas alternativas.
            CloseHorizontal(sector % 2 == 0 ? 5 : 11, 10, 14);
            CloseHorizontal(sector % 2 == 0 ? 11 : 5, 16, 20);
            CloseVertical(sector % 2 == 0 ? 5 : 25, 6, 7);
            CloseVertical(sector % 2 == 0 ? 25 : 5, 9, 10);
            if (sector >= 2) CloseHorizontal(8, sector % 2 == 0 ? 2 : 26, sector % 2 == 0 ? 4 : 28);
            if (sector >= 4) CloseVertical(sector % 2 == 0 ? 9 : 21, 12, 14);
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                if (open[x, y]) cells.Add(new Vector2Int(x, y));
        }
        void CloseHorizontal(int y, int a, int b) { for (int x = a; x <= b; x++) open[x, y] = false; }
        void CloseVertical(int x, int a, int b) { for (int y = a; y <= b; y++) open[x, y] = false; }
        public bool Walkable(Vector2Int p) => p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height && open[p.x, p.y];
        public Vector2 Position(Vector2Int p) => new Vector2((p.x - 15) * Tile, (8 - p.y) * Tile);
        public Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.RoundToInt(p.x / Tile) + 15, 8 - Mathf.RoundToInt(p.y / Tile));
        public Vector2Int Nearest(Vector2Int target)
        {
            if (Walkable(target)) return target;
            var best = cells[0]; int distance = int.MaxValue;
            foreach (var cell in cells) { int d = Mathf.Abs(cell.x - target.x) + Mathf.Abs(cell.y - target.y); if (d < distance) { distance = d; best = cell; } }
            return best;
        }
        // Busca em largura: encontra a rota mais curta respeitando todas as paredes.
        public Vector2Int NextStep(Vector2Int start, Vector2Int goal, Vector2Int previous, int order = 0)
        {
            goal = Nearest(goal);
            var distance = new int[Width, Height];
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) distance[x, y] = -1;
            var queue = new Queue<Vector2Int>(); queue.Enqueue(goal); distance[goal.x, goal.y] = 0;
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var d in Directions)
                {
                    var n = p + d;
                    if (!Walkable(n) || distance[n.x, n.y] >= 0) continue;
                    distance[n.x, n.y] = distance[p.x, p.y] + 1; queue.Enqueue(n);
                }
            }
            var best = start; int bestDistance = int.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                var n = start + Directions[(i + order) % 4];
                if (!Walkable(n) || n == previous || distance[n.x, n.y] < 0) continue;
                if (distance[n.x, n.y] < bestDistance) { bestDistance = distance[n.x, n.y]; best = n; }
            }
            if (best == start && Walkable(previous)) return previous;
            return best;
        }
    }
}
