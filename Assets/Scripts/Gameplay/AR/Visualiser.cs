using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract class Visualiser : MonoBehaviour, IEventSubscribable, IAnimable
{
    protected ARController ARController;

    /// <summary>
    /// Flaga określająca, czy pole jest widoczne
    /// </summary>
    protected bool visible;

    /// <summary>
    /// Lista modeli, które mogą być wyświetlane przez Visualiser
    /// </summary>
    protected List<GameObject> models = new List<GameObject>();
    /// <summary>
    /// Numer modelu aktualnie wyświetlanego przez Visualiser
    /// </summary>
    protected int showedModel = 0;

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    public virtual void OnClick() { }

    protected abstract void InitModels();

    #region Animacje

    public virtual void OnCloseAnimationEnd() {}

    public virtual void OnCloseAnimationStart() {}

    public virtual void OnShowAnimationEnd() {}

    public virtual void OnShowAnimationStart() {}

    #endregion Animacje

    #region Zarządzanie wyświetlanym modelem

    /// <summary>
    /// Pokazuje następny model na liście
    /// </summary>
    public void ShowNextModel()
    {
        if (showedModel == models.Count - 1) ShowModel(0);
        else ShowModel(showedModel + 1);
    }

    /// <summary>
    /// Pokazuje wybrany model
    /// </summary>
    /// <param name="id">Numer modelu, który chcemy pokazać</param>
    protected void ShowModel(int id)
    {
        if (id < models.Count)
        {
            models[showedModel]?.SetActive(false);
            showedModel = id;
            if (visible) models[showedModel]?.SetActive(true);
        }
        else Debug.LogError("Podano nieprawidłowy numer modelu!");
    }

    /// <summary>
    /// Ukrywa obecnie wyświetlony model budynku
    /// </summary>
    protected void HideModel()
    {
        models[showedModel].SetActive(false);
    }

    #endregion Zarządzanie wyświetlanym modelem

    public virtual void ToggleVisibility(bool visible)
    {
        if (visible)
            ShowModel(showedModel);
        else
            HideModel();

        this.visible = visible;
    }
}
