using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // We’ll need this line later in the chapter

[RequireComponent(typeof(Deck))]                                              // a
[RequireComponent(typeof(JsonParseLayout))]
public class Prospector : MonoBehaviour
{
    private static Prospector S; // A private Singleton for Prospector

    [Header("Dynamic")]
    public List<CardProspector> drawPile;

    public List<CardProspector> discardPile;
    public List<CardProspector> mine;
    public CardProspector target;

    private Transform layoutAnchor;

    private Deck deck;
    private JsonLayout jsonLayout;

    private Dictionary<int, CardProspector> mineIdToCardDict;
    private Dictionary<string, CardProspector> topCard;

    void Start()
    {
        // Set the private Singleton. We’ll use this later.
        if (S != null) Debug.LogError("Attempted to set S more than once!");  // b
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;

        deck = GetComponent<Deck>();
        // These two lines replace the Start() call we commented out in Deck
        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);

        drawPile = ConvertCardsToCardProspectors(deck.cards);

        LayoutMine();
        MoveToTarget( Draw() );
        UpdateDrawPile();
        UpdateTopCard();
    }

    /// <summary>
    /// Converts each Card in a List(Card) into a List(CardProspector) so that it
    ///  can be used in the Prospector game.
    /// </summary>
    /// <param name="listCard">A List(Card) to be converted</param>
    /// <returns>A List(CardProspector) of the converted cards</returns>
    List<CardProspector> ConvertCardsToCardProspectors(List<Card> listCard)
    {
        List<CardProspector> listCP = new List<CardProspector>();
        CardProspector cp;
        foreach (Card card in listCard)
        {
            cp = card as CardProspector;                                      // c
            listCP.Add(cp);
        }
        return (listCP);
    }

    /// <summary>
    /// Pulls a single card from the beginning of the drawPile and returns it
    /// Note: There is no protection against trying to draw from an empty pile!
    /// </summary>
    /// <returns>The top card of drawPile</returns>
    CardProspector Draw()
    {
        CardProspector cp = drawPile[0]; // Pull the 0th CardProspector
        drawPile.RemoveAt(0);            // Then remove it from drawPile
        return (cp);                      // And return it
    }

    /// <summary>
    /// Positions the initial tableau of cards, a.k.a. the "mine"
    /// </summary>
    void LayoutMine()
    {
        // Create an empty GameObject to serve as an anchor for the tableau   // a
        if (layoutAnchor == null)
        {
            // Create an empty GameObject named _LayoutAnchor in the Hierarchy
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;             // Grab its Transform
        }

        CardProspector cp;     
        
        mineIdToCardDict = new Dictionary<int, CardProspector>();                                              // b

        // Iterate through the JsonLayoutSlots pulled from the JSON_Layout
        foreach (JsonLayoutSlot slot in jsonLayout.slots)
        {
            cp = Draw(); // Pull a card from the top (beginning) of the draw Pile
            cp.faceUp = slot.faceUp;    // Set its faceUp to the value in SlotDef
                                        // Make the CardProspector a child of layoutAnchor
            cp.transform.SetParent(layoutAnchor);

            // Convert the last char of the layer string to an int (e.g. "Row 0")
            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());  // c

            // Set the localPosition of the card based on the slot information
            cp.SetLocalPos(new Vector3(
            jsonLayout.multiplier.x * slot.x,
            jsonLayout.multiplier.y * slot.y,
            -z));                                                       // d

            cp.layoutID = slot.id;
            cp.layoutSlot = slot;
            // CardProspectors in the mine have the state CardState.mine
            cp.state = eCardState.mine;

            // Set the sorting layer of all SpriteRenderers on the Card
            cp.SetSpriteSortingLayer(slot.layer);

            mine.Add(cp); // Add this CardProspector to the List<mine

            mineIdToCardDict.Add(slot.id, cp);
        }
    }

    /// <summary>
    /// Moves the current target card to the discardPile
    /// </summary>
    /// <param name="cp">The CardProspector to be moved</param>
    void MoveToDiscard(CardProspector cp)
    {
        // Set the state of the card to discard
        cp.state = eCardState.discard;
        discardPile.Add(cp);  // Add it to the discardPile List<>
        cp.transform.SetParent(layoutAnchor); // Update its transform parent

        // Position it on the discardPile
        cp.SetLocalPos(new Vector3(
        jsonLayout.multiplier.x * jsonLayout.discardPile.x,
        jsonLayout.multiplier.y * jsonLayout.discardPile.y,
        0));

        cp.faceUp = true;

        // Place it on top of the pile for depth sorting
        cp.SetSpriteSortingLayer(jsonLayout.discardPile.layer);               // a
        cp.SetSortingOrder(-200 + (discardPile.Count * 3));                  // b
    }

    /// <summary>
    /// Make cp the new target card
    /// </summary>
    /// <param name="cp">The CardProspector to be moved</param>
    void MoveToTarget(CardProspector cp)
    {
        // If there is currently a target card, move it to discardPile
        if (target != null) MoveToDiscard(target);

        // Use MoveToDiscard to move the target card to the correct location
        MoveToDiscard(cp);                                                    // c

        // Then set a few additional things to make cp the new target
        target = cp; // cp is the new target
        cp.state = eCardState.target;

        // Set the depth sorting so that cp is on top of the discardPile
        cp.SetSpriteSortingLayer("Target");                                 // c
        cp.SetSortingOrder(0);
    }

    /// <summary>
    /// Arranges all the cards of the drawPile to show how many are left
    /// </summary>
    void UpdateDrawPile()
    {
        CardProspector cp;
        // Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cp = drawPile[i];
            cp.transform.SetParent(layoutAnchor);

            // Position it correctly with the layout.drawPile.stagger
            Vector3 cpPos = new Vector3();
            cpPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            // Add the staggering for the drawPile
            cpPos.x += jsonLayout.drawPile.xStagger * i;
            cpPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cpPos.z = 0.1f * i;
            cp.SetLocalPos(cpPos);

            cp.faceUp = false; // DrawPile Cards are all face-down
            cp.state = eCardState.drawpile;
            // Set depth sorting
            cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    // public void SetMineFaceUps(){
    //     CardProspector coverCP;
    //     foreach (CardProspector cp in mine){
    //         bool faceUp = true;
    //         foreach(int coverID in cp.layoutSlot.hiddenBy){
    //             coverCP = mineIdToCardDict[coverID];
    //             if(coverCP == null || coverCP.state == eCardState.mine){
    //                 faceUp = false;
    //             }
    //         }
    //         cp.faceUp = faceUp;
    //     }
    // }
    public void UpdateTopCard(){
        topCard = new Dictionary<string, CardProspector>();
        foreach(CardProspector cp in mine){
            string col = cp.layoutSlot.layer;
            if(!topCard.ContainsKey(col)){
                topCard[col] = cp;
            }
            else{
                CardProspector currTop = topCard[col];
                if(cp.layoutID > currTop.layoutID){
                    topCard[col] = cp;
                }
            }
        }
    }
    static public void CARD_CLICKED(CardProspector cp){
        switch(cp.state){
        case eCardState.target:
            break;
        case eCardState.drawpile:
            S.MoveToTarget(S.Draw());
            S.UpdateDrawPile();
            break;
        case eCardState.mine:
            bool validMatch = true;
            string col = cp.layoutSlot.layer;
            if(!S.topCard.ContainsKey(col) || S.topCard[col] != cp){
                validMatch = false;
            }
            if(!cp.faceUp) validMatch = false;
            if(!cp.AdjacentTo(S.target)) validMatch = false;
            if(validMatch){
                S.mine.Remove(cp);
                S.MoveToTarget(cp);
                // S.SetMineFaceUps();
                S.UpdateTopCard();
            }
            break;
        }
    }

}