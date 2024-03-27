using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Sprite xSprite;
    [SerializeField] Sprite oSprite;
    [SerializeField] SpriteRenderer sr;

    Controller.State myState;
    public Vector2 myCoordinates;
    Controller controller;

    public Vector2 Coordinates {
        get { return myCoordinates; }
    }
    public Controller.State MyState {
        get { return myState; }
    }

    public void Setup(Controller c, Vector2 coord) {
        Clear();
        controller = c;
        myCoordinates = coord;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (controller.CanPlayerMove && myState == Controller.State.empty) controller.MakeMove(this);
    }

    public void Mark(bool x) {
        sr.sprite = x ? xSprite : oSprite;
        myState = x ? Controller.State.X : Controller.State.O;
    }

    public void Clear() {
        sr.sprite = null;
        myState = Controller.State.empty;
    }
}
