using UnityEngine;
using Rumbax.Gameplay.Defenders;

namespace Rumbax.Gameplay.Grid
{
    /// <summary>
    /// Represents a single cell in the game grid.
    /// </summary>
    public class GridCell : MonoBehaviour
    {
        [Header("Visual States")]
        [SerializeField] private SpriteRenderer cellRenderer;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.6f, 1f, 0.7f);
        [SerializeField] private Color highlightColor = new Color(0.3f, 1f, 0.3f, 0.7f);
        [SerializeField] private Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        
        public int X { get; private set; }
        public int Y { get; private set; }
        public GridManager Grid { get; private set; }
        public Defender CurrentDefender { get; private set; }
        public bool HasDefender => CurrentDefender != null;
        public bool IsSelected { get; private set; }
        public bool IsHighlighted { get; private set; }

        private void Awake()
        {
            if (cellRenderer == null)
            {
                cellRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Initialize the cell with grid coordinates.
        /// </summary>
        public void Initialize(int x, int y, GridManager grid)
        {
            X = x;
            Y = y;
            Grid = grid;
            gameObject.name = $"Cell_{x}_{y}";
            
            UpdateVisual();
        }

        /// <summary>
        /// Set a defender on this cell.
        /// </summary>
        public void SetDefender(Defender defender)
        {
            CurrentDefender = defender;
            
            if (defender != null)
            {
                defender.SetCell(this);
            }
        }

        /// <summary>
        /// Remove defender from this cell.
        /// </summary>
        public void RemoveDefender()
        {
            if (CurrentDefender != null)
            {
                CurrentDefender.SetCell(null);
                CurrentDefender = null;
            }
        }

        /// <summary>
        /// Set selected state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisual();
        }

        /// <summary>
        /// Set highlighted state (drag target).
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
            UpdateVisual();
        }

        /// <summary>
        /// Start drag from this cell.
        /// </summary>
        public void StartDrag()
        {
            if (CurrentDefender != null)
            {
                CurrentDefender.StartDrag();
            }
            SetSelected(true);
        }

        /// <summary>
        /// End drag on this cell.
        /// </summary>
        public void EndDrag()
        {
            if (CurrentDefender != null)
            {
                CurrentDefender.EndDrag();
            }
            SetSelected(false);
        }

        /// <summary>
        /// Update visual appearance based on state.
        /// </summary>
        private void UpdateVisual()
        {
            if (cellRenderer == null) return;

            if (IsSelected)
            {
                cellRenderer.color = selectedColor;
            }
            else if (IsHighlighted)
            {
                cellRenderer.color = highlightColor;
            }
            else
            {
                cellRenderer.color = normalColor;
            }
        }

        /// <summary>
        /// Show invalid merge indicator.
        /// </summary>
        public void ShowInvalid()
        {
            if (cellRenderer != null)
            {
                cellRenderer.color = invalidColor;
            }
        }

        private void OnMouseDown()
        {
            Grid?.StartDrag(this);
        }

        private void OnMouseUp()
        {
            Grid?.EndDrag(transform.position);
        }

        private void OnMouseDrag()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Grid?.UpdateDragTarget(mousePos);
            
            if (CurrentDefender != null)
            {
                CurrentDefender.UpdateDragPosition(mousePos);
            }
        }
    }
}
