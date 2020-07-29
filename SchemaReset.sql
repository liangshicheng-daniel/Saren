drop table Parameters;
drop table Battles;
drop table Member;
drop table Boss;

CREATE TABLE `Saren_Bot`.`Boss` (
  `boss_id` int(11) NOT NULL AUTO_INCREMENT,
  `event_id` int(11) DEFAULT NULL,
  `boss_code` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `boss_name` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `health_pool` int(11) DEFAULT NULL,
  PRIMARY KEY (`boss_id`),
  UNIQUE KEY `boss_id_UNIQUE` (`boss_id`)
) COMMENT='This table contains basic information for each clan battle bosses.'

CREATE TABLE `Saren_Bot`.`Member` (
  `member_id` bigint(20) NOT NULL,
  `nickname` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `role` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  PRIMARY KEY (`member_id`)
) COMMENT='This tables contains information for all the clan members.'

CREATE TABLE `Saren_Bot`.`Battles` (
  `battle_id` int(11) NOT NULL AUTO_INCREMENT,
  `member_id` bigint(20) DEFAULT NULL,
  `event_id` int(11) DEFAULT NULL,
  `boss_id` int(11) DEFAULT NULL,
  `cycle_number` int(11) DEFAULT NULL,
  `damage` int(11) DEFAULT NULL,
  `status` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `record_time` datetime DEFAULT CURRENT_TIMESTAMP,
  `player_id` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`battle_id`),
  UNIQUE KEY `battle_id_UNIQUE` (`battle_id`),
  KEY `member_id_idx` (`member_id`),
  KEY `boss_code_idx` (`boss_id`),
  KEY `player_id_idx` (`player_id`),
  CONSTRAINT `boss_code` FOREIGN KEY (`boss_id`) REFERENCES `Boss` (`boss_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `member_id` FOREIGN KEY (`member_id`) REFERENCES `Member` (`member_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `player_id` FOREIGN KEY (`player_id`) REFERENCES `Member` (`member_id`) ON DELETE SET NULL ON UPDATE CASCADE
) COMMENT='This table contains every battle record of each clan battle'

CREATE TABLE `Saren_Bot`.`Parameters` (
  `event_id` int(11) NOT NULL AUTO_INCREMENT,
  `event_name` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `status` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `current_boss_id` int(11) DEFAULT NULL,
  `current_cycle` int(11) DEFAULT NULL,
  `current_boss_health` int(11) DEFAULT NULL,
  `max_running_member_allowed` int(11) DEFAULT NULL,
  `first_boss_id` int(11) DEFAULT NULL,
  `last_boss_id` int(11) DEFAULT NULL,
  `event_start_time` datetime NOT NULL,
  `event_end_time` datetime NOT NULL,
  PRIMARY KEY (`event_id`),
  UNIQUE KEY `event_id_UNIQUE` (`event_id`),
  KEY `first_boss_id_idx` (`first_boss_id`),
  KEY `last_boss_id_idx` (`last_boss_id`),
  CONSTRAINT `first_boss_id` FOREIGN KEY (`first_boss_id`) REFERENCES `Boss` (`boss_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `last_boss_id` FOREIGN KEY (`last_boss_id`) REFERENCES `Boss` (`boss_id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) COMMENT='This table contains parameters for the onging clan battle'

CREATE TABLE `Saren_Bot`.`Guides` (
  `guide_id` int(11) NOT NULL AUTO_INCREMENT,
  `event_id` int(11) DEFAULT NULL,
  `boss_id` int(11) DEFAULT NULL,
  `title` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `image_url` varchar(80) DEFAULT NULL,
  `comment` text CHARACTER SET utf8,
  `status` varchar(45) DEFAULT 'Active',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`guide_id`),
  UNIQUE KEY `guide_id_UNIQUE` (`guide_id`),
  KEY `guide_event_id_idx` (`event_id`),
  KEY `guide_boss_id_idx` (`boss_id`),
  CONSTRAINT `guide_boss_id` FOREIGN KEY (`boss_id`) REFERENCES `Boss` (`boss_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `guide_event_id` FOREIGN KEY (`event_id`) REFERENCES `Parameters` (`event_id`) ON DELETE SET NULL ON UPDATE CASCADE
) COMMENT='This table contains information of every uploaded clan battle guide.'

CREATE TABLE `Saren_Bot`.`SL_Record` (
  `record_id` int(11) NOT NULL AUTO_INCREMENT,
  `member_id` bigint(20) DEFAULT NULL,
  `record_time` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`record_id`),
  UNIQUE KEY `record_id_UNIQUE` (`record_id`),
  KEY `member_id_sl_idx` (`member_id`),
  CONSTRAINT `member_id_sl` FOREIGN KEY (`member_id`) REFERENCES `Member` (`member_id`) ON DELETE SET NULL ON UPDATE CASCADE
) COMMENT='This table is used for saving SL records'
