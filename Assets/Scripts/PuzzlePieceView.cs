using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RebuildHighSchool
{
    public sealed class PuzzlePieceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private MemoryPuzzleGame owner;
        private RectTransform rect;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Image image;
        private Vector2 targetPosition;
        private Vector2 targetSize;
        private bool dragging;

        public bool Placed { get; private set; }
        public RectTransform Rect => rect;
        public Vector2 TargetPosition => targetPosition;
        public Vector2 TargetSize => targetSize;

        public void Configure(MemoryPuzzleGame game, Canvas rootCanvas, Vector2 destination, Vector2 placedSize)
        {
            owner = game;
            canvas = rootCanvas;
            rect = (RectTransform)transform;
            targetPosition = destination;
            targetSize = placedSize;
            canvasGroup = GetComponent<CanvasGroup>();
            image = GetComponent<Image>();
            SetSelected(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Placed) return;
            dragging = true;
            owner.SelectPiece(this);
            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
            transform.localScale = Vector3.one * 1.05f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Placed) return;
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Placed) return;
            dragging = false;
            canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;
            owner.TryPlace(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Placed && !dragging) owner.SelectPiece(this);
        }

        public void RotateClockwise()
        {
            if (Placed) return;
            float next = Mathf.Round(transform.localEulerAngles.z / 90f) * 90f - 90f;
            transform.localEulerAngles = new Vector3(0, 0, next);
            owner.TryPlace(this);
        }

        public void Place()
        {
            Placed = true;
            rect.anchoredPosition = targetPosition;
            rect.sizeDelta = targetSize;
            rect.localEulerAngles = Vector3.zero;
            rect.localScale = Vector3.one;
            canvasGroup.blocksRaycasts = false;
            image.color = Color.white;
        }

        public void SetSelected(bool selected)
        {
            if (image == null) return;
            image.color = selected ? new Color(1f, .91f, .70f, 1f) : Color.white;
        }
    }
}
