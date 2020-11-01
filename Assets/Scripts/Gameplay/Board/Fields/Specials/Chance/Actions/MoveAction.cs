
using UnityEngine;

public class MoveAction : ActionCard
{
    /// <summary>
    /// Tryb ruchu, jaki zostanie wykonany po wywołaniu akcji
    /// </summary>
    public Mode mode;
    /// <summary>
    /// Ilość pól, o jakie gracz zostanie przesunięty, gdy mode = Mode.By
    /// </summary>
    public int byAmount;

    public ToTarget toTarget;
    public int targetId;
    public PlaceTypeTarget targetType;
    public MovementType movementType;

    public MoveAction(Mode mode, int byAmount, ToTarget toTarget, int targetId, PlaceTypeTarget targetType, MovementType movementType)
    {
        this.mode = mode;
        this.byAmount = byAmount;
        this.toTarget = toTarget;
        this.targetId = targetId;
        this.targetType = targetType;
        this.movementType = movementType;
    }

    public override void Call(Player caller, bool showMessage = false)
    {
        switch (mode)
        {
            case Mode.By:
                MoveBy(caller, showMessage);
                break;
            case Mode.To:
                MoveTo(caller, showMessage);
                break;
        }
    }

    private void MoveBy(Player caller, bool showMessage)
    {
        Board board = GameplayController.instance.board;

        if (byAmount > 0)
            board.MovePlayer(caller, byAmount);
        else
            board.TeleportPlayer(caller, caller.PlaceId + byAmount);
    }

    private void MoveTo(Player caller, bool showMessage)
    {
        switch (toTarget)
        {
            case ToTarget.PlaceId:
                TargetPlaceId(caller, showMessage);
                break;
            case ToTarget.PlaceType:
                TargetPlaceType(caller, showMessage);
                break;
        }
    }

    private void TargetPlaceId(Player caller, bool showMessage)
    {
        Board board = GameplayController.instance.board;

        int moveAmount = board.GetPlacesDistance(caller.PlaceId, targetId);
        board.MovePlayer(caller, moveAmount);

        if (showMessage)
            ShowMessage(caller);
    }

    private void TargetPlaceType(Player caller, bool showMessage)
    {
        targetId = GetPlaceId(targetType);

        if (movementType == MovementType.Regular)
            TargetPlaceId(caller, showMessage);
        else if(movementType == MovementType.Teleport)
        {
            Board board = GameplayController.instance.board;
            board.TeleportPlayer(caller, targetId);

            if (showMessage)
                ShowMessage(caller);
        }
    }

    private void ShowMessage(Player target)
    {
        string[] message = new string[] { lang.PackKey("YOU_ARE_MOVING_TO_PLACE"), targetId.ToString() };
        EventManager.instance.SendPopupMessage(message, IconPopupType.Message, target);
    }

    private static int GetPlaceId(PlaceTypeTarget targetType)
    {
        Board board = GameplayController.instance.board;

        switch(targetType)
        {
            case PlaceTypeTarget.Prison:
                return board.GetPlaceIndex(typeof(PrisonSpecial));
            case PlaceTypeTarget.Start:
                return board.GetPlaceIndex(typeof(StartSpecial));
            case PlaceTypeTarget.Chance:
                return board.GetPlaceIndex(typeof(ChanceSpecial));
            default:
                return 0;
        }
    }

    public enum Mode
    {
        /// <summary>
        /// Ruch o określoną liczbę pól
        /// </summary>
        By,
        /// <summary>
        /// Ruch do określonego pola
        /// </summary>
        To
    }

    public enum ToTarget
    {
        PlaceId,
        PlaceType
    }

    public enum PlaceTypeTarget
    {
        Prison,
        Start,
        Chance
    }

    public enum MovementType
    {
        /// <summary>
        /// Normalne poruszenie graczem. Gracz przechodzi nad wszystkimi polami pomiędzy swoim położeniem i docelowym położeniem
        /// </summary>
        Regular,
        /// <summary>
        /// Bezpośrednie przeniesienie gracza na lokalizację docelową
        /// </summary>
        Teleport
    }
}