-- BlazedOdyssey MMO Accounts Table
-- This table stores user account information for the MMO
-- Import this into your blazed_odyssey_db database

CREATE TABLE IF NOT EXISTS `accounts` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(50) NOT NULL UNIQUE,
  `email` VARCHAR(255) NOT NULL UNIQUE,
  `password_hash` VARCHAR(255) NOT NULL,
  `salt` VARCHAR(255) NOT NULL,
  `character_slots` INT(11) DEFAULT 3,
  `is_active` BOOLEAN DEFAULT TRUE,
  `is_admin` BOOLEAN DEFAULT FALSE,
  `last_login` DATETIME NULL,
  `last_ip` VARCHAR(45) NULL,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_username` (`username`),
  INDEX `idx_email` (`email`),
  INDEX `idx_last_login` (`last_login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create admin account (password will be hashed in application)
-- Default admin credentials: admin / admin123 (change immediately after setup)
INSERT INTO `accounts` (`username`, `email`, `password_hash`, `salt`, `is_admin`, `character_slots`) 
VALUES ('admin', 'admin@blazedodyssey.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', TRUE, 10)
ON DUPLICATE KEY UPDATE username = username;