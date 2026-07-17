CREATE DATABASE IF NOT EXISTS indy_bot;

USE indy_bot;

CREATE USER IF NOT EXISTS 'bot'@'localhost' IDENTIFIED BY 'indy_pass';
GRANT ALL PRIVILEGES ON indy_bot.* TO 'bot'@'localhost';

CREATE TABLE IF NOT EXISTS user (
   id BIGINT PRIMARY KEY,
   name VARCHAR(50) NOT NULL,
   password VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS user_standard (
   id BIGINT,
   type VARCHAR(30),
   value VARCHAR(100) NOT NULL,

   PRIMARY KEY(id, type)
);
