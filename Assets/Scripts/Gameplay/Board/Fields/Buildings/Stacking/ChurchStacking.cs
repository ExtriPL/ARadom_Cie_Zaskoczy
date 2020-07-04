using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Church", menuName = "ARadom/Field/Building/Stacking/Church")]
public class ChurchStacking : StackingBuilding
{
    ChurchStacking()
    {
        //Ustawienie typu budynku stackującego
        stackingType = StackingBuildingType.Church;
    }

    public override void OnPlayerEnter(Player player, PlaceVisualiser visualiser)
    {
        //throw new NotImplementedException();
        base.OnPlayerEnter(player, visualiser);
    }

    public override void OnPlayerLeave(Player player, PlaceVisualiser visualiser)
    {
        //throw new NotImplementedException();
    }

    public override void OnPlayerPassby(Player player, PlaceVisualiser visualiser)
    {
        //throw new NotImplementedException();
    }

    public override void OnBuyBuilding(Player player, PlaceVisualiser visualiser)
    {
        base.OnBuyBuilding(player, visualiser);
    }

    public override void OnSellBuilding(Player player, PlaceVisualiser visualiser)
    {
        base.OnSellBuilding(player, visualiser);
    }

    public override void OnSameGroupBuy(Player player, PlaceVisualiser visualiser)
    {
        //throw new NotImplementedException();
    }

    public override void OnSameGroupSell(Player player, PlaceVisualiser visualiser)
    {
        //throw new NotImplementedException();
    }

    public override List<string> GetFieldInfo()
    {
        List<string> info = new List<string>
        {
            "jestem kościołem"
        };

        int index = GameplayController.instance.board.GetPlaceIndex(this);
        if (GameplayController.instance.board.GetOwner(index) != null)
        {
            info.Add(GameplayController.instance.board.GetOwner(index).GetName());
        }
        else
        {
            info.Add("Budynek nie ma właściciela!");
        }


        return info;
    }
}
