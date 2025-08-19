# 🎨 PixelLAB MCP Integration Setup Guide

## 🎯 **What This Gives You**

With PixelLAB MCP installed in Cursor, you can now:
- **Generate AI characters** for your MMO
- **Create animated sprites** with multiple directions
- **Generate world tiles** and terrain
- **Create seamless tilesets** for your game world

## 🔧 **Setup Steps**

### **Step 1: Verify MCP Installation**
✅ You already have PixelLAB MCP installed in Cursor with your API key
✅ Your API key is: `5082c0f9-5833-43da-97e5-5ec839009c2a`

### **Step 2: Set Up Unity Scene**
1. **Open `SimpleMMOScene.unity`** in Unity
2. **Select the `MMOUIManager` GameObject** in the Hierarchy
3. **Add the `MMOUIManager` script** to it (if not already there)
4. **Create a new GameObject** called `PixelLABManager`
5. **Add the `PixelLABMCPIntegration` script** to `PixelLABManager`

### **Step 3: Test the Integration**
1. **Enter Play Mode** in Unity
2. **Navigate to Character Creation** screen
3. **Enter a character name** and select a class
4. **Click 🎨 GENERATE WITH AI** button
5. **Watch the Console** for MCP function calls

## 🎨 **Available MCP Functions**

### **Character Generation**
```csharp
// Generate a character sprite
mcp_pixellab_create_character(
    description: "brave warrior with heavy armor, sword and shield",
    size: 48,
    proportions: "default",
    style: "pixel art"
)
```

### **Character Animation**
```csharp
// Add animations to existing character
mcp_pixellab_animate_character(
    character_id: "your_character_id",
    template_animation_id: "walking",
    action_description: "walking quickly"
)
```

### **World Tiles**
```csharp
// Create individual tiles
mcp_pixellab_create_isometric_tile(
    description: "grass terrain with flowers",
    size: 32,
    tile_shape: "thick tile"
)
```

### **Terrain Tilesets**
```csharp
// Create seamless terrain transitions
mcp_pixellab_create_tileset(
    lower_description: "ocean water",
    upper_description: "sandy beach",
    transition_size: 0.25,
    transition_description: "wet sand with foam"
)
```

## 🚀 **How to Use in Your Code**

### **Basic Character Generation**
```csharp
public class CharacterGenerator : MonoBehaviour
{
    public void GenerateWarrior(string name)
    {
        string description = $"brave warrior {name} with heavy armor, sword and shield, battle-scarred";
        
        // This would call your MCP function
        // mcp_pixellab_create_character(description, 48, "default", "pixel art")
    }
}
```

### **Batch Generation**
```csharp
public void GenerateAllClasses()
{
    string[] classes = {"Warrior", "Mage", "Archer", "Rogue"};
    
    foreach (string className in classes)
    {
        string description = $"heroic {className.ToLower()} with {className} equipment and weapons";
        // Call MCP function for each class
    }
}
```

## 🔍 **Troubleshooting**

### **"PixelLAB MCP Integration not found"**
- Make sure `PixelLABMCPIntegration` script is attached to a GameObject
- Check that the GameObject is active in the scene

### **"Could not find character name input"**
- Ensure the Character Creation screen has `TMP_InputField` components
- Check that the UI elements are properly set up

### **MCP Functions Not Working**
- Verify your API key is correct in Cursor MCP settings
- Check that PixelLAB service is online
- Look for error messages in Cursor's MCP console

## 📱 **UI Integration**

The `PixelLABMCPIntegration` script automatically:
- ✅ **Finds UI elements** in your character creation screen
- ✅ **Updates status text** during generation
- ✅ **Manages character data** and previews
- ✅ **Handles errors** gracefully
- ✅ **Logs all MCP calls** for debugging

## 🎮 **Next Steps**

1. **Test basic character generation**
2. **Add character animations**
3. **Create world tiles and terrain**
4. **Build a complete character roster**
5. **Generate unique game assets**

## 🆘 **Need Help?**

- **Check Unity Console** for error messages
- **Verify MCP functions** are working in Cursor
- **Test with simple descriptions** first
- **Use the Debug.Log outputs** to track progress

---

**🎨 Happy Pixel Art Generation! 🎨**
