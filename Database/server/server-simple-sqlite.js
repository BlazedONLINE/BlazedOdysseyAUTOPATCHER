const express = require('express');
const fs = require('fs');
const path = require('path');
const cors = require('cors');
const bodyParser = require('body-parser');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Simple file-based database (JSON)
const dbPath = path.join(__dirname, 'blazed_odyssey_data.json');

// Initialize database file
if (!fs.existsSync(dbPath)) {
  const initialData = {
    accounts: [],
    characters: []
  };
  fs.writeFileSync(dbPath, JSON.stringify(initialData, null, 2));
}

// Helper functions
function readDatabase() {
  try {
    const data = fs.readFileSync(dbPath, 'utf8');
    return JSON.parse(data);
  } catch (error) {
    console.error('Error reading database:', error);
    return { accounts: [], characters: [] };
  }
}

function writeDatabase(data) {
  try {
    fs.writeFileSync(dbPath, JSON.stringify(data, null, 2));
    return true;
  } catch (error) {
    console.error('Error writing database:', error);
    return false;
  }
}

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
  const db = readDatabase();
  
  const exists = db.accounts.some(account => account.username === username);
  res.json({ exists });
});

app.get('/api/accounts/check-email/:email', (req, res) => {
  const { email } = req.params;
  const db = readDatabase();
  
  const exists = db.accounts.some(account => account.email === email);
  res.json({ exists });
});

app.post('/api/accounts', (req, res) => {
  const { username, email, passwordHash, salt } = req.body;
  
  console.log(`📝 Creating account: ${username}, ${email}`);
  
  const db = readDatabase();
  
  // Check if username or email already exists
  const usernameExists = db.accounts.some(account => account.username === username);
  const emailExists = db.accounts.some(account => account.email === email);
  
  if (usernameExists) {
    console.log(`❌ Username ${username} already exists`);
    return res.status(400).json({ error: 'Username already exists' });
  }
  
  if (emailExists) {
    console.log(`❌ Email ${email} already exists`);
    return res.status(400).json({ error: 'Email already registered' });
  }
  
  // Create new account
  const newAccount = {
    id: db.accounts.length + 1,
    username,
    email,
    password_hash: passwordHash,
    salt,
    character_slots: 3,
    is_active: true,
    is_admin: false,
    last_login: null,
    last_ip: null,
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString()
  };
  
  db.accounts.push(newAccount);
  
  if (writeDatabase(db)) {
    console.log(`✅ Account created with ID: ${newAccount.id}`);
    res.json({
      success: true,
      accountId: newAccount.id,
      message: 'Account created successfully'
    });
  } else {
    console.log(`❌ Failed to save account ${username}`);
    res.status(500).json({ error: 'Failed to create account' });
  }
});

app.get('/api/accounts/:username', (req, res) => {
  const { username } = req.params;
  const db = readDatabase();
  
  const account = db.accounts.find(acc => acc.username === username && acc.is_active);
  
  if (!account) {
    return res.status(404).json({ error: 'Account not found' });
  }
  
  res.json({
    id: account.id,
    username: account.username,
    email: account.email,
    characterSlots: account.character_slots,
    isActive: account.is_active,
    isAdmin: account.is_admin,
    lastLogin: account.last_login,
    lastIP: account.last_ip,
    createdAt: account.created_at
  });
});

app.get('/api/accounts/:accountId/password', (req, res) => {
  const { accountId } = req.params;
  const db = readDatabase();
  
  const account = db.accounts.find(acc => acc.id === parseInt(accountId));
  
  if (!account) {
    return res.status(404).json({ error: 'Account not found' });
  }
  
  res.json({
    hash: account.password_hash,
    salt: account.salt
  });
});

app.put('/api/accounts/login', (req, res) => {
  const { accountId, ipAddress } = req.body;
  const db = readDatabase();
  
  const accountIndex = db.accounts.findIndex(acc => acc.id === accountId);
  
  if (accountIndex === -1) {
    return res.status(404).json({ error: 'Account not found' });
  }
  
  db.accounts[accountIndex].last_login = new Date().toISOString();
  db.accounts[accountIndex].last_ip = ipAddress || '';
  db.accounts[accountIndex].updated_at = new Date().toISOString();
  
  if (writeDatabase(db)) {
    res.json({ success: true, message: 'Login updated' });
  } else {
    res.status(500).json({ error: 'Database error' });
  }
});

// Character endpoints
app.get('/api/characters/check-name/:characterName', (req, res) => {
  const { characterName } = req.params;
  const db = readDatabase();
  
  const exists = db.characters.some(char => char.character_name === characterName);
  res.json({ exists });
});

app.get('/api/characters/account/:accountId', (req, res) => {
  const { accountId } = req.params;
  const db = readDatabase();
  
  const characters = db.characters
    .filter(char => char.account_id === parseInt(accountId))
    .sort((a, b) => new Date(b.last_played) - new Date(a.last_played))
    .map(row => ({
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
      isMale: row.is_male,
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
  console.log(`🚀 BlazedOdyssey Simple File-Based API Server running on port ${PORT}`);
  console.log(`📊 Health check: http://localhost:${PORT}/api/health`);
  console.log(`🗄️ Database: JSON file at ${dbPath}`);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Shutting down server...');
  process.exit(0);
});

process.on('SIGTERM', () => {
  console.log('\n🛑 Shutting down server...');
  process.exit(0);
});