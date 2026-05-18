/*
  DemoWebApi - learn database bootstrap script
  Target: MySQL 8.x
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE DATABASE IF NOT EXISTS `learn`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE `learn`;

-- ----------------------------
-- Table: thing
-- ----------------------------
DROP TABLE IF EXISTS `thing`;
CREATE TABLE `thing` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'thing id',
  `color` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'color',
  `price` decimal(10,2) DEFAULT NULL COMMENT 'price',
  `number` int DEFAULT NULL COMMENT 'quantity',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'description',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `thing` (`id`, `color`, `price`, `number`, `description`) VALUES
(1, 'Black', 9999.00, 2, 'Apple Computer'),
(2, 'Red', 3999.00, 1, 'Bed'),
(3, 'Yellow', 44.00, 4, 'Desk');

-- ----------------------------
-- Table: room
-- ----------------------------
DROP TABLE IF EXISTS `room`;
CREATE TABLE `room` (
  `id` tinyint NOT NULL AUTO_INCREMENT COMMENT 'id',
  `computer_id` int DEFAULT NULL COMMENT 'computer thing id',
  `bed_id` int DEFAULT NULL COMMENT 'bed thing id',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `room` (`id`, `computer_id`, `bed_id`) VALUES
(5, 1, 2),
(7, 1, 3);

-- ----------------------------
-- Table: users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_name` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `age` int NOT NULL,
  `password_hash` varchar(255) NOT NULL COMMENT 'PBKDF2 hash',
  `role` int NOT NULL DEFAULT 0 COMMENT '0=NormalUser, 1=SuperAdmin',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `is_active` tinyint(1) NOT NULL DEFAULT 1,
  `last_login_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `UX_users_email` (`email`),
  UNIQUE KEY `UX_users_user_name` (`user_name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- This hash is a placeholder.
-- App startup will overwrite superadmin password hash using appsettings SuperAdmin.Password.
INSERT INTO `users`
(`id`, `user_name`, `email`, `age`, `password_hash`, `role`, `created_at`, `is_active`, `last_login_at`)
VALUES
(1, 'superadmin', 'superadmin@local', 30, 'INIT_HASH_WILL_BE_REPLACED_ON_STARTUP', 1, NOW(), 1, NULL);

SET FOREIGN_KEY_CHECKS = 1;
