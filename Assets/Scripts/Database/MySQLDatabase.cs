// NOTE: Wrap MySQL usage so the client/editor builds without the plugin.
// Enable real DB code by defining BO_ENABLE_MYSQL or building with UNITY_SERVER.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if BO_ENABLE_MYSQL || UNITY_SERVER
using MySql.Data.MySqlClient;
#endif

/// <summary>
/// MySQL database operations for BlazedOdyssey MMO.
/// Real implementation is compiled only when BO_ENABLE_MYSQL or UNITY_SERVER is defined.
/// Otherwise a safe stub is used so the client/editor compiles without MySQL dependencies.
/// </summary>
public class MySQLDatabase : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private bool enableDatabase = true;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private int connectionTimeout = 30;
    [SerializeField] private int commandTimeout = 60;

    private DatabaseConfig config;
    private static MySQLDatabase _instance;

    public static MySQLDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MySQLDatabase>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MySQLDatabase");
                    _instance = go.AddComponent<MySQLDatabase>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            config = DatabaseManager.Instance.Config;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (enableDatabase)
        {
            TestConnectionAsync();
        }
        else
        {
            Debug.Log("🔒 MySQL database is disabled");
        }
    }

    public async void TestConnectionAsync()
    {
        bool connected = await TestConnection();
        if (connected) Debug.Log("✅ MySQL database connection successful!");
        else Debug.LogError("❌ MySQL database connection failed!");
    }

