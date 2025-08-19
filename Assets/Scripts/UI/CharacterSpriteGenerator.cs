using UnityEngine;
using UnityEngine.UI;

public class CharacterSpriteGenerator : MonoBehaviour
{
    // Generate actual sprite textures for characters
    public static Sprite GenerateCharacterSprite(string className, bool isMale, int skinColor, int hairStyle, int hairColor, Color classColor)
    {
        // Create a 256x256 texture for higher quality character sprite
        Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point; // Pixel art style
        
        // Clear texture
        Color[] pixels = new Color[256 * 256];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Get color palettes
        Color[] skinTones = GetSkinTones();
        Color[] hairTones = GetHairTones();
        Color actualSkinColor = skinTones[Mathf.Clamp(skinColor, 0, skinTones.Length - 1)];
        Color actualHairColor = hairTones[Mathf.Clamp(hairColor, 0, hairTones.Length - 1)];
        
        // Draw character with proper proportions and details
        DrawDetailedCharacter(pixels, 256, className, isMale, actualSkinColor, actualHairColor, classColor, hairStyle);
        
        // Apply pixels to texture
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 200f);
        sprite.name = $"{className}_{(isMale ? "Male" : "Female")}_{skinColor}_{hairStyle}_{hairColor}";
        
        return sprite;
    }
    
    static void DrawDetailedCharacter(Color[] pixels, int width, string className, bool isMale, Color skinColor, Color hairColor, Color classColor, int hairStyle)
    {
        // Character proportions (more realistic)
        int centerX = width / 2;
        int headY = 200;
        int neckY = 175;
        int shoulderY = 165;
        int chestY = 140;
        int waistY = 115;
        int hipY = 100;
        int kneeY = 65;
        int footY = 20;
        
        // Head (proper oval shape)
        DrawOval(pixels, width, centerX, headY, 20, 25, skinColor);
        
        // Facial features
        DrawEyes(pixels, width, centerX, headY + 5, isMale);
        DrawNose(pixels, width, centerX, headY, skinColor);
        DrawMouth(pixels, width, centerX, headY - 8);
        
        // Hair
        DrawDetailedHair(pixels, width, centerX, headY + 15, hairStyle, isMale, hairColor);
        
        // Neck
        DrawRectangle(pixels, width, centerX - 6, centerX + 6, neckY + 5, neckY - 5, skinColor);
        
        // Body proportions based on class and gender
        int shoulderWidth = GetShoulderWidth(className, isMale);
        int chestWidth = GetChestWidth(className, isMale);
        int waistWidth = GetWaistWidth(className, isMale);
        
        // Torso with proper shaping
        DrawTorso(pixels, width, centerX, shoulderY, chestY, waistY, shoulderWidth, chestWidth, waistWidth, skinColor, className, classColor);
        
        // Arms with muscle definition
        DrawArms(pixels, width, centerX, shoulderY, chestY, shoulderWidth, isMale, skinColor, className, classColor);
        
        // Legs with proper proportions
        DrawLegs(pixels, width, centerX, waistY, hipY, kneeY, footY, waistWidth, isMale, className, classColor);
        
        // Class-specific equipment and details
        DrawClassEquipment(pixels, width, centerX, shoulderY, chestY, className, isMale, classColor);
    }
    
    static void DrawEyes(Color[] pixels, int width, int centerX, int eyeY, bool isMale)
    {
        // Eye whites
        DrawOval(pixels, width, centerX - 8, eyeY, 4, 3, Color.white);
        DrawOval(pixels, width, centerX + 8, eyeY, 4, 3, Color.white);
        
        // Pupils
        DrawCircle(pixels, width, centerX - 8, eyeY, 2, Color.black);
        DrawCircle(pixels, width, centerX + 8, eyeY, 2, Color.black);
        
        // Eyebrows
        Color browColor = new Color(0.3f, 0.2f, 0.1f, 1f);
        DrawRectangle(pixels, width, centerX - 12, centerX - 4, eyeY + 6, eyeY + 4, browColor);
        DrawRectangle(pixels, width, centerX + 4, centerX + 12, eyeY + 6, eyeY + 4, browColor);
    }
    
    static void DrawNose(Color[] pixels, int width, int centerX, int noseY, Color skinColor)
    {
        Color shadowColor = new Color(skinColor.r * 0.8f, skinColor.g * 0.8f, skinColor.b * 0.8f, 1f);
        DrawRectangle(pixels, width, centerX - 1, centerX + 1, noseY - 2, noseY - 8, shadowColor);
    }
    
    static void DrawMouth(Color[] pixels, int width, int centerX, int mouthY)
    {
        Color mouthColor = new Color(0.8f, 0.4f, 0.4f, 1f);
        DrawRectangle(pixels, width, centerX - 4, centerX + 4, mouthY + 1, mouthY - 1, mouthColor);
    }
    
    static void DrawDetailedHair(Color[] pixels, int width, int centerX, int hairY, int hairStyle, bool isMale, Color hairColor)
    {
        switch (hairStyle)
        {
            case 0: // Classic
                if (isMale)
                {
                    DrawOval(pixels, width, centerX, hairY, 25, 15, hairColor);
                    DrawRectangle(pixels, width, centerX - 15, centerX + 15, hairY + 10, hairY - 5, hairColor);
                }
                else
                {
                    DrawOval(pixels, width, centerX, hairY, 30, 20, hairColor);
                    DrawRectangle(pixels, width, centerX - 20, centerX + 20, hairY + 15, hairY - 10, hairColor);
                }
                break;
            case 1: // Short
                DrawOval(pixels, width, centerX, hairY, 22, 12, hairColor);
                break;
            case 2: // Long
                DrawOval(pixels, width, centerX, hairY, 28, 18, hairColor);
                if (!isMale) // Longer for females
                {
                    DrawRectangle(pixels, width, centerX - 15, centerX + 15, hairY - 10, hairY - 40, hairColor);
                }
                break;
            case 3: // Curly
                DrawCircle(pixels, width, centerX - 10, hairY + 5, 8, hairColor);
                DrawCircle(pixels, width, centerX + 10, hairY + 5, 8, hairColor);
                DrawCircle(pixels, width, centerX, hairY + 10, 12, hairColor);
                DrawCircle(pixels, width, centerX - 8, hairY - 5, 6, hairColor);
                DrawCircle(pixels, width, centerX + 8, hairY - 5, 6, hairColor);
                break;
            case 4: // Braided
                if (!isMale)
                {
                    DrawRectangle(pixels, width, centerX - 25, centerX - 20, hairY + 10, hairY - 30, hairColor);
                    DrawRectangle(pixels, width, centerX + 20, centerX + 25, hairY + 10, hairY - 30, hairColor);
                    DrawOval(pixels, width, centerX, hairY, 25, 15, hairColor);
                }
                else
                {
                    DrawOval(pixels, width, centerX, hairY, 25, 15, hairColor);
                }
                break;
        }
    }
    
    static void DrawTorso(Color[] pixels, int width, int centerX, int shoulderY, int chestY, int waistY, int shoulderWidth, int chestWidth, int waistWidth, Color skinColor, string className, Color classColor)
    {
        // Chest area
        for (int y = shoulderY; y >= chestY; y--)
        {
            float progress = (float)(shoulderY - y) / (shoulderY - chestY);
            int currentWidth = (int)Mathf.Lerp(shoulderWidth, chestWidth, progress);
            Color bodyColor = GetBodyColor(className, classColor);
            DrawRectangle(pixels, width, centerX - currentWidth/2, centerX + currentWidth/2, y + 1, y - 1, bodyColor);
        }
        
        // Waist area
        for (int y = chestY; y >= waistY; y--)
        {
            float progress = (float)(chestY - y) / (chestY - waistY);
            int currentWidth = (int)Mathf.Lerp(chestWidth, waistWidth, progress);
            Color bodyColor = GetBodyColor(className, classColor);
            DrawRectangle(pixels, width, centerX - currentWidth/2, centerX + currentWidth/2, y + 1, y - 1, bodyColor);
        }
    }
    
    static void DrawArms(Color[] pixels, int width, int centerX, int shoulderY, int chestY, int shoulderWidth, bool isMale, Color skinColor, string className, Color classColor)
    {
        int armWidth = isMale ? 12 : 10;
        int armLength = 45;
        
        // Left arm
        DrawRectangle(pixels, width, centerX - shoulderWidth/2 - armWidth, centerX - shoulderWidth/2, shoulderY, shoulderY - armLength, skinColor);
        
        // Right arm
        DrawRectangle(pixels, width, centerX + shoulderWidth/2, centerX + shoulderWidth/2 + armWidth, shoulderY, shoulderY - armLength, skinColor);
        
        // Muscle definition for warriors
        if (className == "Warrior" && isMale)
        {
            Color muscleColor = new Color(skinColor.r * 0.9f, skinColor.g * 0.9f, skinColor.b * 0.9f, 1f);
            DrawRectangle(pixels, width, centerX - shoulderWidth/2 - armWidth + 2, centerX - shoulderWidth/2 - 2, shoulderY - 5, shoulderY - 15, muscleColor);
            DrawRectangle(pixels, width, centerX + shoulderWidth/2 + 2, centerX + shoulderWidth/2 + armWidth - 2, shoulderY - 5, shoulderY - 15, muscleColor);
        }
    }
    
    static void DrawLegs(Color[] pixels, int width, int centerX, int waistY, int hipY, int kneeY, int footY, int waistWidth, bool isMale, string className, Color classColor)
    {
        int legWidth = isMale ? 16 : 14;
        Color legColor = GetLegColor(className, classColor);
        
        // Left leg
        DrawRectangle(pixels, width, centerX - waistWidth/2 - 2, centerX - 2, waistY, footY, legColor);
        
        // Right leg
        DrawRectangle(pixels, width, centerX + 2, centerX + waistWidth/2 + 2, waistY, footY, legColor);
        
        // Knee definition
        Color kneeColor = new Color(legColor.r * 0.9f, legColor.g * 0.9f, legColor.b * 0.9f, 1f);
        DrawRectangle(pixels, width, centerX - waistWidth/2 - 2, centerX - 2, kneeY + 3, kneeY - 3, kneeColor);
        DrawRectangle(pixels, width, centerX + 2, centerX + waistWidth/2 + 2, kneeY + 3, kneeY - 3, kneeColor);
        
        // Boots/feet
        Color bootColor = className == "Thief" ? Color.black : new Color(0.4f, 0.2f, 0.1f, 1f);
        DrawRectangle(pixels, width, centerX - waistWidth/2 - 2, centerX - 2, footY + 8, footY, bootColor);
        DrawRectangle(pixels, width, centerX + 2, centerX + waistWidth/2 + 2, footY + 8, footY, bootColor);
    }
    
    static void DrawClassEquipment(Color[] pixels, int width, int centerX, int shoulderY, int chestY, string className, bool isMale, Color classColor)
    {
        switch (className)
        {
            case "Warrior":
                // Detailed armor
                Color armorColor = Color.Lerp(Color.gray, classColor, 0.4f);
                DrawRectangle(pixels, width, centerX - 25, centerX + 25, shoulderY + 5, chestY - 20, armorColor);
                
                // Helmet
                DrawOval(pixels, width, centerX, shoulderY + 25, 22, 15, armorColor);
                
                // Two-handed sword (iconic weapon)
                Color swordColor = new Color(0.8f, 0.8f, 0.9f, 1f);
                Color handleColor = new Color(0.4f, 0.2f, 0.1f, 1f);
                // Blade
                DrawRectangle(pixels, width, centerX + 30, centerX + 34, shoulderY + 40, shoulderY - 20, swordColor);
                // Handle
                DrawRectangle(pixels, width, centerX + 29, centerX + 35, shoulderY - 20, shoulderY - 35, handleColor);
                // Crossguard
                DrawRectangle(pixels, width, centerX + 25, centerX + 39, shoulderY - 18, shoulderY - 22, swordColor);
                break;
                
            case "Mage":
                // Flowing robes
                Color robeColor = Color.Lerp(Color.blue, classColor, 0.4f);
                DrawRectangle(pixels, width, centerX - 30, centerX + 30, chestY, chestY - 60, robeColor);
                
                // Wizard hat
                DrawTriangle(pixels, width, centerX, shoulderY + 45, 20, robeColor);
                
                // Staff (iconic weapon)
                Color staffColor = new Color(0.4f, 0.2f, 0.1f, 1f);
                Color orbColor = Color.Lerp(Color.cyan, classColor, 0.5f);
                // Staff shaft
                DrawRectangle(pixels, width, centerX - 35, centerX - 32, shoulderY + 35, shoulderY - 40, staffColor);
                // Magic orb
                DrawCircle(pixels, width, centerX - 33, shoulderY + 40, 6, orbColor);
                // Orb glow
                DrawCircle(pixels, width, centerX - 33, shoulderY + 40, 8, new Color(orbColor.r, orbColor.g, orbColor.b, 0.3f));
                break;
                
            case "Archer":
                // Leather armor
                Color leatherColor = Color.Lerp(Color.brown, classColor, 0.3f);
                DrawRectangle(pixels, width, centerX - 20, centerX + 20, shoulderY, chestY - 15, leatherColor);
                
                // Cap
                DrawOval(pixels, width, centerX, shoulderY + 20, 20, 12, leatherColor);
                
                // Bow (iconic weapon)
                Color bowColor = new Color(0.4f, 0.2f, 0.1f, 1f);
                Color stringColor = new Color(0.9f, 0.9f, 0.8f, 1f);
                // Bow frame
                DrawCurve(pixels, width, centerX - 35, shoulderY + 30, centerX - 45, shoulderY - 20, bowColor);
                // Bow string
                DrawLine(pixels, width, centerX - 35, shoulderY + 30, centerX - 45, shoulderY - 20, stringColor);
                break;
                
            case "Thief":
                // Dark hooded cloak
                Color cloakColor = Color.Lerp(Color.black, classColor, 0.3f);
                DrawRectangle(pixels, width, centerX - 25, centerX + 25, shoulderY + 10, chestY - 25, cloakColor);
                
                // Hood
                DrawTriangle(pixels, width, centerX, shoulderY + 30, 25, cloakColor);
                
                // Twin daggers (iconic weapons)
                Color daggerColor = new Color(0.7f, 0.7f, 0.8f, 1f);
                Color daggerHandle = new Color(0.2f, 0.1f, 0.05f, 1f);
                // Left dagger
                DrawRectangle(pixels, width, centerX - 40, centerX - 37, shoulderY, shoulderY - 15, daggerColor);
                DrawRectangle(pixels, width, centerX - 41, centerX - 36, shoulderY - 15, shoulderY - 20, daggerHandle);
                // Right dagger
                DrawRectangle(pixels, width, centerX + 37, centerX + 40, shoulderY, shoulderY - 15, daggerColor);
                DrawRectangle(pixels, width, centerX + 36, centerX + 41, shoulderY - 15, shoulderY - 20, daggerHandle);
                break;
                
            case "Priest":
                // Holy robes
                Color holyColor = Color.Lerp(Color.white, classColor, 0.2f);
                DrawRectangle(pixels, width, centerX - 28, centerX + 28, shoulderY, chestY - 50, holyColor);
                
                // Halo
                DrawCircle(pixels, width, centerX, shoulderY + 35, 12, Color.yellow, true);
                DrawCircle(pixels, width, centerX, shoulderY + 35, 10, Color.yellow, true);
                
                // Holy staff (iconic weapon)
                Color holyStaffColor = new Color(0.9f, 0.9f, 0.7f, 1f);
                Color crossColor = Color.gold;
                // Staff
                DrawRectangle(pixels, width, centerX - 32, centerX - 29, shoulderY + 30, shoulderY - 30, holyStaffColor);
                // Cross at top
                DrawRectangle(pixels, width, centerX - 38, centerX - 23, shoulderY + 32, shoulderY + 28, crossColor);
                DrawRectangle(pixels, width, centerX - 33, centerX - 28, shoulderY + 40, shoulderY + 20, crossColor);
                break;
        }
    }
    
    // Helper drawing functions with improved quality
    static void DrawPixel(Color[] pixels, int width, int x, int y, Color color)
    {
        if (x >= 0 && x < width && y >= 0 && y < width)
        {
            pixels[y * width + x] = color;
        }
    }
    
    static void DrawRectangle(Color[] pixels, int width, int x1, int x2, int y1, int y2, Color color)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            {
                DrawPixel(pixels, width, x, y, color);
            }
        }
    }
    
    static void DrawCircle(Color[] pixels, int width, int centerX, int centerY, int radius, Color color, bool outline = false)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (outline)
                {
                    if (distance <= radius && distance >= radius - 2)
                        DrawPixel(pixels, width, x, y, color);
                }
                else
                {
                    if (distance <= radius)
                        DrawPixel(pixels, width, x, y, color);
                }
            }
        }
    }
    
    static void DrawOval(Color[] pixels, int width, int centerX, int centerY, int radiusX, int radiusY, Color color)
    {
        for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
        {
            for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                float normalizedX = (float)(x - centerX) / radiusX;
                float normalizedY = (float)(y - centerY) / radiusY;
                if (normalizedX * normalizedX + normalizedY * normalizedY <= 1)
                {
                    DrawPixel(pixels, width, x, y, color);
                }
            }
        }
    }
    
    static void DrawTriangle(Color[] pixels, int width, int centerX, int topY, int baseWidth, Color color)
    {
        for (int y = topY; y >= topY - baseWidth; y--)
        {
            int currentWidth = (int)((float)(topY - y) / baseWidth * baseWidth);
            DrawRectangle(pixels, width, centerX - currentWidth/2, centerX + currentWidth/2, y, y, color);
        }
    }
    
    static void DrawLine(Color[] pixels, int width, int x1, int y1, int x2, int y2, Color color)
    {
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int x = x1;
        int y = y1;
        int stepX = x1 < x2 ? 1 : -1;
        int stepY = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawPixel(pixels, width, x, y, color);
            
            if (x == x2 && y == y2) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += stepX;
            }
            if (e2 < dx)
            {
                err += dx;
                y += stepY;
            }
        }
    }
    
    static void DrawCurve(Color[] pixels, int width, int x1, int y1, int x2, int y2, Color color)
    {
        // Simple arc approximation
        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = (int)Mathf.Lerp(x1, x2, t);
            int y = (int)(Mathf.Lerp(y1, y2, t) + Mathf.Sin(t * Mathf.PI) * 10);
            DrawPixel(pixels, width, x, y, color);
            DrawPixel(pixels, width, x + 1, y, color); // Thicker line
        }
    }
    
    // Class-specific helpers with better proportions
    static int GetShoulderWidth(string className, bool isMale)
    {
        int baseWidth = isMale ? 40 : 35;
        switch (className)
        {
            case "Warrior": return baseWidth + 8;
            case "Mage": return baseWidth - 5;
            case "Priest": return baseWidth;
            case "Archer": return baseWidth + 2;
            case "Thief": return baseWidth - 3;
            default: return baseWidth;
        }
    }
    
    static int GetChestWidth(string className, bool isMale)
    {
        int baseWidth = isMale ? 35 : 30;
        switch (className)
        {
            case "Warrior": return baseWidth + 6;
            case "Mage": return baseWidth - 3;
            case "Priest": return baseWidth + 2;
            case "Archer": return baseWidth;
            case "Thief": return baseWidth - 5;
            default: return baseWidth;
        }
    }
    
    static int GetWaistWidth(string className, bool isMale)
    {
        int baseWidth = isMale ? 28 : 22;
        switch (className)
        {
            case "Warrior": return baseWidth + 4;
            case "Mage": return baseWidth - 2;
            case "Priest": return baseWidth;
            case "Archer": return baseWidth;
            case "Thief": return baseWidth - 3;
            default: return baseWidth;
        }
    }
    
    static Color GetBodyColor(string className, Color classColor)
    {
        switch (className)
        {
            case "Warrior": return Color.Lerp(Color.gray, classColor, 0.4f);
            case "Mage": return Color.Lerp(Color.blue, classColor, 0.3f);
            case "Priest": return Color.Lerp(Color.white, classColor, 0.2f);
            case "Archer": return Color.Lerp(Color.brown, classColor, 0.3f);
            case "Thief": return Color.Lerp(Color.black, classColor, 0.2f);
            default: return Color.gray;
        }
    }
    
    static Color GetLegColor(string className, Color classColor)
    {
        switch (className)
        {
            case "Warrior": return Color.Lerp(Color.gray, classColor, 0.4f);
            case "Mage": return Color.Lerp(Color.blue, classColor, 0.4f);
            case "Priest": return Color.Lerp(Color.white, classColor, 0.3f);
            case "Archer": return Color.brown;
            case "Thief": return Color.Lerp(Color.black, classColor, 0.3f);
            default: return Color.gray;
        }
    }
    
    static Color[] GetSkinTones()
    {
        return new Color[]
        {
            new Color(1f, 0.8f, 0.6f, 1f),      // Fair
            new Color(0.95f, 0.76f, 0.57f, 1f), // Light
            new Color(0.9f, 0.7f, 0.5f, 1f),    // Medium
            new Color(0.8f, 0.6f, 0.4f, 1f),    // Tan
            new Color(0.6f, 0.4f, 0.3f, 1f),    // Dark
            new Color(0.4f, 0.3f, 0.2f, 1f)     // Deep
        };
    }
    
    static Color[] GetHairTones()
    {
        return new Color[]
        {
            new Color(0.4f, 0.2f, 0.1f, 1f),    // Brown
            new Color(0.1f, 0.1f, 0.1f, 1f),    // Black
            new Color(0.9f, 0.8f, 0.3f, 1f),    // Blonde
            new Color(0.7f, 0.3f, 0.1f, 1f),    // Red
            new Color(0.7f, 0.7f, 0.7f, 1f),    // Silver
            new Color(0.9f, 0.9f, 0.9f, 1f),    // White
            new Color(0.2f, 0.4f, 0.8f, 1f)     // Blue
        };
    }
}

