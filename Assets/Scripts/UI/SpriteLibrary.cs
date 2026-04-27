using UnityEngine;
using System.Collections.Generic;

public class SpriteLibrary : MonoBehaviour
{
    public Sprite[] availableSprites;

    private Dictionary<string, Sprite> spriteDict;

    void Awake()
    {
        spriteDict = new Dictionary<string, Sprite>();

        foreach (var sprite in availableSprites)
        {
            if (!spriteDict.ContainsKey(sprite.name))
                spriteDict.Add(sprite.name, sprite);
        }
    }

    public Sprite GetSpriteByName(string spriteName)
    {
        if (spriteDict.ContainsKey(spriteName))
            return spriteDict[spriteName];

        return null;
    }
}
