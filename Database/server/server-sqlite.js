const express = require('express');
const sqlite3 = require('sqlite3').verbose();
const cors = require('cors');
const bodyParser = require('body-parser');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// SQLite database
const dbPath = path.join(__dirname, 'blazed_odyssey.db');
const db = new sqlite3.Database(dbPath);

// Initialize database tables
db.serialize(() => {
  // Accounts table
  db.run(`CREATE TABLE IF NOT EXISTS accounts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    email TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    salt TEXT NOT NULL,
    character_slots INTEGER DEFAULT 3,
    is_active BOOLEAN DEFAULT 1,
    is_admin BOOLEAN DEFAULT 0,
    last_login DATETIME,
    last_ip TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
  )`);

  // Characters table
  db.run(`CREATE TABLE IF NOT EXISTS characters (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    account_id INTEGER NOT NULL,
    character_name TEXT UNIQUE NOT NULL,
    character_class TEXT NOT NULL,
    level INTEGER DEFAULT 1,
    experience INTEGER DEFAULT 0,
    health INTEGER DEFAULT 100,
    max_health INTEGER DEFAULT 100,
    mana INTEGER DEFAULT 100,
    max_mana INTEGER DEFAULT 100,
    scene_name TEXT DEFAULT 'TownCenter',
    position_x REAL DEFAULT 0,
    position_y REAL DEFAULT 0,
    position_z REAL DEFAULT 0,
    is_male BOOLEAN DEFAULT 1,
    body_type INTEGER DEFAULT 0,
    skin_color TEXT DEFAULT '#FFFFFF',
    hair_index INTEGER DEFAULT 0,
    face_index INTEGER DEFAULT 0,
    eyebrow_index INTEGER DEFAULT 0,
    eyes_index INTEGER DEFAULT 0,
    nose_index INTEGER DEFAULT 0,
    mouth_index INTEGER DEFAULT 0,
    beard_index INTEGER DEFAULT 0,
    helmet_index INTEGER DEFAULT 0,
    armor_index INTEGER DEFAULT 0,
    pants_index INTEGER DEFAULT 0,
    shoes_index INTEGER DEFAULT 0,
    gloves_index INTEGER DEFAULT 0,
    weapon_index INTEGER DEFAULT 0,
    shield_index INTEGER DEFAULT 0,
    back_index INTEGER DEFAULT 0,
    hair_color TEXT DEFAULT '#FFFFFF',
    eyebrow_color TEXT DEFAULT '#FFFFFF',
    eyes_color TEXT DEFAULT '#FFFFFF',
    beard_color TEXT DEFAULT '#FFFFFF',
    helmet_color TEXT DEFAULT '#FFFFFF',
    armor_color TEXT DEFAULT '#FFFFFF',
    pants_color TEXT DEFAULT '#FFFFFF',
    shoes_color TEXT DEFAULT '#FFFFFF',
    gloves_color TEXT DEFAULT '#FFFFFF',
    weapon_color TEXT DEFAULT '#FFFFFF',
    shield_color TEXT DEFAULT '#FFFFFF',
    back_color TEXT DEFAULT '#FFFFFF',
    gold INTEGER DEFAULT 0,
    stat_strength INTEGER DEFAULT 10,
    stat_dexterity INTEGER DEFAULT 10,
    stat_intelligence INTEGER DEFAULT 10,
    stat_vitality INTEGER DEFAULT 10,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_played DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES accounts (id)
  )`);
});

