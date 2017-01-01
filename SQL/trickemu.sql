SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for characters
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `level` int(3) DEFAULT '0',
  `money` int(10) unsigned DEFAULT '0',
  `health` int(11) DEFAULT '100',
  `mana` int(11) DEFAULT '100',
  `map` int(11) DEFAULT '33',
  `pos_x` int(11) DEFAULT '768',
  `pos_y` int(11) DEFAULT '768',
  `job` int(2) NOT NULL,
  `type` int(2) NOT NULL,
  `ftype` int(2) NOT NULL,
  `hair` int(2) NOT NULL,
  `build` varchar(7) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=100000001 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for char_equip
-- ----------------------------
DROP TABLE IF EXISTS `char_equip`;
CREATE TABLE `char_equip` (
  `id` int(11) NOT NULL,
  `ears` bigint(11) DEFAULT '0',
  `tail` bigint(11) DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for item_common
-- ----------------------------
DROP TABLE IF EXISTS `item_common`;
CREATE TABLE `item_common` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `owner` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `item_count` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2010000001 DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for item_drill
-- ----------------------------
DROP TABLE IF EXISTS `item_drill`;
CREATE TABLE `item_drill` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `owner` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `item_life` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2020000001 DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for item_rare
-- ----------------------------
DROP TABLE IF EXISTS `item_rare`;
CREATE TABLE `item_rare` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `owner` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `wearing` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2030000001 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL DEFAULT '0',
  `password` varchar(255) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` VALUES ('1', 'gm001', '111111');

SET FOREIGN_KEY_CHECKS=1;
