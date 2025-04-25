using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
    [Header("Set Dynamically")]
    public string suit; // SUit of the card (C, D, H, or S)
    public int rank; // Rank of the card (1-14)
    public Color color = Color.black; // color to tink pips
    public string colS = "Black"; // or "Red". Name of the color

    // This list holds all of the Decorator GaeObjects
    public List<GameObject> decoGOs = new List<GameObject>();
    // This list holds all of the Pip GameObjects
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; // The GameObject of the back of the card
    public CardDefinition def; // Parsed from the DeckXML.xml

    // List of the SpriteRenderer Components of this GameObject and its children
    public SpriteRenderer[] spriteRenderers;

    void Start(){
        SetSortOrder(0); // Ensures that the card starts properly depth sorted    
    }

    // If spriteRenderers is not yet defined, this function will define it
    public void PopulateSpriteRenderers() {
        if (spriteRenderers == null || spriteRenderers.Length == 0){
            // Get SpriteRenderer Components of this Game Object and its children
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // Sets the SortingLayerName on the SpriteRenderer Components
    public void SetSortingLayerName(string tSLN){
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSR in spriteRenderers){
            tSR.sortingLayerName = tSLN;
        }
    }

    // Sets the SortingOrder on the SpriteRenderer Components
    public void SetSortOrder(int sOrd){
        PopulateSpriteRenderers();

        //Iterate through all the spriteRenderers as tSR
        foreach (SpriteRenderer tSR in spriteRenderers){
            if (tSR.gameObject == this.gameObject){
                // If the gameObject is this.gameObject, its the background
                tSR.sortingOrder = sOrd;
                continue; // Continue the next iteration of the loop
            }

            //switch based on the names
            switch(tSR.gameObject.name){
                case "back":
                    // Set to the highest layer to cover the other sprites 
                    tSR.sortingOrder = sOrd + 2;
                    break;

                case "face":
                default:
                    // Set it to the middle layer to be above the background
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    public bool faceUp{
        get{
            return (!back.activeSelf);
        }
        set{
            back.SetActive(!value);
        }
    }

    // Virtual methods can be overridden by subclass methods with the same name 
    virtual public void OnMouseUpAsButton(){
    }
}

[System.Serializable] // A serializable class is able to be edited in the Inspector
public class Decorator {
    // THis class stores information about each decorator or pip from DeckXML
    public string type; // For card pops, type = "pip"
    public Vector3 loc; // The location of the Sprite on the Card
    public bool flip = false; // Wether to flip the sprite vertically
    public float scale = 1f; // The scale of the sprite
}

[System.Serializable]
public class CardDefinition{
    // This class stores information for each rank of card
    public string face; // Sprite to use for each face card
    public int rank; // The rank (1-13) of this card
    public List<Decorator> pips = new List<Decorator>(); // Pips used
}


