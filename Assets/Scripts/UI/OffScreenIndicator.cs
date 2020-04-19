using UnityEngine;
using System.Collections;

public class OffScreenIndicator : MonoBehaviour {
    
    public Sprite Icon;
    public Camera MainCamera;
    public float ScreenMargin = 10.0f;
    
    
    /* This is to ensure resolution-independent scaling. */
    private float scaleFactor = Screen.width / 500.0f;
    
    private Vector2 indicatorBounds;
    private bool visible = true;
    
    void Start()
    {

        MainCamera = Camera.main;
        visible = GetComponent<SpriteRenderer>().isVisible;

        indicatorBounds.x = Screen.width - ScreenMargin;
        indicatorBounds.y = Screen.height - ScreenMargin;
        indicatorBounds /= 2.0f;

    }

    void OnGUI() {
        if (!visible) {
            Vector3 screenDirection = transform.position - MainCamera.transform.position;
            screenDirection = Vector3.Normalize(screenDirection);
            screenDirection.y *= -1f;

            Vector2 indicatorPosition =
                new Vector2((Screen.width / 2.0f) + indicatorBounds.x * screenDirection.x,
                            (Screen.height / 2.0f) + indicatorBounds.y * screenDirection.y);

            Vector3 screenPoint = new Vector3(indicatorPosition.x, indicatorPosition.y, transform.position.z);
            Vector3 pointDirection = transform.position - MainCamera.ScreenToWorldPoint(screenPoint);
            pointDirection = Vector3.Normalize(pointDirection);

            float angle = Mathf.Atan2(pointDirection.x, pointDirection.y) * Mathf.Rad2Deg;

            Rect spriteRect = Icon.rect;
            Texture2D tex = Icon.texture;
            GUIUtility.RotateAroundPivot(angle, indicatorPosition);
            Rect rect = new Rect(indicatorPosition.x, indicatorPosition.y, scaleFactor * spriteRect.width, scaleFactor * spriteRect.height);
            GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width/ tex.width, spriteRect.height / tex.height));
            GUIUtility.RotateAroundPivot(0, indicatorPosition);
        }
    }

    void OnBecameInvisible() {
        visible = false;

        //When the obstacle is on screen, destroy the indicator component so it doesn't show up
        //  when the obstacle moves offscreen again
        Destroy(this);
    }

    void OnBecameVisible() {
        visible = true;
    }
}