# BlazedOdyssey MMO Database Setup

## Overview
This guide will help you set up the complete MMO authentication and character system for BlazedOdyssey using your Digital Ocean MySQL server.

## Server Information
- **IP**: 129.212.181.87
- **Database**: blazed_odyssey_db  
- **Username**: gm
- **Password**: .bLanch33x
- **Platform**: Ubuntu 22.04 LTS (2GB RAM, 1 vCPU, 70GB Disk)

## Installation Steps

### 1. Import Database Tables
Connect to your MySQL server using DBeaver or command line and run these SQL scripts in order:

```bash
# Connect to your server
mysql -h 129.212.181.87 -u gm -p blazed_odyssey_db
```

Then execute the SQL files:
1. `create_accounts_table.sql` - Creates user accounts table
2. `create_characters_table.sql` - Creates characters table with SPUM integration

### 2. API Server Setup
The MMO system now uses a Node.js API server instead of direct MySQL connections to avoid Unity dependency issues.

**Upload server files to your Digital Ocean droplet:**
```bash
# Copy the entire /Database/server/ folder to your droplet
scp -r ./Database/server/ gm@129.212.181.87:~/blazed-api/
```

**On your droplet, run:**
```bash
cd ~/blazed-api
chmod +x start-server.sh
./start-server.sh
```

This will:
- Install Node.js and npm if needed
- Install API dependencies
- Start MySQL service
- Launch the API server on port 3000

### 3. Configure Connection
The DatabaseConfig.cs file is already configured with your server details:
- Server: 129.212.181.87:3306
- Database: blazed_odyssey_db
- Credentials: gm / .bLanch33x

### 4. Scene Setup
Make sure these components are in your scenes:

**Login Scene:**
- MMOLoginSystem component
- AccountManager (auto-created)
- DatabaseManager (auto-created)

**Character Selection Scene:**
- BlazedCharacterSelector component
- CharacterManager (auto-created)

**Game Scenes:**
- SettingsPanel with backToCharacterSelectButton assigned

## Features Implemented

### Account System
- ✅ Secure account creation with password hashing (PBKDF2)
- ✅ Login authentication
- ✅ Remember me functionality
- ✅ Account validation and error handling

### Character System  
- ✅ Character creation with SPUM appearance data
- ✅ Character selection from database
- ✅ Full character customization storage
- ✅ Character limit enforcement (3 per account)
- ✅ Character name uniqueness validation

### UI Integration
- ✅ Unified login/registration interface
- ✅ Character creation saves to database
- ✅ Character selection loads from database
- ✅ Settings panel "Back to Character Select" button
- ✅ Automatic character data saving on scene transitions

### Security Features
- ✅ Password hashing with salt
- ✅ SQL injection prevention patterns
- ✅ Input validation and sanitization
- ✅ Account lockout protection
- ✅ Secure session management

## Database Schema

### Accounts Table
- User authentication and account management
- Secure password storage with salting
- Character slot limits and permissions
- Login tracking and security

### Characters Table  
- Complete SPUM character appearance data
- Equipment and color customization
- Character stats and progression
- Position and scene tracking
- Foreign key relationship to accounts

## Usage Flow

1. **User Downloads Client** → Sees login screen
2. **Create Account** → New users register with username/email/password
3. **Login** → Existing users authenticate
4. **Character Selection** → Shows characters from database
5. **Character Creation** → New characters saved to database
6. **Gameplay** → Character data auto-saves
7. **Settings Menu** → "Back to Character Select" returns to character list

## Testing

### Default Admin Account
A default admin account is created during setup:
- Username: admin
- Password: admin123 (change immediately!)
- Email: admin@blazedodyssey.com

### Sample Character Data
The system includes sample character data for testing the complete flow.

## Security Notes

⚠️ **Important Security Considerations:**
1. Change the default admin password immediately
2. Consider using SSL connections in production
3. Implement rate limiting for login attempts
4. Regular database backups are recommended
5. Monitor for suspicious login patterns

## Troubleshooting

### Connection Issues
- Verify firewall allows connections on port 3306
- Check MySQL user permissions for remote connections
- Ensure Unity has proper MySQL connector package

### Character Creation Issues
- Verify character name uniqueness constraints
- Check character limit enforcement
- Validate SPUM data serialization

### Authentication Issues
- Check password hashing implementation
- Verify account activation status
- Review session timeout settings

## Support

If you encounter issues:
1. Check Unity Console for detailed error messages
2. Verify database connection in DatabaseManager
3. Check MySQL error logs on your server
4. Ensure all required Unity packages are installed

The system is designed to gracefully handle connection failures and provide meaningful error messages to users.