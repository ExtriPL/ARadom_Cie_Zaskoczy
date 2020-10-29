using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PopupBox : MonoBehaviour, IAnimable
{
    public Popup source;
    public Animator boxAnimator;
    protected PopupSystem pSystem = PopupSystem.instance;
    public int CurrentPosition { get => boxAnimator.GetInteger("currentPosition"); }

    protected virtual void Start()
    {
        boxAnimator = GetComponent<Animator>();
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

        if (boxAnimator == null)
            Start();
    }

    /// <summary>
    /// Deinicjalizacja Popupbox-u gdy nie jest już potrzebny
    /// </summary>
    public virtual void Deinit()
    {
        source?.ClearDelegates();
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
        boxAnimator.SetTrigger("Hide");
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

    public virtual void OnCloseAnimationStart() {}

    public virtual void OnShowAnimationStart() {}
}