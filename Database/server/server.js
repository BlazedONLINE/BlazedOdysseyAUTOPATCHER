const express = require('express');
const mysql = require('mysql2/promise');
const cors = require('cors');
const bodyParser = require('body-parser');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Database configuration
const dbConfig = {
  host: process.env.DB_HOST || 'localhost',
  port: process.env.DB_PORT || 3306,
  user: process.env.DB_USER || 'gm',
  password: process.env.DB_PASSWORD || '.bLanch33x',
  database: process.env.DB_NAME || 'blazed_odyssey_db',
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0
};

// Create connection pool
const pool = mysql.createPool(dbConfig);

// Logging middleware
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.path}`);
  next();
});

// Health check endpoint
app.get('/api/health', async (req, res) => {
  try {
    const connection = await pool.getConnection();
    await connection.execute('SELECT 1');
    connection.release();
    
    res.json({
      status: 'healthy',
      message: 'Database connection successful',
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    console.error('Health check failed:', error);
    res.status(500).json({
      status: 'unhealthy',
      message: 'Database connection failed',
      error: error.message
    });
  }
});

// Account endpoints
app.get('/api/accounts/check-username/:username', async (req, res) => {
  try {
    const { username } = req.params;
    const [rows] = await pool.execute(
      'SELECT COUNT(*) as count FROM accounts WHERE username = ?',
      [username]
    );
    
    res.json({ exists: rows[0].count > 0 });
  } catch (error) {
    console.error('Check username error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/accounts/check-email/:email', async (req, res) => {
  try {
    const { email } = req.params;
    const [rows] = await pool.execute(
      'SELECT COUNT(*) as count FROM accounts WHERE email = ?',
      [email]
    );
    
    res.json({ exists: rows[0].count > 0 });
  } catch (error) {
    console.error('Check email error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.post('/api/accounts', async (req, res) => {
  try {
    const { username, email, passwordHash, salt } = req.body;
    
    const [result] = await pool.execute(
      `INSERT INTO accounts (username, email, password_hash, salt, character_slots, is_active, created_at) 
       VALUES (?, ?, ?, ?, 3, 1, NOW())`,
      [username, email, passwordHash, salt]
    );
    
    res.json({
      success: true,
      accountId: result.insertId,
      message: 'Account created successfully'
    });
  } catch (error) {
    console.error('Create account error:', error);
    res.status(500).json({ error: 'Failed to create account' });
  }
});

app.get('/api/accounts/:username', async (req, res) => {
  try {
    const { username } = req.params;
    
    const [rows] = await pool.execute(
      `SELECT id, username, email, character_slots, is_active, is_admin, 
              last_login, last_ip, created_at 
       FROM accounts 
       WHERE username = ? AND is_active = 1`,
      [username]
    );
    
    if (rows.length === 0) {
      return res.status(404).json({ error: 'Account not found' });
    }
    
    const account = rows[0];
    res.json({
      id: account.id,
      username: account.username,
      email: account.email,
      characterSlots: account.character_slots,
      isActive: account.is_active === 1,
      isAdmin: account.is_admin === 1,
      lastLogin: account.last_login,
      lastIP: account.last_ip,
      createdAt: account.created_at
    });
  } catch (error) {
    console.error('Get account error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/accounts/:accountId/password', async (req, res) => {
  try {
    const { accountId } = req.params;
    
    const [rows] = await pool.execute(
      'SELECT password_hash, salt FROM accounts WHERE id = ?',
      [accountId]
    );
    
    if (rows.length === 0) {
      return res.status(404).json({ error: 'Account not found' });
    }
    
    res.json({
      hash: rows[0].password_hash,
      salt: rows[0].salt
    });
  } catch (error) {
    console.error('Get password data error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.put('/api/accounts/login', async (req, res) => {
  try {
    const { accountId, ipAddress } = req.body;
    
    await pool.execute(
      `UPDATE accounts 
       SET last_login = NOW(), last_ip = ?, updated_at = NOW() 
       WHERE id = ?`,
      [ipAddress || '', accountId]
    );
    
    res.json({ success: true, message: 'Login updated' });
  } catch (error) {
    console.error('Update login error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

// Character endpoints
app.get('/api/characters/check-name/:characterName', async (req, res) => {
  try {
    const { characterName } = req.params;
    const [rows] = await pool.execute(
      'SELECT COUNT(*) as count FROM characters WHERE character_name = ?',
      [characterName]
    );
    
    res.json({ exists: rows[0].count > 0 });
  } catch (error) {
    console.error('Check character name error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/characters/account/:accountId', async (req, res) => {
  try {
    const { accountId } = req.params;
    
    const [rows] = await pool.execute(
      'SELECT * FROM characters WHERE account_id = ? ORDER BY last_played DESC',
      [accountId]
    );
    
    const characters = rows.map(row => ({
      id: row.id,
      accountId: row.account_id,
      characterName: row.character_name,
      characterClass: row.character_class,
      level: row.level,
      experience: row.experience,
      health: row.health,
      maxHealth: row.max_health,
      mana: row.mana,
      maxMana: row.max_mana,
      sceneName: row.scene_name,
      positionX: row.position_x,
      positionY: row.position_y,
      positionZ: row.position_z,
      isMale: row.is_male === 1,
      bodyType: row.body_type,
      skinColor: row.skin_color,
      hairIndex: row.hair_index,
      faceIndex: row.face_index,
      eyebrowIndex: row.eyebrow_index,
      eyesIndex: row.eyes_index,
      noseIndex: row.nose_index,
      mouthIndex: row.mouth_index,
      beardIndex: row.beard_index,
      helmetIndex: row.helmet_index,
      armorIndex: row.armor_index,
      pantsIndex: row.pants_index,
      shoesIndex: row.shoes_index,
      glovesIndex: row.gloves_index,
      weaponIndex: row.weapon_index,
      shieldIndex: row.shield_index,
      backIndex: row.back_index,
      hairColor: row.hair_color,
      eyebrowColor: row.eyebrow_color,
      eyesColor: row.eyes_color,
      beardColor: row.beard_color,
      helmetColor: row.helmet_color,
      armorColor: row.armor_color,
      pantsColor: row.pants_color,
      shoesColor: row.shoes_color,
      glovesColor: row.gloves_color,
      weaponColor: row.weapon_color,
      shieldColor: row.shield_color,
      backColor: row.back_color,
      gold: row.gold,
      statStrength: row.stat_strength,
      statDexterity: row.stat_dexterity,
      statIntelligence: row.stat_intelligence,
      statVitality: row.stat_vitality,
      createdAt: row.created_at,
      updatedAt: row.updated_at,
      lastPlayed: row.last_played
    }));
    
    res.json({ characters });
  } catch (error) {
    console.error('Get characters error:', error);
    res.status(500).json({ error: 'Database error' });
  }
});

app.post('/api/characters', async (req, res) => {
  try {
    const character = req.body;
    
    const [result] = await pool.execute(
      `INSERT INTO characters (
        account_id, character_name, character_class, level, experience,
        health, max_health, mana, max_mana, scene_name, position_x, position_y, position_z,
        is_male, body_type, skin_color, hair_index, face_index, eyebrow_index, eyes_index,
        nose_index, mouth_index, beard_index, helmet_index, armor_index, pants_index,
        shoes_index, gloves_index, weapon_index, shield_index, back_index,
        hair_color, eyebrow_color, eyes_color, beard_color, helmet_color, armor_color,
        pants_color, shoes_color, gloves_color, weapon_color, shield_color, back_color,
        gold, stat_strength, stat_dexterity, stat_intelligence, stat_vitality,
        created_at, updated_at, last_played
      ) VALUES (
        ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?, ?,
        ?, ?, ?, ?, ?,
        NOW(), NOW(), NOW()
      )`,
      [
        character.accountId, character.characterName, character.characterClass, character.level, character.experience,
        character.health, character.maxHealth, character.mana, character.maxMana, character.sceneName, 
        character.positionX, character.positionY, character.positionZ,
        character.isMale, character.bodyType, character.skinColor, character.hairIndex, character.faceIndex, 
        character.eyebrowIndex, character.eyesIndex, character.noseIndex, character.mouthIndex, character.beardIndex,
        character.helmetIndex, character.armorIndex, character.pantsIndex, character.shoesIndex, character.glovesIndex,
        character.weaponIndex, character.shieldIndex, character.backIndex,
        character.hairColor, character.eyebrowColor, character.eyesColor, character.beardColor, character.helmetColor,
        character.armorColor, character.pantsColor, character.shoesColor, character.glovesColor, character.weaponColor,
        character.shieldColor, character.backColor,
        character.gold, character.statStrength, character.statDexterity, character.statIntelligence, character.statVitality
      ]
    );
    
    res.json({
      success: true,
      characterId: result.insertId,
      message: 'Character created successfully'
    });
  } catch (error) {
    console.error('Create character error:', error);
    res.status(500).json({ error: 'Failed to create character' });
  }
});

app.put('/api/characters/:characterId', async (req, res) => {
  try {
    const { characterId } = req.params;
    const character = req.body;
    
    await pool.execute(
      `UPDATE characters SET
        character_name = ?, character_class = ?, level = ?, experience = ?, health = ?, max_health = ?,
        mana = ?, max_mana = ?, scene_name = ?, position_x = ?, position_y = ?, position_z = ?,
        is_male = ?, body_type = ?, skin_color = ?, hair_index = ?, face_index = ?, eyebrow_index = ?,
        eyes_index = ?, nose_index = ?, mouth_index = ?, beard_index = ?, helmet_index = ?, armor_index = ?,
        pants_index = ?, shoes_index = ?, gloves_index = ?, weapon_index = ?, shield_index = ?, back_index = ?,
        hair_color = ?, eyebrow_color = ?, eyes_color = ?, beard_color = ?, helmet_color = ?, armor_color = ?,
        pants_color = ?, shoes_color = ?, gloves_color = ?, weapon_color = ?, shield_color = ?, back_color = ?,
        gold = ?, stat_strength = ?, stat_dexterity = ?, stat_intelligence = ?, stat_vitality = ?,
        updated_at = NOW(), last_played = NOW()
       WHERE id = ?`,
      [
        character.characterName, character.characterClass, character.level, character.experience, character.health, character.maxHealth,
        character.mana, character.maxMana, character.sceneName, character.positionX, character.positionY, character.positionZ,
        character.isMale, character.bodyType, character.skinColor, character.hairIndex, character.faceIndex, character.eyebrowIndex,
        character.eyesIndex, character.noseIndex, character.mouthIndex, character.beardIndex, character.helmetIndex, character.armorIndex,
        character.pantsIndex, character.shoesIndex, character.glovesIndex, character.weaponIndex, character.shieldIndex, character.backIndex,
        character.hairColor, character.eyebrowColor, character.eyesColor, character.beardColor, character.helmetColor, character.armorColor,
        character.pantsColor, character.shoesColor, character.glovesColor, character.weaponColor, character.shieldColor, character.backColor,
        character.gold, character.statStrength, character.statDexterity, character.statIntelligence, character.statVitality,
        characterId
      ]
    );
    
    res.json({ success: true, message: 'Character updated successfully' });
  } catch (error) {
    console.error('Update character error:', error);
    res.status(500).json({ error: 'Failed to update character' });
  }
});

app.delete('/api/characters/:characterId', async (req, res) => {
  try {
    const { characterId } = req.params;
    
    await pool.execute('DELETE FROM characters WHERE id = ?', [characterId]);
    
    res.json({ success: true, message: 'Character deleted successfully' });
  } catch (error) {
    console.error('Delete character error:', error);
    res.status(500).json({ error: 'Failed to delete character' });
  }
});

// Error handling middleware
app.use((error, req, res, next) => {
  console.error('Unhandled error:', error);
  res.status(500).json({ error: 'Internal server error' });
});

// 404 handler
app.use('*', (req, res) => {
  res.status(404).json({ error: 'Endpoint not found' });
});

// Start server
app.listen(PORT, () => {
  console.log(`🚀 BlazedOdyssey API Server running on port ${PORT}`);
  console.log(`📊 Health check: http://localhost:${PORT}/api/health`);
  console.log(`🗄️ Database: ${dbConfig.host}:${dbConfig.port}/${dbConfig.database}`);
});

// Graceful shutdown
process.on('SIGINT', async () => {
  console.log('\n🛑 Shutting down server...');
  await pool.end();
  process.exit(0);
});

process.on('SIGTERM', async () => {
  console.log('\n🛑 Shutting down server...');
  await pool.end();
  process.exit(0);
});