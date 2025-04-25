
using UnityEngine;
using System.Collections.Generic;

public class GolfManager : MonoBehaviour {
    public TextAsset layoutJSON;
    public Layout layout;
    public List<CardGolf> tableau;
    public Transform layoutAnchor;

    void Start() {
        layout = JsonUtility.FromJson<Layout>(layoutJSON.text);
        LayoutGame();
    }

    void LayoutGame() {
        layoutAnchor = new GameObject("_Layout").transform;
        tableau = new List<CardGolf>();

        foreach (SlotDef tSD in layout.slotDefs) {
            GameObject go = Instantiate(Resources.Load("CardGolf") as GameObject);
            CardGolf cd = go.GetComponent<CardGolf>();
            cd.transform.position = new Vector3(tSD.x, tSD.y, -tSD.layerID);
            cd.faceUp = tSD.faceUp;
            cd.transform.SetParent(layoutAnchor);
            tableau.Add(cd);
        }
    }
}
