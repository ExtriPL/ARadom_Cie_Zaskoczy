using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Church", menuName = "ARadom/Field/Building/Stacking/Church")]
public class ChurchStacking : StackingBuilding
{
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

}
