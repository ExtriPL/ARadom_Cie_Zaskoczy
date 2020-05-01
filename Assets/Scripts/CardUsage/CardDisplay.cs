using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    public Card card;

    public Image image;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private int originalSibling;
    private Vector3 originalPosition;
    private bool isDragged = false;

    private void Start()
    {
        loadCardSettings();
    }

    private void loadCardSettings()
    {
        image.sprite = card.logo;
        nameText.text = card.name;
        descriptionText.text = card.descripition;
        originalSibling = transform.GetSiblingIndex();
        originalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localPosition = new Vector3(originalPosition.x, 300, 0);
        transform.SetAsLastSibling();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.SetSiblingIndex(originalSibling);
        if (!isDragged)
        {
            transform.localPosition = new Vector3(originalPosition.x, 0, 0);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;

        isDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.position.y > Screen.height/3)
        {
            //TODO: Logika użycia karty
            Destroy(gameObject);
        }
        
        transform.SetSiblingIndex(originalSibling);
        transform.localPosition = originalPosition;

        isDragged = false;
    }
}