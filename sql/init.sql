CREATE DATABASE IF NOT EXISTS indy_bot;

USE indy_bot;

CREATE USER IF NOT EXISTS 'bot'@'localhost' IDENTIFIED BY 'indy_pass';
GRANT ALL PRIVILEGES ON indy_bot.* TO 'bot'@'localhost';

CREATE TABLE IF NOT EXISTS guild (
   id BIGINT PRIMARY KEY,
   default_channel BIGINT NOT NULL,
   log BIGINT,
   auto_entry BIGINT,
   group_entry BIGINT
);

CREATE TABLE IF NOT EXISTS user (
   id BIGINT PRIMARY KEY,
   name VARCHAR(50) NOT NULL,
   password VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS user_guild (
   user_id BIGINT,
   guild_id BIGINT,

   PRIMARY KEY(user_id, guild_id),
   FOREIGN KEY(guild_id) REFERENCES guild(id)
);

CREATE TABLE IF NOT EXISTS user_standard (
   id BIGINT,
   type VARCHAR(30),
   value VARCHAR(100) NOT NULL,

   PRIMARY KEY(id, type)
);

CREATE TABLE IF NOT EXISTS auto_entry (
   id BIGINT PRIMARY KEY,
   time TIME NOT NULL,
   status VARCHAR(30) NOT NULL,

   FOREIGN KEY(id) REFERENCES user(id)
);
