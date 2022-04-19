using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TextureExtentions {
    public static Sprite ToSprite(this Texture2D texture2d) {
        return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero);
    }

    public static Sprite ToSprite(this Texture texture) {
        return texture.ToTexture2D().ToSprite();
    }

    public static RenderTexture ToRenderTexture(this Texture texture) {
        var rt = new RenderTexture(texture.width, texture.height, 32);

        Graphics.Blit(texture, rt);

        return rt;
    }

    public static Texture2D ToTexture2D(this RenderTexture rTex) {
        var result = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        var currentRT = RenderTexture.active;

        RenderTexture.active = rTex;
        result.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        result.Apply();
        RenderTexture.active = currentRT;

        return result;
    }

    public static Texture2D ToTexture2D(this Texture texture) {
        return texture.ToRenderTexture().ToTexture2D();
    }

    public static Texture2D ClippingSquare(this Texture2D texture) {
        Color[] pixel;
        Texture2D clipTex;

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        if (textureWidth == textureHeight)
            return texture;
        if (textureWidth > textureHeight) {
            int x = (textureWidth - textureHeight) / 2;
            pixel = texture.GetPixels(x, 0, textureHeight, textureHeight);
            clipTex = new Texture2D(textureHeight, textureHeight);
        } else {
            int y = textureHeight - textureWidth;
            pixel = texture.GetPixels(0, y, textureWidth, textureWidth);
            clipTex = new Texture2D(textureWidth, textureWidth);
        }

        clipTex.SetPixels(pixel);
        clipTex.Apply();

        return clipTex;
    }

    public static Texture2D ClippingSquare(this Texture texture) {
        return texture.ToTexture2D().ClippingSquare();
    }
}
