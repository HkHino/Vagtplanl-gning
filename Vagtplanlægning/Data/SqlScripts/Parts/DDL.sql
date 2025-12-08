-- create database ------------------------------------------------------------
CREATE DATABASE IF NOT EXISTS cykelBudDB;
USE cykelBudDB;
-- drop tables if exist --------------------------------------------------------
DROP TABLE IF EXISTS AuditLog;
DROP TABLE IF EXISTS WorkHoursInMonths;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS ListOfShift;
DROP TABLE IF EXISTS Bicycles;
DROP TABLE IF EXISTS Substituteds;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Routes;
-- =========================
-- create database ------------------------------------------------------------
CREATE DATABASE IF NOT EXISTS cykelBudDB;
USE cykelBudDB;
-- =========================
-- Create Tabels
-- =========================
-- AuditLog --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AuditLog (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tableName VARCHAR(255) NOT NULL,
    recordId VARCHAR(255) NOT NULL,
    action ENUM('INSERT', 'UPDATE', 'DELETE') NOT NULL,
    changedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    changedBy VARCHAR(255) NULL,
    oldData JSON NULL,
    newData JSON NULL
);

-- bicycles -------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Bicycles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    bicycleNumber INT NOT NULL UNIQUE,
    inOperate BOOLEAN NOT NULL DEFAULT FALSE
);

-- employees ------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Employees (
    id INT AUTO_INCREMENT PRIMARY KEY,
    firstName VARCHAR(255) NOT NULL,
    lastName VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
	phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    experienceLevel INT NOT NULL DEFAULT 1
);
-- substituteds ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Substituteds (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employeeId INT NOT NULL,
    hasSubstituted BOOL NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_sub_employee 
        FOREIGN KEY (employeeId) REFERENCES Employees(id)
        ON DELETE CASCADE ON UPDATE CASCADE
);
-- routes ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Routes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    routeNumber int not null unique 
);

