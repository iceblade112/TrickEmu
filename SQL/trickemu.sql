SET FOREIGN_KEY_CHECKS=0;

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
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=100015585 DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL DEFAULT '0',
  `password` varchar(255) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

INSERT INTO `users` VALUES ('1', 'gm001', '111111');
