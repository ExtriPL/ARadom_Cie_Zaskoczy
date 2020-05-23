using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PopupBox : MonoBehaviour
{
    public Popup source;
    protected PopupSystem pSystem = PopupSystem.instance;
    protected float lifeStartTime;

    protected virtual void Update()
    {
        if (source != null && Time.time - lifeStartTime >= source.lifeSpan) Close();
    }

    /// <summary>
    /// Inicjuje box-a zawierającego popup o podanym typie
    /// </summary>
    /// <param name="source">Popup wyświetlany przez box-a</param>
    public virtual void Init(Popup source)
    {
        this.source = source;
        source.onOpen?.Invoke(source);
        lifeStartTime = Time.time;
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
        source.onClose?.Invoke(source);
        pSystem.ClosePopup(this);
    }
}