-- list of ListOfShift --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ListOfShift (
    id INT AUTO_INCREMENT PRIMARY KEY,
    dateOfShift date NOT NULL,
    employeeId INT NOT NULL,
    bicycleId INT NOT NULL,
-- Right now it’s NOT NULL, but sometimes a shift may not have a substitute. Consider changing it to NULL by default.
    substitutedId INT NOT NULL,
    routeId INT NOT NULL,
    startTime TIME,
    endTime TIME,
    totalHours DECIMAL(4,2),
	CONSTRAINT fk_shift_employee FOREIGN KEY (employeeId) REFERENCES Employees(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_bicycle FOREIGN KEY (bicycleId) REFERENCES Bicycles(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_substituted FOREIGN KEY (substitutedId) REFERENCES Substituteds(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_route FOREIGN KEY (routeId) REFERENCES Routes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- list of ShiftPlans --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ShiftPlans (
-- i dont know what it for 
    id CHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    startDate DATE NOT NULL,
    endDate DATE NOT NULL,
    shifts JSON NOT NULL
);

-- Users -----------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Users ( 
    id INT AUTO_INCREMENT PRIMARY KEY, 
    username VARCHAR(255) NOT NULL UNIQUE, 
    hash VARCHAR(255) NOT NULL, 
    role VARCHAR(50) NOT NULL,
    employeeId INT UNIQUE NOT NULL, 
    CONSTRAINT fk_user_employee 
        FOREIGN KEY (employeeId) REFERENCES Employees(id) 
            ON DELETE RESTRICT ON UPDATE CASCADE 
);

-- work hours in months -------------------------------------------------------
CREATE TABLE IF NOT EXISTS WorkHoursInMonths (
-- WorkHoursInMonths, Ja, den bryder 3NF, men begrundelse er (performance, rapporter).
    id INT AUTO_INCREMENT PRIMARY KEY,
    employeeId INT NOT NULL,
    payrollYear INT NOT NULL,
    payrollMonth INT NOT NULL,
    periodStart DATE NOT NULL,
    periodEnd DATE NOT NULL,
    totalHours DECIMAL(7,2) NOT NULL DEFAULT 0,
    hasSubstituted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_workhours_employee 
        FOREIGN KEY (employeeId) REFERENCES Employees(id)
        ON DELETE CASCADE
);

USE cykelBudDB;
-- =========================
-- Create user
-- =========================
-- create API user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'api'@'127.0.0.1' IDENTIFIED BY '123456';
-- GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON cykelBudDB.* TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Bicycles TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Employees TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.ListOfShift TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Routes TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.ShiftPlans TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Substituteds TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Users TO 'api'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.WorkHoursInMonths TO 'api'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.AuditLog TO 'api'@'127.0.0.1';
GRANT EXECUTE ON cykelBudDB.* TO 'api'@'127.0.0.1';
-- FLUSH PRIVILEGES;

-- create migration user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'migrator'@'127.0.0.1' IDENTIFIED BY '123456';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Bicycles TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Employees TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.ListOfShift TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Routes TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.ShiftPlans TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Substituteds TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Users TO 'migrator'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.WorkHoursInMonths TO 'migrator'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.AuditLog TO 'migrator'@'127.0.0.1';
-- Så api kan ikke skrive, ændre eller slette audit-logs, 
-- men kan stadig læse dem, hvilket ofte er ønskeligt for rapportering eller overvågning
GRANT EXECUTE ON cykelBudDB.* TO 'migrator'@'127.0.0.1';
-- FLUSH PRIVILEGES;

-- create readOnly user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'readOnly'@'127.0.0.1' IDENTIFIED BY '123456';
GRANT SELECT ON cykelBudDB.Bicycles TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.Employees TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.ListOfShift TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.Routes TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.ShiftPlans TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.Substituteds TO 'readOnly'@'127.0.0.1';
GRANT SELECT ON cykelBudDB.WorkHoursInMonths TO 'readOnly'@'127.0.0.1';
GRANT SELECT, UPDATE, INSERT, DELETE ON cykelBudDB.Users TO 'readOnly'@'127.0.0.1';
-- Ingen adgang til AuditLog
-- REVOKE SELECT,UPDATE,DELETE,INSERT ON cykelBudDB.AuditLog FROM 'readOnly'@'127.0.0.1';
-- FLUSH PRIVILEGES;

-- create onwer user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'jan'@'127.0.0.1' IDENTIFIED BY '123456';
GRANT ALL PRIVILEGES ON cykelBudDB.* TO 'jan'@'127.0.0.1';
-- FLUSH PRIVILEGES;

-- =========================
-- input test data
-- =========================
-- 10 Employees ---------------------------------------------------------------
INSERT INTO Employees (firstName, lastName, address, phone, email, experienceLevel) VALUES
('Alice', 'Andersen', 'Street 1', '11111111', 'alice@mail.com', 1),
('Bob', 'Bendtsen', 'Street 2', '22222222', 'bob@mail.com', 1),
('Charlie', 'Christensen', 'Street 3', '33333333', 'charlie@mail.com', 1),
('Diana', 'Dahl', 'Street 4', '44444444', 'diana@mail.com', 2),
('Erik', 'Eriksen', 'Street 5', '55555555', 'erik@mail.com', 1),
('Freja', 'Friis', 'Street 6', '66666666', 'freja@mail.com', 2),
('Gustav', 'Gul', 'Street 7', '77777777', 'gustav@mail.com', 1),
('Helle', 'Hansen', 'Street 8', '88888888', 'helle@mail.com', 1),
('Ivan', 'Iversen', 'Street 9', '99999999', 'ivan@mail.com', 1),
('Julie', 'Jensen', 'Street 10', '10101010', 'julie@mail.com', 1);
-- 10 Users ---------------------------------------------------------------
INSERT INTO Users (username, hash, role, employeeId) VALUES
-- argon2id hashing from 123456  
('alice', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Admin", 1),
('bob', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 2),
('charlie', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 3),
('diana', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 4),
('erik', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 5),
('freja', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 6),
('gustav', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 7),
('helle', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 8),
('ivan', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 9),
('julie', '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa', "Employee", 10);
-- 15 Bicycles ----------------------------------------------------------------
INSERT INTO Bicycles (bicycleNumber, inOperate) VALUES
(1, TRUE),(2, TRUE),(3, TRUE),(4, TRUE),(5, TRUE),
(6, TRUE),(7, TRUE),(8, TRUE),(9, TRUE),(10, TRUE),
(11, TRUE),(12, TRUE),(13, TRUE),(14, TRUE),(15, TRUE);
-- 13 Routes ------------------------------------------------------------------
INSERT INTO Routes (routeNumber) VALUES
(101),(102),(103),(104),(105),(106),(107),
(108),(109),(110),(111),(112),(113);
-- Substituteds (10 employees, substitute != employeeId) ---------------------
INSERT INTO Substituteds (employeeId, hasSubstituted) VALUES
(1,FALSE),(2,FALSE),(3,FALSE),(4,FALSE),(5,FALSE),
(6,FALSE),(7,FALSE),(8,FALSE),(9,FALSE),(10,FALSE);

-- ListOfShift 30 shifts ------------------------------------------------------
-- For simplicity, we will assign shifts 1-10 to November, 11-20 December, 21-30 January
INSERT INTO ListOfShift (dateOfShift, employeeId, bicycleId, substitutedId, routeId, startTime, endTime)
-- before today (3 with hasSubstituted TRUE)
VALUES
('2025-11-01', 1, 1, 2, 1, '08:00', '12:00'), -- Alice, substituted by Bob
('2025-11-02', 2, 2, 3, 2, '09:00', '13:00'), -- Bob, substituted by Charlie
('2025-11-03', 3, 3, 4, 3, '07:00', '11:00'),-- Charlie, substituted by Diana
('2025-11-04', 4, 4, 5, 4, '08:00', '12:00'),
('2025-11-05', 5, 5, 6, 5, '10:00', '14:00');
-- December shifts (all after today, no startTime/endTime)
INSERT INTO ListOfShift (dateOfShift, employeeId, bicycleId, substitutedId, routeId)
VALUES
('2025-12-01',1,1,1,1),
('2025-12-02',2,2,2,2),
('2025-12-03',3,3,3,3),
('2025-12-04',4,4,4,4),
('2025-12-05',5,5,5,5),
('2025-12-06',6,6,6,6),
('2025-12-07',7,7,7,7),
('2025-12-08',8,8,8,8),
('2025-12-09',9,9,9,9),
('2025-12-10',10,10,10,10);
-- January shifts (all after today, no startTime/endTime)
INSERT INTO ListOfShift (dateOfShift, employeeId, bicycleId, substitutedId, routeId)
VALUES
('2026-01-01',1,1,1,1),
('2026-01-02',2,2,2,2),
('2026-01-03',3,3,3,3),
('2026-01-04',4,4,4,4),
('2026-01-05',5,5,5,5),
('2026-01-06',6,6,6,6),
('2026-01-07',7,7,7,7),
('2026-01-08',8,8,8,8),
('2026-01-09',9,9,9,9),
('2026-01-10',10,10,10,10);