// New weapon icon generator for class selection UI
public static class WeaponIconGenerator
{
    public static Sprite GenerateWeaponIcon(string className, Color classColor)
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        switch (className)
        {
            case "Warrior":
                // Two-handed sword
                DrawWeaponRect(pixels, 64, 30, 34, 55, 10, new Color(0.8f, 0.8f, 0.9f, 1f)); // Blade
                DrawWeaponRect(pixels, 64, 29, 35, 15, 10, new Color(0.4f, 0.2f, 0.1f, 1f)); // Handle
                DrawWeaponRect(pixels, 64, 25, 39, 17, 15, new Color(0.8f, 0.8f, 0.9f, 1f)); // Crossguard
                break;
                
            case "Mage":
                // Staff with orb
                DrawWeaponRect(pixels, 64, 30, 34, 55, 10, new Color(0.4f, 0.2f, 0.1f, 1f)); // Staff
                DrawWeaponCircle(pixels, 64, 32, 50, 6, Color.Lerp(Color.cyan, classColor, 0.5f)); // Orb
                break;
                
            case "Archer":
                // Bow
                DrawWeaponCurve(pixels, 64, 25, 50, 25, 15, new Color(0.4f, 0.2f, 0.1f, 1f)); // Bow
                DrawWeaponLine(pixels, 64, 25, 50, 25, 15, new Color(0.9f, 0.9f, 0.8f, 1f)); // String
                break;
                
            case "Thief":
                // Twin daggers
                DrawWeaponRect(pixels, 64, 20, 22, 50, 30, new Color(0.7f, 0.7f, 0.8f, 1f)); // Left blade
                DrawWeaponRect(pixels, 64, 42, 44, 50, 30, new Color(0.7f, 0.7f, 0.8f, 1f)); // Right blade
                DrawWeaponRect(pixels, 64, 19, 23, 30, 25, new Color(0.2f, 0.1f, 0.05f, 1f)); // Left handle
                DrawWeaponRect(pixels, 64, 41, 45, 30, 25, new Color(0.2f, 0.1f, 0.05f, 1f)); // Right handle
                break;
                
            case "Priest":
                // Holy staff
                DrawWeaponRect(pixels, 64, 30, 34, 55, 15, new Color(0.9f, 0.9f, 0.7f, 1f)); // Staff
                DrawWeaponRect(pixels, 64, 25, 39, 52, 48, Color.gold); // Cross horizontal
                DrawWeaponRect(pixels, 64, 30, 34, 58, 42, Color.gold); // Cross vertical
                break;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }
    
