using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    private Animation loadingAnimator;
    /// <summary>
    /// Flaga określająca, czy ekran ładowania nadal wsuwa się na ekran
    /// </summary>
    public bool IsLoading { get; private set; }
    /// <summary>
    /// Akcje wywoływane gdy ekran ładowania jest w pełni wsunięty
    /// </summary>
    public Action onLoadingInMiddle;

    private void OnEnable()
    {
        loadingAnimator = GetComponent<Animation>();
        IsLoading = false;
    }

    /// <summary>
    /// Włącza ekran ładowania
    /// </summary>
    public void StartLoading()
    {
        IsLoading = true;
        loadingAnimator.Play("LeftToMiddle");
    }

    /// <summary>
    /// Wyłącza ekran ładowania
    /// </summary>
    public void EndLoading()
    {
        if (!IsLoading)
            loadingAnimator.Play("MiddleToRight");
        else
            onLoadingInMiddle += delegate { loadingAnimator.Play("MiddleToRight"); };
    }

    /// <summary>
    /// Wywołuje się, gdy animacja wchodzenia panelu ładowania zakończyła się
    /// </summary>
    public void OnLoadingEnded()
    {
        IsLoading = false;
        onLoadingInMiddle?.Invoke();
        onLoadingInMiddle = null;
    }
}
