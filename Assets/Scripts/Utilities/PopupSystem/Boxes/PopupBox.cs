using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PopupBox : MonoBehaviour
{
    public Popup source;
    public Animation animations;
    protected abstract Action CloseAnimationTrigger { get; }
    protected PopupSystem pSystem = PopupSystem.instance;

    protected virtual void Start()
    {
        animations = GetComponent<Animation>();
    }

    public virtual void InitBox() {}

    /// <summary>
    /// Inicjuje box-a zawierającego popup o podanym typie
    /// </summary>
    /// <param name="source">Popup wyświetlany przez box-a</param>
    public virtual void Init(Popup source)
    {
        this.source = source;
        source.onOpen?.Invoke(source);

        if (animations == null)
            Start();
    }

    /// <summary>
    /// Deinicjalizacja Popupbox-u gdy nie jest już potrzebny
    /// </summary>
    public virtual void Deinit()
    {
        source.ClearDelegates();
        source = null;
    }

    /// <summary>
    /// Akcja wywoływana po kliknięciu Popupbox-u
    /// </summary>
    public virtual void Click()
    {
        source.onClick?.Invoke(source);
    }

    /// <summary>
    /// Metoda zamykająca Popupbox
    /// </summary>
    public virtual void Close()
    {
        if (CloseAnimationTrigger != null)
            CloseAnimationTrigger.Invoke();
        else
            OnCloseAnimationEnd();
    }

    public virtual void OnCloseAnimationEnd()
    {
        source.onClose?.Invoke(source);
        pSystem.ClosePopup(this);
    }

    public virtual void OnShowAnimationEnd() {}

    /// <summary>
    /// Zmienianie obecnej pozycji boxu na prawidłową, wynikającą z obecnej ilości boxów danego typu na ekranie
    /// </summary>
    public virtual void Reposition() { }
}