// Logging middleware
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.path}`);
  next();
});

// Health check endpoint
app.get('/api/health', (req, res) => {
  res.json({
    status: 'healthy',
    message: 'SQLite database connection successful',
    timestamp: new Date().toISOString()
  });
});

// Account endpoints
app.get('/api/accounts/check-username/:username', (req, res) => {
  const { username } = req.params;
  
  db.get('SELECT COUNT(*) as count FROM accounts WHERE username = ?', [username], (err, row) => {
    if (err) {
      console.error('Check username error:', err);
      return res.status(500).json({ error: 'Database error' });
    }
    
    res.json({ exists: row.count > 0 });
  });
});

app.get('/api/accounts/check-email/:email', (req, res) => {
  const { email } = req.params;
  
  db.get('SELECT COUNT(*) as count FROM accounts WHERE email = ?', [email], (err, row) => {
    if (err) {
      console.error('Check email error:', err);
      return res.status(500).json({ error: 'Database error' });
    }
    
    res.json({ exists: row.count > 0 });
  });
});

app.post('/api/accounts', (req, res) => {
  const { username, email, passwordHash, salt } = req.body;
  
  console.log(`📝 Creating account: ${username}, ${email}`);
  
  db.run(
    `INSERT INTO accounts (username, email, password_hash, salt, character_slots, is_active, created_at) 
     VALUES (?, ?, ?, ?, 3, 1, datetime('now'))`,
    [username, email, passwordHash, salt],
    function(err) {
      if (err) {
        console.error('Create account error:', err);
        return res.status(500).json({ error: 'Failed to create account' });
      }
      
      console.log(`✅ Account created with ID: ${this.lastID}`);
      res.json({
        success: true,
        accountId: this.lastID,
        message: 'Account created successfully'
      });
    }
  );
});

app.get('/api/accounts/:username', (req, res) => {
  const { username } = req.params;
  
  db.get(
    `SELECT id, username, email, character_slots, is_active, is_admin, 
            last_login, last_ip, created_at 
     FROM accounts 
     WHERE username = ? AND is_active = 1`,
    [username],
    (err, row) => {
      if (err) {
        console.error('Get account error:', err);
        return res.status(500).json({ error: 'Database error' });
      }
      
      if (!row) {
        return res.status(404).json({ error: 'Account not found' });
      }
      
      res.json({
        id: row.id,
        username: row.username,
        email: row.email,
        characterSlots: row.character_slots,
        isActive: row.is_active === 1,
        isAdmin: row.is_admin === 1,
        lastLogin: row.last_login,
        lastIP: row.last_ip,
        createdAt: row.created_at
      });
    }
  );
});

app.get('/api/accounts/:accountId/password', (req, res) => {
  const { accountId } = req.params;
  
  db.get(
    'SELECT password_hash, salt FROM accounts WHERE id = ?',
    [accountId],
    (err, row) => {
      if (err) {
        console.error('Get password data error:', err);
        return res.status(500).json({ error: 'Database error' });
      }
      
      if (!row) {
        return res.status(404).json({ error: 'Account not found' });
      }
      
      res.json({
        hash: row.password_hash,
        salt: row.salt
      });
    }
  );
});

app.put('/api/accounts/login', (req, res) => {
  const { accountId, ipAddress } = req.body;
  
  db.run(
    `UPDATE accounts 
     SET last_login = datetime('now'), last_ip = ?, updated_at = datetime('now') 
     WHERE id = ?`,
    [ipAddress || '', accountId],
    function(err) {
      if (err) {
        console.error('Update login error:', err);
        return res.status(500).json({ error: 'Database error' });
      }
      
      res.json({ success: true, message: 'Login updated' });
    }
  );
});

// Character endpoints
app.get('/api/characters/check-name/:characterName', (req, res) => {
  const { characterName } = req.params;
  
  db.get('SELECT COUNT(*) as count FROM characters WHERE character_name = ?', [characterName], (err, row) => {
    if (err) {
      console.error('Check character name error:', err);
      return res.status(500).json({ error: 'Database error' });
    }
    
    res.json({ exists: row.count > 0 });
  });
});

app.get('/api/characters/account/:accountId', (req, res) => {
  const { accountId } = req.params;
  
  db.all(
    'SELECT * FROM characters WHERE account_id = ? ORDER BY last_played DESC',
    [accountId],
    (err, rows) => {
      if (err) {
        console.error('Get characters error:', err);
        return res.status(500).json({ error: 'Database error' });
      }
      
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
    }
  );
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
  console.log(`🚀 BlazedOdyssey SQLite API Server running on port ${PORT}`);
  console.log(`📊 Health check: http://localhost:${PORT}/api/health`);
  console.log(`🗄️ Database: SQLite at ${dbPath}`);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Shutting down server...');
  db.close((err) => {
    if (err) {
      console.error(err.message);
    }
    console.log('SQLite database connection closed.');
    process.exit(0);
  });
});

process.on('SIGTERM', () => {
  console.log('\n🛑 Shutting down server...');
  db.close((err) => {
    if (err) {
      console.error(err.message);
    }
    console.log('SQLite database connection closed.');
    process.exit(0);
  });
});