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