CREATE DATABASE IF NOT EXISTS indy_bot_db;

USE indy_bot_db;

CREATE USER IF NOT EXISTS 'bot'@'localhost' IDENTIFIED BY 'indy_pass';
GRANT ALL PRIVILEGES ON indy_bot_db.* TO 'bot'@'localhost';

CREATE TABLE users (
   id BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
   discord_id BIGINT UNSIGNED UNIQUE NOT NULL,

   whereis_enabled BOOLEAN NOT NULL DEFAULT FALSE,
   whereis_visibility ENUM('Group', 'Server') NOT NULL DEFAULT 'Group',

   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE credentials (
   user_id BIGINT UNSIGNED PRIMARY KEY,

   username VARCHAR(255) NOT NULL,
   password VARCHAR(255) NOT NULL,

   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
);


CREATE TABLE guilds (
   id BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
   discord_id BIGINT UNSIGNED UNIQUE NOT NULL,

   timezone VARCHAR(64) NOT NULL DEFAULT "Europe/Vienna",

   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE guild_features (
   guild_id BIGINT UNSIGNED NOT NULL,
   feature VARCHAR(64) NOT NULL,
   enabled BOOLEAN NOT NULL DEFAULT TRUE,

   PRIMARY KEY(guild_id, feature),
   FOREIGN KEY(guild_id) REFERENCES guilds(id) ON DELETE CASCADE
);


CREATE TABLE groups (
   id BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
   guild_id BIGINT UNSIGNED UNIQUE NOT NULL,
   owner_id BIGINT UNSIGNED UNIQUE NOT NULL,

   name VARCHAR(100) NOT NULL,
   role_id BIGINT UNSIGNED NOT NULL,
   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

   FOREIGN KEY(guild_id) REFERENCES guilds(id),
   FOREIGN KEY(owner_id) REFERENCES users(id)
);

CREATE TABLE group_members (
   group_id BIGINT UNSIGNED,
   user_id BIGINT UNSIGNED,

   role ENUM('Member', 'Moderator', 'Owner') NOT NULL DEFAULT 'Member',
   joined_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

   PRIMARY KEY(group_id, user_id),
   FOREIGN KEY(group_id) REFERENCES guilds(id) ON DELETE CASCADE,
   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE group_invites (
   id BIGINT UNSIGNED PRIMARY KEY,

   group_id BIGINT UNSIGNED UNIQUE NOT NULL,
   inviter_id BIGINT UNSIGNED UNIQUE NOT NULL,
   invitee_id BIGINT UNSIGNED UNIQUE NOT NULL,

   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
   expires_at DATETIME NOT NULL,

   FOREIGN KEY(group_id) REFERENCES groups(id) ON DELETE CASCADE,
   FOREIGN KEY(inviter_id) REFERENCES users(id) ON DELETE CASCADE,
   FOREIGN KEY(invitee_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE group_entries (
   id BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
   group_id BIGINT UNSIGNED NOT NULL,
   creator_id BIGINT UNSIGNED NOT NULL,

   entry_date DATETIME NOT NULL,
   subject VARCHAR(255) NOT NULL,
   teacher VARCHAR(255) NOT NULL,
   description VARCHAR(255) NOT NULL,

   hour3 BOOLEAN NOT NULL,
   hour4 BOOLEAN NOT NULL,

   status ENUM('Open', 'Closed') NOT NULL DEFAULT 'Open',
   created_id DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
   closed_at DATETIME,

   FOREIGN KEY(group_id) REFERENCES groups(id) ON DELETE CASCADE,
   FOREIGN KEY(creator_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE group_entry_members (
   group_entry_id BIGINT UNSIGNED,
   user_id BIGINT UNSIGNED,

   joined_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
   
   PRIMARY KEY(group_entry_id, user_id),

   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
   FOREIGN KEY(group_entry_id) REFERENCES group_entries(id) ON DELETE CASCADE
);

CREATE TABLE group_entry_templates (
   id BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,

   group_id BIGINT UNSIGNED NOT NULL,
   creator_id BIGINT UNSIGNED NOT NULL,

   name VARCHAR(100) NOT NULL,
   subject VARCHAR(100) NOT NULL,
   teacher VARCHAR(100) NOT NULL,
   description VARCHAR(100) NOT NULL,

   hour3 BOOLEAN NOT NULL,
   hour4 BOOLEAN NOT NULL,

   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

   FOREIGN KEY(group_id) REFERENCES groups(id) ON DELETE CASCADE,
   FOREIGN KEY(creator_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE auto_entries (
   user_id BIGINT UNSIGNED,
   indy_day TINYINT CHECK (indy_day IN (1, 3, 5)),
   execution_day TINYINT NOT NULL CHECK (execution_day BETWEEN 0 AND 6),

   subject VARCHAR(100) NOT NULL,
   teacher VARCHAR(100) NOT NULL,
   description VARCHAR(100) NOT NULL,

   PRIMARY KEY(user_id, indy_day),

   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE auto_entry_configs (
   user_id BIGINT UNSIGNED PRIMARY KEY,

   time TIME NOT NULL,
   enabled BOOLEAN NOT NULL DEFAULT FALSE,
   notifications ENUM('Disabled', 'Always', 'On_Error') NOT NULL DEFAULT 'Always',

   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE auto_entry_history (
   user_id BIGINT UNSIGNED,
   executed_at DATETIME(6),

   status ENUM('Success', 'Failed', 'Skipped') NOT NULL,
   entries_created TINYINT NOT NULL CHECK (entries_created BETWEEN 0 AND 2),

   PRIMARY KEY(user_id, executed_at),

   FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
);
