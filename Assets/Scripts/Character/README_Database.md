# SPUM Character Database Setup

## Problem
The character selection UI is failing because the `SPUMCharacterDatabase` asset exists but has no character classes defined.

## Solution
Use the `CreateSPUMDatabase` script to populate the database with valid character classes.

## Steps to Fix:

1. **In Unity Editor:**
   - Find the `CreateSPUMDatabase` script in the Project window
   - Right-click on it and select "Create SPUM Database" from the context menu
   - This will populate the `SPUMCharacterDatabase.asset` file with valid character classes

2. **Alternative Manual Method:**
   - Select the `SPUMCharacterDatabase.asset` file in the Project window
   - In the Inspector, you'll see lists for Human, Elf, Devil, and Skeleton classes
   - Click the "+" button to add new classes to each race
   - Fill in the class details (name, description, stats, etc.)

## Valid Character Combinations:
- **Human:** Warrior, Archer
- **Elf:** Mage, Ranger  
- **Devil:** Warrior, Warlock
- **Skeleton:** Warrior, Necromancer

## What This Fixes:
- ✅ Character selection UI will load properly
- ✅ No more "Could not find SPUMCharacterDatabase" errors
- ✅ Characters will show valid race/class combinations
- ✅ UI scaling and positioning will work correctly

## After Running:
The character selection screen should display properly with:
- A clean, scalable UI layout
- Valid character data (no more "Elf Mage" or "Skeleton Death Knight" errors)
- Proper button positioning and character slot layout