    static void DrawWeaponRect(Color[] pixels, int width, int x1, int x2, int y1, int y2, Color color)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            {
                if (x >= 0 && x < width && y >= 0 && y < width)
                    pixels[y * width + x] = color;
            }
        }
    }
    
    static void DrawWeaponCircle(Color[] pixels, int width, int centerX, int centerY, int radius, Color color)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (distance <= radius && x >= 0 && x < width && y >= 0 && y < width)
                {
                    pixels[y * width + x] = color;
                }
            }
        }
    }
    
    static void DrawWeaponCurve(Color[] pixels, int width, int x1, int y1, int x2, int y2, Color color)
    {
        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = (int)Mathf.Lerp(x1, x2, t);
            int y = (int)(Mathf.Lerp(y1, y2, t) + Mathf.Sin(t * Mathf.PI) * 8);
            if (x >= 0 && x < width && y >= 0 && y < width)
            {
                pixels[y * width + x] = color;
                if (x + 1 < width) pixels[y * width + x + 1] = color; // Thicker line
            }
        }
    }
    
    static void DrawWeaponLine(Color[] pixels, int width, int x1, int y1, int x2, int y2, Color color)
    {
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int x = x1;
        int y = y1;
        int stepX = x1 < x2 ? 1 : -1;
        int stepY = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x >= 0 && x < width && y >= 0 && y < width)
                pixels[y * width + x] = color;
            
            if (x == x2 && y == y2) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += stepX;
            }
            if (e2 < dx)
            {
                err += dx;
                y += stepY;
            }
        }
    }
}