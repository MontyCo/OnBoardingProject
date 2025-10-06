using System.Collections.Generic;
using com.ksgames.services.content;
using R3;
using UnityEngine;
using UnityEngine.U2D;

public class ContentTester : MonoBehaviour
{
    private Observable<SpriteAtlas> currentLoad;
    private const string TEST_ATLAS_ADDRESS = "TOKENS";

    private Sprite displayedSprite; // Store the retrieved sprite

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "Load Atlas"))
        {
            LoadAtlas();
        }

        if (GUI.Button(new Rect(10, 60, 200, 40), "Load & Unsubscribe"))
        {
            LoadAndUnsubscribe();
        }

        if (GUI.Button(new Rect(10, 110, 200, 40), "Get Sprite"))
        {
            GetSprite("TokenType", 0);
        }

        if (GUI.Button(new Rect(10, 160, 200, 40), "Unload Atlas"))
        {
            UnloadAtlas();
        }

        // Draw the sprite if it exists
        if (displayedSprite != null)
        {
            Texture2D texture = displayedSprite.texture;
            Rect texCoords = displayedSprite.textureRect;
            texCoords.x /= texture.width;
            texCoords.y /= texture.height;
            texCoords.width /= texture.width;
            texCoords.height /= texture.height;

            // Display sprite on screen
            GUI.DrawTextureWithTexCoords(
                new Rect(250, 10, 128, 128), // position and size
                texture,
                texCoords
            );
        }
    }

    void LoadAtlas()
    {
        Debug.Log("Test: Load Atlas");
        CoreContentService.Instance.LoadAtlas(TEST_ATLAS_ADDRESS).Subscribe(atlas =>
        {
            if (atlas != null)
                Debug.Log($"Atlas loaded successfully: {atlas.name}");
            else
                Debug.Log("Failed to load atlas");
        });
    }

    void LoadAndUnsubscribe()
    {
        Debug.Log("Test: Load & Unsubscribe");
        currentLoad = CoreContentService.Instance.LoadAtlas(TEST_ATLAS_ADDRESS);
        var subscription = currentLoad.Subscribe(atlas =>
        {
            if (atlas != null)
                Debug.Log($"Atlas loaded successfully: {atlas.name}");
            else
                Debug.Log("Failed to load atlas");
        });

        subscription.Dispose();
        Debug.Log("Unsubscribed immediately");
    }

    void GetSprite(string type, int level)
    {
        var sprite = CoreContentService.Instance.GetTokenSpriteByType(type, level);
        if (sprite != null)
        {
            Debug.Log($"Got sprite: {sprite.name}");
            displayedSprite = sprite; // store it to draw on screen
        }
        else
        {
            Debug.Log("Sprite not found or atlas not loaded");
        }
    }

    void UnloadAtlas()
    {
        bool result = CoreContentService.Instance.UnloadAtlas(TEST_ATLAS_ADDRESS);
        Debug.Log(result ? "Atlas unloaded successfully" : "Atlas was not cached");

        displayedSprite = null; // clear the display when unloaded
    }
}
