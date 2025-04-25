using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This enum defines the variable type eCardState with four named values.      // a
public enum eCardState { drawpile, mine, target, discard }

public class CardProspector : Card
{ // Make CardProspector extend Card        // b
    [Header("Dynamic: CardProspector")]
    public eCardState state = eCardState.drawpile;                   // c
                                                                     // The hiddenBy list stores which other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // The layoutID matches this card to the tableau JSON if it’s a tableau card
    public int layoutID;
    // The JsonLayoutSlot class stores information pulled in from JSON_Layout
    public JsonLayoutSlot layoutSlot;

    override public void OnMouseUpAsButton(){
        Prospector.CARD_CLICKED(this);
    }
}
