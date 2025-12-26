using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;

namespace Rumbax.Gameplay.Grid
{
    /// <summary>
    /// Manages the game grid where defenders are placed and merged.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 5;
        [SerializeField] private float cellSize = 1.5f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;
        
        [Header("Visual")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform gridParent;
        
        private GridCell[,] _grid;
        private List<GridCell> _emptyCells = new List<GridCell>();
        private GridCell _selectedCell;
        private GridCell _dragTargetCell;
        
        public int Width => gridWidth;
        public int Height => gridHeight;
        public int TotalCells => gridWidth * gridHeight;
        public int OccupiedCells => TotalCells - _emptyCells.Count;
        public bool HasEmptyCell => _emptyCells.Count > 0;

        private void Awake()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[gridWidth, gridHeight];
            _emptyCells.Clear();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 position = GetWorldPosition(x, y);
                    
                    GameObject cellObj;
                    if (cellPrefab != null)
                    {
                        cellObj = Instantiate(cellPrefab, position, Quaternion.identity, gridParent);
                    }
                    else
                    {
                        cellObj = new GameObject($"Cell_{x}_{y}");
                        cellObj.transform.position = position;
                        cellObj.transform.SetParent(gridParent);
                    }

                    var cell = cellObj.GetComponent<GridCell>();
                    if (cell == null)
                    {
                        cell = cellObj.AddComponent<GridCell>();
                    }

                    cell.Initialize(x, y, this);
                    _grid[x, y] = cell;
                    _emptyCells.Add(cell);
                }
            }

            Debug.Log($"[GridManager] Grid initialized: {gridWidth}x{gridHeight}");
        }

        /// <summary>
        /// Convert grid coordinates to world position.
        /// </summary>
        public Vector3 GetWorldPosition(int x, int y)
        {
            float worldX = (x - gridWidth / 2f + 0.5f) * cellSize + gridOffset.x;
            float worldY = (y - gridHeight / 2f + 0.5f) * cellSize + gridOffset.y;
            return new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - gridOffset.x) / cellSize + gridWidth / 2f);
            int y = Mathf.FloorToInt((worldPosition.y - gridOffset.y) / cellSize + gridHeight / 2f);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Get cell at specific grid coordinates.
        /// </summary>
        public GridCell GetCell(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                return _grid[x, y];
            }
            return null;
        }

        /// <summary>
        /// Get cell at world position.
        /// </summary>
        public GridCell GetCellAtPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = GetGridPosition(worldPosition);
            return GetCell(gridPos.x, gridPos.y);
        }

        /// <summary>
        /// Get a random empty cell.
        /// </summary>
        public GridCell GetRandomEmptyCell()
        {
            if (_emptyCells.Count == 0) return null;
            
            int index = Random.Range(0, _emptyCells.Count);
            return _emptyCells[index];
        }

        /// <summary>
        /// Mark cell as occupied.
        /// </summary>
        public void SetCellOccupied(GridCell cell, bool occupied)
        {
            if (cell == null) return;

            if (occupied)
            {
                _emptyCells.Remove(cell);
            }
            else if (!_emptyCells.Contains(cell))
            {
                _emptyCells.Add(cell);
            }
        }

        /// <summary>
        /// Try to merge two cells with defenders.
        /// </summary>
        public bool TryMerge(GridCell sourceCell, GridCell targetCell)
        {
            if (sourceCell == null || targetCell == null) return false;
            if (sourceCell == targetCell) return false;
            if (!sourceCell.HasDefender || !targetCell.HasDefender) return false;

            var sourceDefender = sourceCell.CurrentDefender;
            var targetDefender = targetCell.CurrentDefender;

            // Check if merge is possible
            if (!CanMerge(sourceDefender, targetDefender)) return false;

            // Perform merge
            int newLevel = sourceDefender.Level + 1;
            string defenderType = sourceDefender.DefenderType;

            // Remove source defender
            sourceCell.RemoveDefender();
            SetCellOccupied(sourceCell, false);

            // Upgrade target defender
            targetDefender.SetLevel(newLevel);

            // Publish event
            ServiceLocator.Get<IEventBus>()?.Publish(new DefenderMergedEvent(defenderType, newLevel));

            Debug.Log($"[GridManager] Merged defenders to level {newLevel}");
            return true;
        }

        /// <summary>
        /// Check if two defenders can be merged.
        /// </summary>
        public bool CanMerge(Defenders.Defender source, Defenders.Defender target)
        {
            if (source == null || target == null) return false;
            if (source.DefenderType != target.DefenderType) return false;
            if (source.Level != target.Level) return false;
            
            var gameManager = GameManager.Instance;
            int maxLevel = gameManager?.Config?.MaxMergeLevel ?? 15;
            
            return source.Level < maxLevel;
        }

        /// <summary>
        /// Select a cell for interaction.
        /// </summary>
        public void SelectCell(GridCell cell)
        {
            if (_selectedCell != null)
            {
                _selectedCell.SetSelected(false);
            }

            _selectedCell = cell;
            
            if (_selectedCell != null)
            {
                _selectedCell.SetSelected(true);
            }
        }

        /// <summary>
        /// Start dragging from a cell.
        /// </summary>
        public void StartDrag(GridCell cell)
        {
            if (cell == null || !cell.HasDefender) return;
            
            _selectedCell = cell;
            cell.StartDrag();
        }

        /// <summary>
        /// Update drag target cell.
        /// </summary>
        public void UpdateDragTarget(Vector3 worldPosition)
        {
            var cell = GetCellAtPosition(worldPosition);
            
            if (_dragTargetCell != cell)
            {
                _dragTargetCell?.SetHighlighted(false);
                _dragTargetCell = cell;
                _dragTargetCell?.SetHighlighted(true);
            }
        }

        /// <summary>
        /// End drag operation.
        /// </summary>
        public void EndDrag(Vector3 worldPosition)
        {
            if (_selectedCell == null) return;

            var targetCell = GetCellAtPosition(worldPosition);
            
            if (targetCell != null && targetCell != _selectedCell)
            {
                if (targetCell.HasDefender)
                {
                    // Try merge
                    if (!TryMerge(_selectedCell, targetCell))
                    {
                        // Swap positions
                        SwapDefenders(_selectedCell, targetCell);
                    }
                }
                else
                {
                    // Move to empty cell
                    MoveDefender(_selectedCell, targetCell);
                }
            }

            _selectedCell?.EndDrag();
            _selectedCell = null;
            _dragTargetCell?.SetHighlighted(false);
            _dragTargetCell = null;
        }

        /// <summary>
        /// Swap defenders between two cells.
        /// </summary>
        private void SwapDefenders(GridCell cellA, GridCell cellB)
        {
            var defenderA = cellA.CurrentDefender;
            var defenderB = cellB.CurrentDefender;

            cellA.SetDefender(defenderB);
            cellB.SetDefender(defenderA);

            defenderA?.MoveTo(cellB.transform.position);
            defenderB?.MoveTo(cellA.transform.position);
        }

        /// <summary>
        /// Move defender from one cell to another.
        /// </summary>
        private void MoveDefender(GridCell from, GridCell to)
        {
            var defender = from.CurrentDefender;
            if (defender == null) return;

            from.RemoveDefender();
            to.SetDefender(defender);
            defender.MoveTo(to.transform.position);

            SetCellOccupied(from, false);
            SetCellOccupied(to, true);
        }

        /// <summary>
        /// Get all defenders on the grid.
        /// </summary>
        public List<Defenders.Defender> GetAllDefenders()
        {
            var defenders = new List<Defenders.Defender>();
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_grid[x, y].HasDefender)
                    {
                        defenders.Add(_grid[x, y].CurrentDefender);
                    }
                }
            }
            
            return defenders;
        }

        /// <summary>
        /// Clear all defenders from the grid.
        /// </summary>
        public void ClearGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_grid[x, y].HasDefender)
                    {
                        _grid[x, y].RemoveDefender();
                    }
                    SetCellOccupied(_grid[x, y], false);
                }
            }
        }
    }
}
