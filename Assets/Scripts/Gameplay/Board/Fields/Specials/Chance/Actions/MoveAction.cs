
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

    public MoveAction(Mode mode, int byAmount, ToTarget toTarget, int targetId, PlaceTypeTarget targetType)
    {
        this.mode = mode;
        this.byAmount = byAmount;
        this.toTarget = toTarget;
        this.targetId = targetId;
        this.targetType = targetType;
    }

    public override void Call(Player caller)
    {
        switch (mode)
        {
            case Mode.By:
                MoveBy(caller);
                break;
            case Mode.To:
                MoveTo(caller);
                break;
        }
    }

    private void MoveBy(Player caller)
    {
        Board board = GameplayController.instance.board;

        if (byAmount > 0)
            board.MovePlayer(caller, byAmount);
        else
            board.TeleportPlayer(caller, caller.PlaceId + byAmount);
    }

    private void MoveTo(Player caller)
    {
        switch (toTarget)
        {
            case ToTarget.PlaceId:
                TargetPlaceId(caller);
                break;
            case ToTarget.PlaceType:
                TargetPlaceType(caller);
                break;
        }
    }

    private void TargetPlaceId(Player caller)
    {
        Board board = GameplayController.instance.board;

        int moveAmount = board.GetPlacesDistance(caller.PlaceId, targetId);
        board.MovePlayer(caller, moveAmount);
    }

    private void TargetPlaceType(Player caller)
    {
        Debug.LogError("Brak obsługi przesówania do pola o podanym typie");
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
        Start
    }
}