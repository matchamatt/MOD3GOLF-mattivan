using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{

    [Header("Set in Inspector")]
    public bool startFaceUp = false;
    //suits
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    // Prefabs
    public GameObject prefabCard;
    public GameObject prefabGoldCard;
    public GameObject prefabSprite;
    int randomChance;

    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    // InitDeck is called by Prospector when it is ready
    public void InitDeck(string deckXMLText)
    {
        // This creates an anchor for all the card GameObjects in the Hierarchy
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        // Initialize the Dictionary of SuitSprits with necessary Sprits
        dictSuits = new Dictionary<string, Sprite>(){
            { "C", suitClub },
            { "D", suitDiamond },
            { "H", suitHeart },
            { "S", suitSpade }
        };

        ReadDeck(deckXMLText);

        MakeCards();
    }

    // ReadDeck parses the XML file passed to it into CardDefinitions
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader(); // Create a new PT_XMLReader
        xmlr.Parse(deckXMLText); // Use that PT_XMLReader to parse DeckXML

        // This prints a test line to show you how xmlr can be used.
        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");

        // Read decorators for all cards
        decorators = new List<Decorator>(); // Init LIst of decorators
        // Grab an PT_XMLHashlist of all <decorator>s in the XML file
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;

        for (int i = 0; i < xDecos.Count; i++)
        {
            // FOr each <decorator> in the XML
            deco = new Decorator(); // Make a new decorator
            // Cope the attributes of the <decorator> to the Decorator
            deco.type = xDecos[i].att("type");
            // bool deco.flip is true if the text of the flip attribute is "1"
            deco.flip = (xDecos[i].att("flip") == "1");
            // floats need to be parsed from the attribute strings
            deco.scale = float.Parse(xDecos[i].att("scale"));
            // Vector3 loc initializes to [0,0,0], so we just need to modify it
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            // Add the temporary deco to the List decorators
            decorators.Add(deco);
        }

        //Read pip locations for each card number
        cardDefs = new List<CardDefinition>();
        // Grab an PT_XMLHashlist of all <card>s in the XML file
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for (int i = 0; i < xCardDefs.Count; i++)
        {
            // For each of the <card>s
            // Create a new card definition
            CardDefinition cDef = new CardDefinition();
            // Parse the attribute values and add them to cDef
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            // Grab an PT_XMLHashlist of all the <pip>s on this <card>
            PT_XMLHashList xPips = xCardDefs[i]["pip"];

            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    // Iiterate through all the <pip>s
                    deco = new Decorator();
                    // <pip>s on the <card> are handles via the Decorator Class
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));

                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }

            // Face cards (Jack, Queen, and Kind) have a face attribute
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }

    // Get the proper CardDefinition based on Rank (1-14 is Ace to King)
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        // Search through all of the CardDefinitions
        foreach (CardDefinition cd in cardDefs)
        {
            // If the rank is correct, return this definition
            if (cd.rank == rnk)
            {
                return (cd);
            }
        }
        return (null);
    }

    // Make the Card GameObjects
    public void MakeCards()
    {
        // cardNames will be the names of cards to build
        // Each suit goes from 1 to 14 (e.g., C1 to C14 fro clubs)
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };

        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        // Make a List to hold all of the cards
        cards = new List<Card>();

        // Iterate through all of the card names that were just made
        for (int i = 0; i < cardNames.Count; i++)
        {
            // Make the card and add it to the cards Deck
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        // Create a random number for a 10% chance of being a gold card
        randomChance = Random.Range(0, 10);
        GameObject cgo;

        if (randomChance == 0)
        {
            cgo = Instantiate(prefabGoldCard) as GameObject;
            cgo.gameObject.tag = "GoldCard";
        }
        else
        {
            cgo = Instantiate(prefabCard) as GameObject;
        }

        // Set the transform.parent of the new card to the anchor
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>(); // Get the Card Component

        // This line stacks the cards so that they'reall in nice rows
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        //Assign basic values to the Card
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        // Pull the CardDefinition for this card
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    // These private variables will be reused several times in the helper methods
    private Sprite _tSP = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        // Add Decorators
        foreach (Decorator deco in decorators)
        {
            if (deco.type == "suit")
            {
                // Instantiate a Sprite GameObject
                _tGO = Instantiate(prefabSprite) as GameObject;
                // Get the SpriteRenderer Component
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                // Set the Sprite to the proper suit
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                // Get the proper sprite to show this rank
                _tSP = rankSprites[card.rank];
                // Assign this rank Sprite to the SPrite Renderer
                _tSR.sprite = _tSP;
                // Set the color of the rank to match the suit
                _tSR.color = card.color;
            }

            // Make the deco Sprites render above the card
            _tSR.sortingOrder = 1;
            // Make the decorator Sprite a child of the Card
            _tGO.transform.SetParent(card.transform);
            // Set the localPosition based on the location from DeckXML
            _tGO.transform.localPosition = deco.loc;
            //Flip the decorator if needed
            if (deco.flip)
            {
                // An Euler rotation of 180 degrees around the Z-axis will flip it
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            // Set the scale to keep decos from being too big
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }

            // Name this GameObject so its easy to see
            _tGO.name = deco.type;
            // Add this deco GameObject to the List card.decoGOs
            card.decoGOs.Add(_tGO);
        }
    }

    private void AddPips(Card card)
    {
        // For each of the pips in the definition...
        foreach (Decorator pip in card.def.pips)
        {
            // ...Instantiate a pip game object
            _tGO = Instantiate(prefabSprite) as GameObject;
            // Set the parent to be the card GameObject
            _tGO.transform.SetParent(card.transform);
            // Set the position to that specified in the XML
            _tGO.transform.localPosition = pip.loc;

            // Flip it if necessary
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            // Scale it if necessary (only for the Ace)
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            // Give this game object a name
            _tGO.name = "pip";
            //Get the sprite renderer component
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            //Set the sprite to the proper suit
            _tSR.sprite = dictSuits[card.suit];
            // Set sortingOrder so the pip is rendered above the card_front
            _tSR.sortingOrder = 1;
            // Add this to the card's list of pips
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return; // No need to run if this isn't a face card
        }

        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();

        // Generate the right name and pass it to GetFace()
        _tSP = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSP; // Assign this Sprite to _tSR
        _tSR.sortingOrder = 1; // Set the sortingOrder
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    // Find the proper face card Sprite
    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSP in faceSprites)
        {
            // If this sprite has the right name...
            if (_tSP.name == faceS)
            {
                return (_tSP);
            }
        }
        //If nothing can be found, return null
        return (null);
    }

    private void AddBack(Card card)
    {
        // Add card back
        // The Card_Back will be able to cover everything else on the card
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        if (randomChance == 0)
        {
            _tSR.sprite = cardBackGold;
        }
        else
        {
            _tSR.sprite = cardBack;
        }

        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        //This is a higher sortingOrder than anything else
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        // Default to face up
        card.faceUp = startFaceUp; // Use the property faceUp of card

    }

    // Shuff the cards in Deck.cards
    static public void Shuffle(ref List<Card> oCards)
    {
        // Create a temporary List to hold the new shuffle order
        List<Card> tCards = new List<Card>();

        int ndx; // This will hold the index of the card to be moved
        tCards = new List<Card>(); // Initialize the temporary list

        // Repeat as long as there are cards in the origional List
        while (oCards.Count > 0)
        {
            // Pick the index of a random card
            ndx = Random.Range(0, oCards.Count);
            // Add that card to the temporary list
            tCards.Add(oCards[ndx]);
            // And remove that card from the origional lsit
            oCards.RemoveAt(ndx);
        }
        // Replace the origional list with the temporary list
        oCards = tCards;
    }
}