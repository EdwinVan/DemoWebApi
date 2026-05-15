/*
 Navicat Premium Dump SQL

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 80046 (8.0.46)
 Source Host           : localhost:3306
 Source Schema         : learn

 Target Server Type    : MySQL
 Target Server Version : 80046 (8.0.46)
 File Encoding         : 65001

 Date: 15/05/2026 17:53:59
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for room
-- ----------------------------
DROP TABLE IF EXISTS `room`;
CREATE TABLE `room`  (
  `id` tinyint NOT NULL AUTO_INCREMENT COMMENT 'id',
  `computer_id` int NULL DEFAULT NULL COMMENT '电脑id',
  `bed_id` int NULL DEFAULT NULL COMMENT '床id',
  PRIMARY KEY (`id` DESC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_bin ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of room
-- ----------------------------
INSERT INTO `room` VALUES (5, 1, 2);

-- ----------------------------
-- Table structure for thing
-- ----------------------------
DROP TABLE IF EXISTS `thing`;
CREATE TABLE `thing`  (
  `id` int NOT NULL COMMENT '物品id',
  `color` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin NULL DEFAULT NULL COMMENT '颜色',
  `price` decimal(10, 2) NULL DEFAULT NULL COMMENT '价格',
  `number` int NULL DEFAULT NULL COMMENT '数量',
  `description` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin NULL DEFAULT NULL COMMENT '描述',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_bin ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of thing
-- ----------------------------
INSERT INTO `thing` VALUES (1, 'black', 9999.00, 2, '电脑');
INSERT INTO `thing` VALUES (2, 'red', 3999.00, 1, '床');

SET FOREIGN_KEY_CHECKS = 1;
