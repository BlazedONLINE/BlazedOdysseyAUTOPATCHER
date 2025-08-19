-- BlazedOdyssey MMO Characters Table
-- This table stores character data with full SPUM integration
-- Import this into your blazed_odyssey_db database

CREATE TABLE IF NOT EXISTS `characters` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `account_id` INT(11) NOT NULL,
  `character_name` VARCHAR(50) NOT NULL,
  `character_class` VARCHAR(50) NOT NULL DEFAULT 'Vanguard Knight',
  `level` INT(11) DEFAULT 1,
  `experience` INT(11) DEFAULT 0,
  `health` INT(11) DEFAULT 100,
  `max_health` INT(11) DEFAULT 100,
  `mana` INT(11) DEFAULT 100,
  `max_mana` INT(11) DEFAULT 100,
  
  -- Position data
  `scene_name` VARCHAR(100) DEFAULT 'StarterMap',
  `position_x` FLOAT DEFAULT 0.0,
  `position_y` FLOAT DEFAULT 0.0,
  `position_z` FLOAT DEFAULT 0.0,
  
  -- SPUM Character Appearance Data
  `is_male` BOOLEAN DEFAULT TRUE,
  `body_type` VARCHAR(50) DEFAULT 'Human',
  `skin_color` VARCHAR(7) DEFAULT '#FFDBAC',
  
  -- Equipment/Appearance indices for SPUM system
  `hair_index` INT(11) DEFAULT 0,
  `face_index` INT(11) DEFAULT 0,
  `eyebrow_index` INT(11) DEFAULT 0,
  `eyes_index` INT(11) DEFAULT 0,
  `nose_index` INT(11) DEFAULT 0,
  `mouth_index` INT(11) DEFAULT 0,
  `beard_index` INT(11) DEFAULT -1,
  
  -- Equipment indices
  `helmet_index` INT(11) DEFAULT -1,
  `armor_index` INT(11) DEFAULT 0,
  `pants_index` INT(11) DEFAULT 0,
  `shoes_index` INT(11) DEFAULT 0,
  `gloves_index` INT(11) DEFAULT -1,
  `weapon_index` INT(11) DEFAULT -1,
  `shield_index` INT(11) DEFAULT -1,
  `back_index` INT(11) DEFAULT -1,
  
  -- Character colors (hex values)
  `hair_color` VARCHAR(7) DEFAULT '#8B4513',
  `eyebrow_color` VARCHAR(7) DEFAULT '#8B4513',
  `eyes_color` VARCHAR(7) DEFAULT '#4169E1',
  `beard_color` VARCHAR(7) DEFAULT '#8B4513',
  
  -- Equipment colors
  `helmet_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `armor_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `pants_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `shoes_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `gloves_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `weapon_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `shield_color` VARCHAR(7) DEFAULT '#FFFFFF',
  `back_color` VARCHAR(7) DEFAULT '#FFFFFF',
  
  -- Character stats
  `gold` INT(11) DEFAULT 100,
  `stat_strength` INT(11) DEFAULT 10,
  `stat_dexterity` INT(11) DEFAULT 10,
  `stat_intelligence` INT(11) DEFAULT 10,
  `stat_vitality` INT(11) DEFAULT 10,
  
  -- Timestamps
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `last_played` DATETIME DEFAULT CURRENT_TIMESTAMP,
  
  PRIMARY KEY (`id`),
  FOREIGN KEY (`account_id`) REFERENCES `accounts`(`id`) ON DELETE CASCADE,
  UNIQUE KEY `unique_character_name` (`character_name`),
  INDEX `idx_account_characters` (`account_id`),
  INDEX `idx_character_name` (`character_name`),
  INDEX `idx_last_played` (`last_played`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add constraint to limit characters per account
DELIMITER //
CREATE TRIGGER check_character_limit
  BEFORE INSERT ON characters
  FOR EACH ROW
BEGIN
  DECLARE char_count INT;
  DECLARE max_slots INT;
  
  SELECT COUNT(*) INTO char_count 
  FROM characters 
  WHERE account_id = NEW.account_id;
  
  SELECT character_slots INTO max_slots 
  FROM accounts 
  WHERE id = NEW.account_id;
  
  IF char_count >= max_slots THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Character limit reached for this account';
  END IF;
END//
DELIMITER ;