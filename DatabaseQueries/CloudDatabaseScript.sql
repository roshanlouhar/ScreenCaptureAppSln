CREATE DATABASE IF NOT EXISTS `cloudtoposcreen` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE cloudtoposcreen;
CREATE TABLE  IF NOT EXISTS `tbltoposcreens` (
  `fldTopoScreenID` int(11) NOT NULL AUTO_INCREMENT,
  `fldDateTime` datetime NOT NULL,
  `fldMacID` varchar(45) NOT NULL,
  `fldScreenshot` varchar(45) NOT NULL,
  `fldScreenShotProcessedYesNo` varchar(45) NOT NULL DEFAULT '0',
  `fldScreenBlob` mediumblob,
  PRIMARY KEY (`fldTopoScreenID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
CREATE TABLE  IF NOT EXISTS `tbltoposcreensservicestatus` (
  `fldTopoScreensServiceStatusID` int(11) NOT NULL AUTO_INCREMENT,
  `fldDateTime` datetime NOT NULL,
  `fldCurrentState` varchar(5) DEFAULT NULL,
  `MachineId` varchar(45) NOT NULL,
  PRIMARY KEY (`fldTopoScreensServiceStatusID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