#if BO_ENABLE_MYSQL || UNITY_SERVER
    public async Task<bool> TestConnection()
    {
        if (!enableDatabase) return false;

        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT 1", connection))
                {
                    command.CommandTimeout = commandTimeout;
                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Database connection test failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> CheckUsernameExists(string username)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@username", username);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckUsernameExists failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> CheckEmailExists(string email)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = "SELECT COUNT(*) FROM accounts WHERE email = @email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@email", email);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckEmailExists failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> CreateAccount(string username, string email, string passwordHash, string salt)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    INSERT INTO accounts (username, email, password_hash, salt, character_slots, is_active, created_at)
                    VALUES (@username, @email, @passwordHash, @salt, 3, 1, NOW())";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    command.Parameters.AddWithValue("@salt", salt);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CreateAccount failed: {e.Message}");
            return false;
        }
    }

    public async Task<MMOAccountData> GetAccount(string username)
    {
        if (!enableDatabase) return null;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    SELECT id, username, email, character_slots, is_active, is_admin,
                           last_login, last_ip, created_at
                    FROM accounts
                    WHERE username = @username AND is_active = 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new MMOAccountData
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                username = reader.GetString(reader.GetOrdinal("username")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                characterSlots = reader.GetInt32(reader.GetOrdinal("character_slots")),
                                isActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
                                isAdmin = reader.GetBoolean(reader.GetOrdinal("is_admin")),
                                lastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("last_login")),
                                lastIP = reader.IsDBNull(reader.GetOrdinal("last_ip")) ? "" : reader.GetString(reader.GetOrdinal("last_ip")),
                                createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                            };
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetAccount failed: {e.Message}");
        }
        return null;
    }

    public async Task<(string hash, string salt)> GetPasswordData(int accountId)
    {
        if (!enableDatabase) return ("", "");
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = "SELECT password_hash, salt FROM accounts WHERE id = @accountId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@accountId", accountId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return (reader.GetString(reader.GetOrdinal("password_hash")), reader.GetString(reader.GetOrdinal("salt")));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetPasswordData failed: {e.Message}");
        }
        return ("", "");
    }

    public async Task<bool> UpdateLastLogin(int accountId, string ipAddress = "")
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    UPDATE accounts
                    SET last_login = NOW(), last_ip = @ipAddress, updated_at = NOW()
                    WHERE id = @accountId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@ipAddress", ipAddress);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ UpdateLastLogin failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> CheckCharacterNameExists(string characterName)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = "SELECT COUNT(*) FROM characters WHERE character_name = @characterName";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@characterName", characterName);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckCharacterNameExists failed: {e.Message}");
            return false;
        }
    }

    public async Task<List<MMOCharacterData>> GetCharactersForAccount(int accountId)
    {
        var characters = new List<MMOCharacterData>();
        if (!enableDatabase) return characters;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    SELECT * FROM characters
                    WHERE account_id = @accountId
                    ORDER BY last_played DESC";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@accountId", accountId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            characters.Add(ReadCharacterFromReader(reader as MySqlDataReader));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetCharactersForAccount failed: {e.Message}");
        }
        return characters;
    }

    public async Task<int> CreateCharacter(MMOCharacterData character)
    {
        if (!enableDatabase) return -1;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    INSERT INTO characters (
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
                        @accountId, @characterName, @characterClass, @level, @experience,
                        @health, @maxHealth, @mana, @maxMana, @sceneName, @positionX, @positionY, @positionZ,
                        @isMale, @bodyType, @skinColor, @hairIndex, @faceIndex, @eyebrowIndex, @eyesIndex,
                        @noseIndex, @mouthIndex, @beardIndex, @helmetIndex, @armorIndex, @pantsIndex,
                        @shoesIndex, @glovesIndex, @weaponIndex, @shieldIndex, @backIndex,
                        @hairColor, @eyebrowColor, @eyesColor, @beardColor, @helmetColor, @armorColor,
                        @pantsColor, @shoesColor, @glovesColor, @weaponColor, @shieldColor, @backColor,
                        @gold, @statStrength, @statDexterity, @statIntelligence, @statVitality,
                        NOW(), NOW(), NOW()
                    )";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    AddCharacterParameters(command, character);
                    await command.ExecuteNonQueryAsync();
                    return (int)command.LastInsertedId;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CreateCharacter failed: {e.Message}");
            return -1;
        }
    }

    public async Task<bool> UpdateCharacter(MMOCharacterData character)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = @"
                    UPDATE characters SET
                        character_name = @characterName, character_class = @characterClass,
                        level = @level, experience = @experience, health = @health, max_health = @maxHealth,
                        mana = @mana, max_mana = @maxMana, scene_name = @sceneName,
                        position_x = @positionX, position_y = @positionY, position_z = @positionZ,
                        is_male = @isMale, body_type = @bodyType, skin_color = @skinColor,
                        hair_index = @hairIndex, face_index = @faceIndex, eyebrow_index = @eyebrowIndex,
                        eyes_index = @eyesIndex, nose_index = @noseIndex, mouth_index = @mouthIndex,
                        beard_index = @beardIndex, helmet_index = @helmetIndex, armor_index = @armorIndex,
                        pants_index = @pantsIndex, shoes_index = @shoesIndex, gloves_index = @glovesIndex,
                        weapon_index = @weaponIndex, shield_index = @shieldIndex, back_index = @backIndex,
                        hair_color = @hairColor, eyebrow_color = @eyebrowColor, eyes_color = @eyesColor,
                        beard_color = @beardColor, helmet_color = @helmetColor, armor_color = @armorColor,
                        pants_color = @pantsColor, shoes_color = @shoesColor, gloves_color = @glovesColor,
                        weapon_color = @weaponColor, shield_color = @shieldColor, back_color = @backColor,
                        gold = @gold, stat_strength = @statStrength, stat_dexterity = @statDexterity,
                        stat_intelligence = @statIntelligence, stat_vitality = @statVitality,
                        updated_at = NOW(), last_played = NOW()
                    WHERE id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    AddCharacterParameters(command, character);
                    command.Parameters.AddWithValue("@id", character.id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ UpdateCharacter failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCharacter(int characterId)
    {
        if (!enableDatabase) return false;
        try
        {
            using (var connection = new MySqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();
                const string query = "DELETE FROM characters WHERE id = @characterId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddWithValue("@characterId", characterId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ DeleteCharacter failed: {e.Message}");
            return false;
        }
    }

    private void AddCharacterParameters(MySqlCommand command, MMOCharacterData character)
    {
        command.Parameters.AddWithValue("@accountId", character.accountId);
        command.Parameters.AddWithValue("@characterName", character.characterName);
        command.Parameters.AddWithValue("@characterClass", character.characterClass);
        command.Parameters.AddWithValue("@level", character.level);
        command.Parameters.AddWithValue("@experience", character.experience);
        command.Parameters.AddWithValue("@health", character.health);
        command.Parameters.AddWithValue("@maxHealth", character.maxHealth);
        command.Parameters.AddWithValue("@mana", character.mana);
        command.Parameters.AddWithValue("@maxMana", character.maxMana);
        command.Parameters.AddWithValue("@sceneName", character.sceneName);
        command.Parameters.AddWithValue("@positionX", character.positionX);
        command.Parameters.AddWithValue("@positionY", character.positionY);
        command.Parameters.AddWithValue("@positionZ", character.positionZ);
        command.Parameters.AddWithValue("@isMale", character.isMale);
        command.Parameters.AddWithValue("@bodyType", character.bodyType);
        command.Parameters.AddWithValue("@skinColor", character.skinColor);
        command.Parameters.AddWithValue("@hairIndex", character.hairIndex);
        command.Parameters.AddWithValue("@faceIndex", character.faceIndex);
        command.Parameters.AddWithValue("@eyebrowIndex", character.eyebrowIndex);
        command.Parameters.AddWithValue("@eyesIndex", character.eyesIndex);
        command.Parameters.AddWithValue("@noseIndex", character.noseIndex);
        command.Parameters.AddWithValue("@mouthIndex", character.mouthIndex);
        command.Parameters.AddWithValue("@beardIndex", character.beardIndex);
        command.Parameters.AddWithValue("@helmetIndex", character.helmetIndex);
        command.Parameters.AddWithValue("@armorIndex", character.armorIndex);
        command.Parameters.AddWithValue("@pantsIndex", character.pantsIndex);
        command.Parameters.AddWithValue("@shoesIndex", character.shoesIndex);
        command.Parameters.AddWithValue("@glovesIndex", character.glovesIndex);
        command.Parameters.AddWithValue("@weaponIndex", character.weaponIndex);
        command.Parameters.AddWithValue("@shieldIndex", character.shieldIndex);
        command.Parameters.AddWithValue("@backIndex", character.backIndex);
        command.Parameters.AddWithValue("@hairColor", character.hairColor);
        command.Parameters.AddWithValue("@eyebrowColor", character.eyebrowColor);
        command.Parameters.AddWithValue("@eyesColor", character.eyesColor);
        command.Parameters.AddWithValue("@beardColor", character.beardColor);
        command.Parameters.AddWithValue("@helmetColor", character.helmetColor);
        command.Parameters.AddWithValue("@armorColor", character.armorColor);
        command.Parameters.AddWithValue("@pantsColor", character.pantsColor);
        command.Parameters.AddWithValue("@shoesColor", character.shoesColor);
        command.Parameters.AddWithValue("@glovesColor", character.glovesColor);
        command.Parameters.AddWithValue("@weaponColor", character.weaponColor);
        command.Parameters.AddWithValue("@shieldColor", character.shieldColor);
        command.Parameters.AddWithValue("@backColor", character.backColor);
        command.Parameters.AddWithValue("@gold", character.gold);
        command.Parameters.AddWithValue("@statStrength", character.statStrength);
        command.Parameters.AddWithValue("@statDexterity", character.statDexterity);
        command.Parameters.AddWithValue("@statIntelligence", character.statIntelligence);
        command.Parameters.AddWithValue("@statVitality", character.statVitality);
    }

    private MMOCharacterData ReadCharacterFromReader(MySqlDataReader reader)
    {
        return new MMOCharacterData
        {
            id = reader.GetInt32(reader.GetOrdinal("id")),
            accountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            characterName = reader.GetString(reader.GetOrdinal("character_name")),
            characterClass = reader.GetString(reader.GetOrdinal("character_class")),
            level = reader.GetInt32(reader.GetOrdinal("level")),
            experience = reader.GetInt32(reader.GetOrdinal("experience")),
            health = reader.GetInt32(reader.GetOrdinal("health")),
            maxHealth = reader.GetInt32(reader.GetOrdinal("max_health")),
            mana = reader.GetInt32(reader.GetOrdinal("mana")),
            maxMana = reader.GetInt32(reader.GetOrdinal("max_mana")),
            sceneName = reader.GetString(reader.GetOrdinal("scene_name")),
            positionX = reader.GetFloat(reader.GetOrdinal("position_x")),
            positionY = reader.GetFloat(reader.GetOrdinal("position_y")),
            positionZ = reader.GetFloat(reader.GetOrdinal("position_z")),
            isMale = reader.GetBoolean(reader.GetOrdinal("is_male")),
            bodyType = reader.GetString(reader.GetOrdinal("body_type")),
            skinColor = reader.GetString(reader.GetOrdinal("skin_color")),
            hairIndex = reader.GetInt32(reader.GetOrdinal("hair_index")),
            faceIndex = reader.GetInt32(reader.GetOrdinal("face_index")),
            eyebrowIndex = reader.GetInt32(reader.GetOrdinal("eyebrow_index")),
            eyesIndex = reader.GetInt32(reader.GetOrdinal("eyes_index")),
            noseIndex = reader.GetInt32(reader.GetOrdinal("nose_index")),
            mouthIndex = reader.GetInt32(reader.GetOrdinal("mouth_index")),
            beardIndex = reader.GetInt32(reader.GetOrdinal("beard_index")),
            helmetIndex = reader.GetInt32(reader.GetOrdinal("helmet_index")),
            armorIndex = reader.GetInt32(reader.GetOrdinal("armor_index")),
            pantsIndex = reader.GetInt32(reader.GetOrdinal("pants_index")),
            shoesIndex = reader.GetInt32(reader.GetOrdinal("shoes_index")),
            glovesIndex = reader.GetInt32(reader.GetOrdinal("gloves_index")),
            weaponIndex = reader.GetInt32(reader.GetOrdinal("weapon_index")),
            shieldIndex = reader.GetInt32(reader.GetOrdinal("shield_index")),
            backIndex = reader.GetInt32(reader.GetOrdinal("back_index")),
            hairColor = reader.GetString(reader.GetOrdinal("hair_color")),
            eyebrowColor = reader.GetString(reader.GetOrdinal("eyebrow_color")),
            eyesColor = reader.GetString(reader.GetOrdinal("eyes_color")),
            beardColor = reader.GetString(reader.GetOrdinal("beard_color")),
            helmetColor = reader.GetString(reader.GetOrdinal("helmet_color")),
            armorColor = reader.GetString(reader.GetOrdinal("armor_color")),
            pantsColor = reader.GetString(reader.GetOrdinal("pants_color")),
            shoesColor = reader.GetString(reader.GetOrdinal("shoes_color")),
            glovesColor = reader.GetString(reader.GetOrdinal("gloves_color")),
            weaponColor = reader.GetString(reader.GetOrdinal("weapon_color")),
            shieldColor = reader.GetString(reader.GetOrdinal("shield_color")),
            backColor = reader.GetString(reader.GetOrdinal("back_color")),
            gold = reader.GetInt32(reader.GetOrdinal("gold")),
            statStrength = reader.GetInt32(reader.GetOrdinal("stat_strength")),
            statDexterity = reader.GetInt32(reader.GetOrdinal("stat_dexterity")),
            statIntelligence = reader.GetInt32(reader.GetOrdinal("stat_intelligence")),
            statVitality = reader.GetInt32(reader.GetOrdinal("stat_vitality")),
            createdAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            lastPlayed = reader.GetDateTime(reader.GetOrdinal("last_played"))
        };
    }

#else
    // ===== Stub implementation when MySQL is disabled =====
    public Task<bool> TestConnection() { Warn(); return Task.FromResult(false); }
    public Task<bool> CheckUsernameExists(string username) { Warn(); return Task.FromResult(false); }
    public Task<bool> CheckEmailExists(string email) { Warn(); return Task.FromResult(false); }
    public Task<bool> CreateAccount(string username, string email, string passwordHash, string salt) { Warn(); return Task.FromResult(false); }
    public Task<MMOAccountData> GetAccount(string username) { Warn(); return Task.FromResult<MMOAccountData>(null); }
    public Task<(string hash, string salt)> GetPasswordData(int accountId) { Warn(); return Task.FromResult(("", "")); }
    public Task<bool> UpdateLastLogin(int accountId, string ipAddress = "") { Warn(); return Task.FromResult(false); }
    public Task<bool> CheckCharacterNameExists(string characterName) { Warn(); return Task.FromResult(false); }
    public Task<List<MMOCharacterData>> GetCharactersForAccount(int accountId) { Warn(); return Task.FromResult(new List<MMOCharacterData>()); }
    public Task<int> CreateCharacter(MMOCharacterData character) { Warn(); return Task.FromResult(-1); }
    public Task<bool> UpdateCharacter(MMOCharacterData character) { Warn(); return Task.FromResult(false); }
    public Task<bool> DeleteCharacter(int characterId) { Warn(); return Task.FromResult(false); }

    private void Warn()
    {
        if (debugMode)
        {
            Debug.LogWarning("MySQL disabled. Define BO_ENABLE_MYSQL or build with UNITY_SERVER to enable DB operations.");
        }
    }
#endif
}