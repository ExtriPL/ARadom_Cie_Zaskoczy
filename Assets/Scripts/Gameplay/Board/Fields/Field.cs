using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field : ScriptableObject
{
    /// <summary>
    /// Nazwa pola
    /// </summary>
    [SerializeField, Tooltip("Nazwa pola")]
    protected string fieldName;
    /// <summary>
    /// Określa, czy dany budynek może znaleźć się na mapie
    /// </summary>
    [SerializeField, Tooltip("Czy dany budynek może zostać umieszczony na mapie")]
    protected bool canBePlaced = true;

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz wejdzie na pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnEnter(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz zejdzie z pola
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnLeave(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane, gdy gracz przejdzie przez pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnPassby(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane gdy gracz rozpocznie rundę na danym polu
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    /// <param name="visualiser">Instancja pola</param>
    public virtual void OnAwake(Player player, PlaceVisualiser visualiser) 
    { 
        if(player.NetworkPlayer.IsLocal)
        {
            Board board = GameplayController.instance.board;

            QuestionPopup startTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_STARTED"));
            startTurn.AddButton("Ok", Popup.Functionality.Destroy(startTurn));

            IconPopup dice = new IconPopup(IconPopupType.None);
            dice.onClick += Popup.Functionality.Destroy(dice);
            Popup.PopupAction rolldice = delegate (Popup source)
            {
                int firstThrow = board.dice.last1;
                int secondThrow = board.dice.last2;
                InfoPopup rollResult = new InfoPopup(SettingsController.instance.languageController.GetWord("YOU_GOT") + firstThrow + SettingsController.instance.languageController.GetWord("AND") + secondThrow, 1.5f);
                PopupSystem.instance.AddPopup(rollResult);
                board.MovePlayer(player, firstThrow + secondThrow);
            };
            dice.onClose += rolldice;
            startTurn.onClose += Popup.Functionality.Show(dice);

            PopupSystem.instance.AddPopup(startTurn);
        }
    }

    /// <summary>
    /// Zdarzenie wywoływane gdy gracz zakończy rundę na danym polu
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    /// <param name="visualiser">Instancja pola</param>
    public virtual void OnEnd(Player player, PlaceVisualiser visualiser)
    {
        if (player.NetworkPlayer.IsLocal)
        {
            QuestionPopup endTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_ENDED"));
            endTurn.AddButton("Ok", Popup.Functionality.Destroy(endTurn));
            PopupSystem.instance.AddPopup(endTurn);
        }
    }

    /// <summary>
    /// Funkcja zwraca model domyślny stojący na danym polu
    /// </summary>
    /// <returns>Model domyślny stojący na danym polu</returns>
    public abstract GameObject GetStartModel();

    /// <summary>
    /// Określa, czy dany budynek może znaleźć się na mapie
    /// </summary>
    public bool CanBePlaced() => canBePlaced;

    /// <summary>
    /// Zwraca nazwę pola
    /// </summary>
    /// <returns>Nazwa pola</returns>
    public string GetFieldName() => fieldName;
}
