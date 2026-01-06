-- drop database if exists -----------------------------------------------------
DROP DATABASE IF EXISTS cykelBudDB;
-- =========================

-- drop tables if exist --------------------------------------------------------
DROP TABLE IF EXISTS AuditLog;
DROP TABLE IF EXISTS WorkHoursInMonths;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS ListOfShift;
DROP TABLE IF EXISTS Bicycles;
DROP TABLE IF EXISTS Substituteds;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Routes;
DROP TABLE IF EXISTS ShiftPlans;
DROP TABLE IF EXISTS OutboxEvents;
-- =========================
-- create database ------------------------------------------------------------
CREATE DATABASE IF NOT EXISTS cykelBudDB;
USE cykelBudDB;
-- Create Tabels
-- =========================
-- AuditLog --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AuditLog (
    auditId INT AUTO_INCREMENT PRIMARY KEY,
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
                                         employeeId INT AUTO_INCREMENT PRIMARY KEY,
                                         firstName VARCHAR(255) NOT NULL,
    lastName VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    experienceLevel INT NOT NULL DEFAULT 1
    );
-- substituteds ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Substituteds (
                                            substitutedId INT AUTO_INCREMENT PRIMARY KEY,
                                            employeeId INT NOT NULL,
                                            hasSubstituted BOOL NOT NULL DEFAULT FALSE,
                                            CONSTRAINT fk_sub_employee FOREIGN KEY (employeeId) REFERENCES Employees(employeeId) ON DELETE CASCADE ON UPDATE CASCADE
    );
-- routes ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Routes (
                                      id INT PRIMARY KEY AUTO_INCREMENT,
                                      routeNumber int not null unique
);
-- list of ListOfShift --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ListOfShift (
                                           shiftId INT AUTO_INCREMENT PRIMARY KEY,
                                           dateOfShift date NOT NULL,
                                           employeeId INT NOT NULL,
                                           bicycleId INT NOT NULL,
    -- Right now it’s NOT NULL, but sometimes a shift may not have a substitute. Consider changing it to NULL by default.
                                           substitutedId INT NOT NULL,
                                           routeId INT NOT NULL,
                                           startTime TIME,
                                           endTime TIME,
                                           totalHours DECIMAL(4, 2),
    CONSTRAINT fk_shift_employee FOREIGN KEY (employeeId) REFERENCES Employees(employeeId) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_bicycle FOREIGN KEY (bicycleId) REFERENCES Bicycles(id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_substituted FOREIGN KEY (substitutedId) REFERENCES Substituteds(substitutedId) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_shift_route FOREIGN KEY (routeId) REFERENCES Routes(id) ON DELETE CASCADE ON UPDATE CASCADE
    );
-- list of ShiftPlans --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ShiftPlans (
    -- i dont know what it for 
                                          shiftPlanId CHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    startDate DATE NOT NULL,
    endDate DATE NOT NULL,
    shifts JSON NOT NULL
    );
-- Users -----------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Users (
                                     userId INT AUTO_INCREMENT PRIMARY KEY,
                                     username VARCHAR(255) NOT NULL UNIQUE,
    hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    employeeId INT UNIQUE NULL,
    CONSTRAINT fk_user_employee FOREIGN KEY (employeeId) REFERENCES Employees(employeeId) ON DELETE CASCADE ON UPDATE CASCADE
    );
-- work hours in months -------------------------------------------------------
CREATE TABLE IF NOT EXISTS WorkHoursInMonths (
    -- WorkHoursInMonths, Ja, den bryder 3NF, men begrundelse er (performance, rapporter).
                                                 workHoursInMonthId INT AUTO_INCREMENT PRIMARY KEY,
                                                 employeeId INT NOT NULL,
                                                 payrollYear INT NOT NULL,
                                                 payrollMonth INT NOT NULL,
                                                 periodStart DATE NOT NULL,
                                                 periodEnd DATE NOT NULL,
                                                 totalHours DECIMAL(7, 2) NOT NULL DEFAULT 0,
    hasSubstituted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_workhours_employee FOREIGN KEY (employeeId) REFERENCES Employees(employeeId) ON DELETE CASCADE
    );
-- =========================
-- Create indexes --------------------------------------------------------------
-- =========================
CREATE INDEX idx_shift_date_employee -- (run once on fresh DB; remove if you re-run script many times)
    ON ListOfShift (dateOfShift, employeeId);
CREATE INDEX idx_shift_date_substituted -- (run once on fresh DB; remove if you re-run script many times)
    ON ListOfShift (dateOfShift, substitutedId);
-- Ensure only one row per employee per payroll period
CREATE UNIQUE INDEX idx_payroll_employee_period ON WorkHoursInMonths (employeeId, periodStart, periodEnd);
-- =========================
-- Views --------------------------------------------------------------------------------------------
-- =========================
-- view: v_employee_shifts ----------------------------------------------------
DELIMITER $$ 
CREATE OR REPLACE VIEW v_employee_shifts AS
SELECT s.shiftId,
       s.dateOfShift AS shiftDate,
       e.employeeId,
       CONCAT(e.firstName, ' ', e.lastName) AS employeeName,
       r.routeNumber AS routeNumber,
       s.startTime,
       s.endTime,
       s.totalHours,
       b.bicycleNumber AS bicycleNumber,
       b.inOperate
FROM ListOfShift s
         JOIN Employees e ON s.employeeId = e.employeeId
         JOIN Routes r ON s.routeId = r.id
         JOIN Bicycles b ON s.bicycleId = b.id;
$$
DELIMITER ;

DELIMITER $$ 
CREATE OR REPLACE VIEW v_employee_shifts_WithSubs AS
SELECT s.shiftId AS shiftId,
       s.dateOfShift AS shiftDate,
       e.employeeId AS employeeId,
       CONCAT(e.firstName, ' ', e.lastName) AS employeeName,
       r.routeNumber AS routeNumber,
       s.startTime AS startTime,
       s.endTime AS endTime,
       s.totalHours AS totalHours,
       b.bicycleNumber AS bicycleNumber,
       b.inOperate AS inOperate,
       -- Hvis der ikke findes en employee på substituten, vis 'Ingen employee'
       COALESCE(
               CONCAT(su_e.firstName, ' ', su_e.lastName),
               'Ingen employee'
       ) AS Substituted,
       su.hasSubstituted
FROM ListOfShift s
         JOIN Employees e ON s.employeeId = e.employeeId
         JOIN Routes r ON s.routeId = r.id
         JOIN Bicycles b ON s.bicycleId = b.id
         LEFT JOIN Substituteds su ON s.substitutedId = su.substitutedId
         LEFT JOIN Employees su_e ON su.employeeId = su_e.employeeId;
/* prettier-ignore */
$$
DELIMITER ;

-- =========================
-- stored procedures --------------------------------------------------------------------------------------------
-- =========================
-- stored procedure: GetMonthlyHours-----------------------------
DELIMITER $$
CREATE PROCEDURE GetMonthlyHours (
    IN pEmployeeId INT,
    IN pYear INT,
    IN pMonth INT
) BEGIN
DECLARE vPeriodStart DATE;
DECLARE vPeriodEnd DATE;
IF pYear IS NOT NULL
AND pMonth IS NOT NULL THEN
SET vPeriodEnd = DATE(CONCAT(pYear, '-', LPAD(pMonth, 2, '0'), '-25'));
SET vPeriodStart = DATE_SUB(vPeriodEnd, INTERVAL 1 MONTH) + INTERVAL 1 DAY;
ELSE
SET vPeriodStart = NULL;
SET vPeriodEnd = NULL;
END IF;
SELECT e.employeeId,
       e.firstName,
       e.lastName,
       pYear AS year,
    pMonth AS month,
    COALESCE(
        SUM(
            CASE
                WHEN s.substitutedId = e.employeeId
                OR s.employeeId = e.employeeId THEN s.totalHours
                ELSE 0
            END
        ),
        0
    ) AS totalMonthlyHours,
    CASE
        WHEN SUM(
            CASE
                WHEN s.substitutedId = e.employeeId THEN 1
                ELSE 0
            END
        ) > 0 THEN TRUE
        ELSE FALSE
END AS hasSubstituted
FROM Employees e
    LEFT JOIN ListOfShift s ON (
        s.employeeId = e.employeeId
        OR s.substitutedId = e.employeeId
    )
    AND (
        vPeriodStart IS NULL
        OR s.dateOfShift BETWEEN vPeriodStart AND vPeriodEnd
    )
WHERE pEmployeeId IS NULL
    OR e.employeeId = pEmployeeId
GROUP BY e.employeeId,
    e.firstName,
    e.lastName;
END $ $
DELIMITER ;

-- stored procedure: GetRoutes -----------------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE GetRoutes (IN pRouteId INT) BEGIN
SELECT id AS routeId,
       routeNumber
FROM Routes
WHERE pRouteId IS NULL
   OR id = pRouteId
ORDER BY routeNumber;
END $$
DELIMITER ;

-- stored procedure: GetBicycles ----------------------------------------------
DELIMITER $$
CREATE PROCEDURE GetBicycles (IN pBicycleId INT) BEGIN
SELECT id AS bicycleId,
       bicycleNumber,
       inOperate
FROM Bicycles
WHERE pBicycleId IS NULL
   OR id = pBicycleId
ORDER BY bicycleNumber;
END $$
DELIMITER ;

-- stored procedure: GetEmployees ---------------------------------------------
DELIMITER $$
CREATE PROCEDURE GetEmployees (IN pEmployeeId INT) BEGIN
SELECT employeeId,
       firstName,
       lastName,
       address,
       phone,
       email,
       experienceLevel
FROM Employees
WHERE pEmployeeId IS NULL
   OR employeeId = pEmployeeId
ORDER BY lastName,
         firstName;
END $$
DELIMITER ;

-- stored procedure: AddEmployee ----------------------------------------------
DELIMITER $$
CREATE PROCEDURE AddEmployee (
    IN pFirstName VARCHAR(255),
    IN pLastName VARCHAR(255),
    IN pAddress VARCHAR(255),
    IN pPhone VARCHAR(20),
    IN pEmail VARCHAR(255),
    IN pExperienceLevel TINYINT
) BEGIN
INSERT INTO Employees (
        firstName,
        lastName,
        address,
        phone,
        email,
        experienceLevel
    )
VALUES (
        pFirstName,
        pLastName,
        pAddress,
        pPhone,
        pEmail,
        COALESCE(pExperienceLevel, 1)
    );
END $$
DELIMITER ;

-- stored procedure: AddBicycle -----------------------------------------------
DELIMITER $$
CREATE PROCEDURE AddBicycle () BEGIN
INSERT INTO Bicycles (bicycleNumber, inOperate)
VALUES (
        (
            SELECT IFNULL(MAX(bicycleNumber), 0) + 1
            FROM Bicycles
        ),
        DEFAULT
    );
END $$
DELIMITER ;

-- stored procedure: UpdateBicycleStatus --------------------------------------
DELIMITER $$
CREATE PROCEDURE UpdateBicycleStatus (
    IN pBicycleId INT,
    IN pInOperate BOOLEAN
) BEGIN
UPDATE Bicycles
SET inOperate = pInOperate
WHERE id = pBicycleId;
END $$
DELIMITER ;

-- stored procedure: AddRoute -------------------------------------------------
DELIMITER $$
CREATE PROCEDURE AddRoute () BEGIN
INSERT INTO Route (routeNumber)
VALUES (
        (
            SELECT IFNULL(MAX(routeNumber), 0) + 1
            FROM Route
        )
    );
END $$
DELIMITER ;

-- stored procedure: AddShift -------------------------------------------------
DELIMITER $$
CREATE PROCEDURE AddShift (
    IN pDateOfShift DATE,
    IN pEmployeeId INT,
    IN pBicycleId INT,
    IN pRouteId INT,
    IN pStartTime TIME,
    IN pSubstitutedId INT
) BEGIN
INSERT INTO ListOfShift (
        dateOfShift,
        employeeId,
        bicycleId,
        routeId,
        startTime,
        substitutedId
    )
VALUES (
        pDateOfShift,
        pEmployeeId,
        pBicycleId,
        pRouteId,
        pStartTime,
        pSubstitutedId
    );
END $$
DELIMITER ;

-- stored procedure: UpdateShiftSubstituted -------------------------------------
DELIMITER $$
CREATE PROCEDURE UpdateShiftSubstituted (
    IN pShiftId INT,
    IN pHasSubstituted BOOL
) BEGIN
DECLARE vSubstitutedId INT;
SELECT substitutedId INTO vSubstitutedId
FROM ListOfShift
WHERE shiftId = pShiftId;
IF vSubstitutedId IS NOT NULL THEN
UPDATE Substituteds
SET hasSubstituted = pHasSubstituted
WHERE substitutedId = vSubstitutedId;
END IF;
END $$
DELIMITER ;

-- stored procedure: UpdateStartTime ---------------------------------------------
DELIMITER $$
CREATE PROCEDURE UpdateStartTime (IN pShiftId INT, IN pStartTime TIME) BEGIN
UPDATE ListOfShift
SET startTime = pStartTime
WHERE shiftId = pShiftId;
END $$
DELIMITER ;

-- stored procedure: UpdateEndTime -----------------------------------------------
DELIMITER $$
CREATE PROCEDURE UpdateEndTime (IN pShiftId INT, IN pEndTime TIME) BEGIN
UPDATE ListOfShift
SET endTime = pEndTime
WHERE shiftId = pShiftId;
END $$
DELIMITER ;

-- =========================
-- Create event --------------------------------------------------------------------------------------------
-- =========================
DELIMITER $$
CREATE EVENT ev_generateWorkHoursInMonths -- event: monthly rollup ------------------------------------------------------
-- if your MySQL version complains about CREATE EVENT (or scheduler disabled),
-- you can enable it with: SET GLOBAL event_scheduler = ON;
ON SCHEDULE EVERY 1 MONTH STARTS '2025-09-25 23:50:00' DO BEGIN
INSERT INTO WorkHoursInMonths (
        employeeId,
        payrollYear,
        payrollMonth,
        periodStart,
        periodEnd,
        totalHours,
        hasSubstituted
    )
SELECT e.employeeId,
    YEAR(CURDATE()) AS payrollYear,
    MONTH(CURDATE()) AS payrollMonth,
       -- Periode: 26. sidste måned -> 25. denne måned
    DATE_SUB(
    DATE_FORMAT(CURDATE(), '%Y-%m-26'),
    INTERVAL 1 MONTH
    ) AS periodStart,
    DATE_FORMAT(CURDATE(), '%Y-%m-25') AS periodEnd,
       -- Total timer som hovedarbejder eller som substitut
    COALESCE(
    SUM(
    CASE
    WHEN s.startTime IS NOT NULL
    AND s.endTime IS NOT NULL THEN TIME_TO_SEC(TIMEDIFF(s.endTime, s.startTime)) / 3600
    ELSE 0
    END
    ),
    0
    ) AS totalHours,
       -- Har employee substitueret andre?
    CASE
    WHEN COUNT(sub.substitutedId) > 0 THEN TRUE
    ELSE FALSE
END AS hasSubstituted
FROM Employees e
    LEFT JOIN ListOfShift s ON (
        e.employeeId = s.employeeId
        OR e.employeeId = s.substitutedId
    )
    AND s.dateOfShift BETWEEN DATE_SUB(
        DATE_FORMAT(CURDATE(), '%Y-%m-26'),
        INTERVAL 1 MONTH
    )
    AND DATE_FORMAT(CURDATE(), '%Y-%m-25')
    LEFT JOIN Substituteds sub ON s.substitutedId = sub.substitutedId
    AND sub.employeeId = e.employeeId
GROUP BY e.employeeId;
END $$
DELIMITER ;

-- =========================
-- Create triggers
-- =========================
-- trigger to recalc total hours ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_update_totalHours
    BEFORE UPDATE ON ListOfShift
    FOR EACH ROW
BEGIN
    IF NEW.startTime IS NOT NULL AND NEW.endTime IS NOT NULL THEN
        SET NEW.totalHours = TIME_TO_SEC(TIMEDIFF(NEW.endTime, NEW.startTime)) / 3600;
END IF;
END$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER trg_insert_totalHours
    BEFORE INSERT ON ListOfShift
    FOR EACH ROW
BEGIN
    IF NEW.startTime IS NOT NULL AND NEW.endTime IS NOT NULL THEN
        SET NEW.totalHours = TIME_TO_SEC(TIMEDIFF(NEW.endTime, NEW.startTime)) / 3600;
END IF;
END$$
DELIMITER ;
-- TRIGGER to  AuditLog ----------------------------------------------
-- TRIGGER for AuditLog of Bicyles ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_Bicycles_insert
    AFTER INSERT ON Bicycles
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'Bicycles',
               NEW.id,
               'INSERT',
               JSON_OBJECT(
                       'id', NEW.id,
                       'bicycleNumber', NEW.bicycleNumber,
                       'inOperate', NEW.inOperate
               ), USER()
           );
END $$

CREATE TRIGGER trg_Bicycles_update
    AFTER UPDATE ON Bicycles
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'Bicycles',
               NEW.id,
               'UPDATE',
               JSON_OBJECT(
                       'id', OLD.id,
                       'bicycleNumber', OLD.bicycleNumber,
                       'inOperate', OLD.inOperate
               ),
               JSON_OBJECT(
                       'id', NEW.id,
                       'bicycleNumber', NEW.bicycleNumber,
                       'inOperate', NEW.inOperate
               ), USER()
           );
END $$

CREATE TRIGGER trg_Bicycles_delete
    AFTER DELETE ON Bicycles
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'Bicycles',
               OLD.id,
               'DELETE',
               JSON_OBJECT(
                       'id', OLD.id,
                       'bicycleNumber', OLD.bicycleNumber,
                       'inOperate', OLD.inOperate
               ), USER()
           );
END $$
DELIMITER ;

-- TRIGGER for AuditLog of Employees ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_Employees_insert
    AFTER INSERT ON Employees
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'Employees',
               NEW.employeeId,
               'INSERT',
               JSON_OBJECT(
                       'employeeId', NEW.employeeId,
                       'firstName', NEW.firstName,
                       'lastName', NEW.lastName,
                       'address', NEW.address,
                       'phone', NEW.phone,
                       'email', NEW.email,
                       'experienceLevel', NEW.experienceLevel
               ), USER()
           );
END $$

CREATE TRIGGER trg_Employees_update
    AFTER UPDATE ON Employees
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'Employees',
               NEW.employeeId,
               'UPDATE',
               JSON_OBJECT(
                       'employeeId', OLD.employeeId,
                       'firstName', OLD.firstName,
                       'lastName', OLD.lastName,
                       'address', OLD.address,
                       'phone', OLD.phone,
                       'email', OLD.email,
                       'experienceLevel', OLD.experienceLevel
               ),
               JSON_OBJECT(
                       'employeeId', NEW.employeeId,
                       'firstName', NEW.firstName,
                       'lastName', NEW.lastName,
                       'address', NEW.address,
                       'phone', NEW.phone,
                       'email', NEW.email,
                       'experienceLevel', NEW.experienceLevel
               ), USER()
           );
END $$

CREATE TRIGGER trg_Employees_delete
    AFTER DELETE ON Employees
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'Employees',
               OLD.employeeId,
               'DELETE',
               JSON_OBJECT(
                       'employeeId', OLD.employeeId,
                       'firstName', OLD.firstName,
                       'lastName', OLD.lastName,
                       'address', OLD.address,
                       'phone', OLD.phone,
                       'email', OLD.email,
                       'experienceLevel', OLD.experienceLevel
               ), USER()
           );
END $$
DELIMITER ;
-- TRIGGER for AuditLog of ListOfShift ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_ListOfShift_insert
    AFTER INSERT ON ListOfShift
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'ListOfShift',
               NEW.shiftId,
               'INSERT',
               JSON_OBJECT(
                       'shiftId', NEW.shiftId,
                       'dateOfShift', NEW.dateOfShift,
                       'employeeId', NEW.employeeId,
                       'bicycleId', NEW.bicycleId,
                       'substitutedId', NEW.substitutedId,
                       'routeId', NEW.routeId,
                       'startTime', NEW.startTime,
                       'endTime', NEW.endTime,
                       'totalHours', NEW.totalHours
               ), USER()
           );
END $$

CREATE TRIGGER trg_ListOfShift_update
    AFTER UPDATE ON ListOfShift
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'ListOfShift',
               NEW.shiftId,
               'UPDATE',
               JSON_OBJECT(
                       'shiftId', OLD.shiftId,
                       'dateOfShift', OLD.dateOfShift,
                       'employeeId', OLD.employeeId,
                       'bicycleId', OLD.bicycleId,
                       'substitutedId', OLD.substitutedId,
                       'routeId', OLD.routeId,
                       'startTime', OLD.startTime,
                       'endTime', OLD.endTime,
                       'totalHours', OLD.totalHours
               ),
               JSON_OBJECT(
                       'shiftId', NEW.shiftId,
                       'dateOfShift', NEW.dateOfShift,
                       'employeeId', NEW.employeeId,
                       'bicycleId', NEW.bicycleId,
                       'substitutedId', NEW.substitutedId,
                       'routeId', NEW.routeId,
                       'startTime', NEW.startTime,
                       'endTime', NEW.endTime,
                       'totalHours', NEW.totalHours
               ), USER()
           );
END $$

CREATE TRIGGER trg_ListOfShift_delete
    AFTER DELETE ON ListOfShift
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'ListOfShift',
               OLD.shiftId,
               'DELETE',
               JSON_OBJECT(
                       'shiftId', OLD.shiftId,
                       'dateOfShift', OLD.dateOfShift,
                       'employeeId', OLD.employeeId,
                       'bicycleId', OLD.bicycleId,
                       'substitutedId', OLD.substitutedId,
                       'routeId', OLD.routeId,
                       'startTime', OLD.startTime,
                       'endTime', OLD.endTime,
                       'totalHours', OLD.totalHours
               ), USER()
           );
END $$
DELIMITER ;
-- TRIGGER for AuditLog of Route ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_Route_insert
    AFTER INSERT ON Routes
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'Route',
               NEW.id,
               'INSERT',
               JSON_OBJECT(
                       'id', NEW.id,
                       'routeNumber', NEW.routeNumber
               ), USER()
           );
END $$

CREATE TRIGGER trg_Route_update
    AFTER UPDATE ON Routes
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'Route',
               NEW.id,
               'UPDATE',
               JSON_OBJECT(
                       'id', OLD.id,
                       'routeNumber', OLD.routeNumber
               ),
               JSON_OBJECT(
                       'id', NEW.id,
                       'routeNumber', NEW.routeNumber
               ), USER()
           );
END $$

CREATE TRIGGER trg_Route_delete
    AFTER DELETE ON Routes
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'Route',
               OLD.id,
               'DELETE',
               JSON_OBJECT(
                       'id', OLD.id,
                       'routeNumber', OLD.routeNumber
               ), USER()
           );
END $$
DELIMITER ;
-- TRIGGER for AuditLog of ShiftPlans ----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_ShiftPlans_insert
    AFTER INSERT ON ShiftPlans
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'ShiftPlans',
               NEW.shiftPlanId,
               'INSERT',
               JSON_OBJECT(
                       'shiftPlanId', NEW.shiftPlanId,
                       'name', NEW.name,
                       'startDate', NEW.startDate,
                       'endDate', NEW.endDate,
                       'shifts', NEW.shifts
               ), USER()
           );
END $$

CREATE TRIGGER trg_ShiftPlans_update
    AFTER UPDATE ON ShiftPlans
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'ShiftPlans',
               NEW.shiftPlanId,
               'UPDATE',
               JSON_OBJECT(
                       'shiftPlanId', OLD.shiftPlanId,
                       'name', OLD.name,
                       'startDate', OLD.startDate,
                       'endDate', OLD.endDate,
                       'shifts', OLD.shifts
               ),
               JSON_OBJECT(
                       'shiftPlanId', NEW.shiftPlanId,
                       'name', NEW.name,
                       'startDate', NEW.startDate,
                       'endDate', NEW.endDate,
                       'shifts', NEW.shifts
               ), USER()
           );
END $$

CREATE TRIGGER trg_ShiftPlans_delete
    AFTER DELETE ON ShiftPlans
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'ShiftPlans',
               OLD.shiftPlanId,
               'DELETE',
               JSON_OBJECT(
                       'shiftPlanId', OLD.shiftPlanId,
                       'name', OLD.name,
                       'startDate', OLD.startDate,
                       'endDate', OLD.endDate,
                       'shifts', OLD.shifts
               ), USER()
           );
END $$
DELIMITER ;
-- TRIGGER for AuditLog of Substituteds ----------------------------------------------
DELIMITER $$
-- INSERT trigger
CREATE TRIGGER trg_Substituteds_insert
    AFTER INSERT ON Substituteds
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
    VALUES (
               'Substituteds',
               NEW.substitutedId,
               'INSERT',
               JSON_OBJECT(
                       'substitutedId', NEW.substitutedId,
                       'employeeId', NEW.employeeId,
                       'hasSubstituted', NEW.hasSubstituted
               ), USER()
           );
END $$

-- UPDATE trigger
CREATE TRIGGER trg_Substituteds_update
    AFTER UPDATE ON Substituteds
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
    VALUES (
               'Substituteds',
               NEW.substitutedId,
               'UPDATE',
               JSON_OBJECT(
                       'substitutedId', OLD.substitutedId,
                       'employeeId', OLD.employeeId,
                       'hasSubstituted', OLD.hasSubstituted
               ),
               JSON_OBJECT(
                       'substitutedId', NEW.substitutedId,
                       'employeeId', NEW.employeeId,
                       'hasSubstituted', NEW.hasSubstituted
               ), USER()
           );
END $$

-- DELETE trigger
CREATE TRIGGER trg_Substituteds_delete
    AFTER DELETE ON Substituteds
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
    VALUES (
               'Substituteds',
               OLD.substitutedId,
               'DELETE',
               JSON_OBJECT(
                       'substitutedId', OLD.substitutedId,
                       'employeeId', OLD.employeeId,
                       'hasSubstituted', OLD.hasSubstituted
               ), USER()
           );
END $$
DELIMITER ;
-- TRIGGER for AuditLog of users -----------------------------------------------
DELIMITER $$
CREATE TRIGGER trg_users_insert
    AFTER INSERT ON Users
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLog(tableName, recordId, action, changedBy, newData)
    VALUES (
               'Users',
               NEW.userId,
               'INSERT',
               USER(),
               JSON_OBJECT(
                       'userId', NEW.userId,
                       'userName', NEW.userName,
                       'employeeId', NEW.employeeId,
                       'role' , NEW.role
               )
           );
    END$$

    CREATE TRIGGER trg_users_update
        AFTER UPDATE ON Users
        FOR EACH ROW
    BEGIN
        INSERT INTO AuditLog(tableName, recordId, action, changedBy, oldData, newData)
        VALUES (
                   'Users',
                   NEW.userId,
                   'UPDATE',
                   USER(),
                   JSON_OBJECT(
                           'userId', OLD.userId,
                           'userName', OLD.userName,
                           'employeeId', OLD.employeeId,
                           'role' , NEW.role
                   ),
                   JSON_OBJECT(
                           'userId', NEW.userId,
                           'userName', NEW.userName,
                           'employeeId', NEW.employeeId,
                           'role' , NEW.role
                   )
               );
        END$$

        CREATE TRIGGER trg_users_delete
            AFTER DELETE ON Users
            FOR EACH ROW
        BEGIN
            INSERT INTO AuditLog(tableName, recordId, action, changedBy, oldData)
            VALUES (
                       'Users',
                       OLD.userId,
                       'DELETE',
                       USER(),
                       JSON_OBJECT(
                               'userId', OLD.userId,
                               'userName', OLD.userName,
                               'employeeId', OLD.employeeId,
                               'role' , NEW.role
                       )
                   );
            END$$
            DELIMITER ;
-- TRIGGER for AuditLog of WorkHoursInMonths ----------------------------------------------
DELIMITER $$
            -- INSERT trigger
            CREATE TRIGGER trg_WorkHoursInMonths_insert
                AFTER INSERT ON WorkHoursInMonths
                FOR EACH ROW
            BEGIN
                INSERT INTO AuditLog (tableName, recordId, action, newData, changedBy)
                VALUES (
                           'WorkHoursInMonths',
                           NEW.workHoursInMonthId,
                           'INSERT',
                           JSON_OBJECT(
                                   'workHoursInMonthId', NEW.workHoursInMonthId,
                                   'employeeId', NEW.employeeId,
                                   'payrollYear', NEW.payrollYear,
                                   'payrollMonth', NEW.payrollMonth,
                                   'periodStart', NEW.periodStart,
                                   'periodEnd', NEW.periodEnd,
                                   'totalHours', NEW.totalHours,
                                   'hasSubstituted', NEW.hasSubstituted
                           ),
                           USER()
                       );
            END $$

            -- UPDATE trigger
            CREATE TRIGGER trg_WorkHoursInMonths_update
                AFTER UPDATE ON WorkHoursInMonths
                FOR EACH ROW
            BEGIN
                INSERT INTO AuditLog (tableName, recordId, action, oldData, newData, changedBy)
                VALUES (
                           'WorkHoursInMonths',
                           NEW.workHoursInMonthId,
                           'UPDATE',
                           JSON_OBJECT(
                                   'workHoursInMonthId', OLD.workHoursInMonthId,
                                   'employeeId', OLD.employeeId,
                                   'payrollYear', OLD.payrollYear,
                                   'payrollMonth', OLD.payrollMonth,
                                   'periodStart', OLD.periodStart,
                                   'periodEnd', OLD.periodEnd,
                                   'totalHours', OLD.totalHours,
                                   'hasSubstituted', OLD.hasSubstituted
                           ),
                           JSON_OBJECT(
                                   'workHoursInMonthId', NEW.workHoursInMonthId,
                                   'employeeId', NEW.employeeId,
                                   'payrollYear', NEW.payrollYear,
                                   'payrollMonth', NEW.payrollMonth,
                                   'periodStart', NEW.periodStart,
                                   'periodEnd', NEW.periodEnd,
                                   'totalHours', NEW.totalHours,
                                   'hasSubstituted', NEW.hasSubstituted
                           ),
                           USER()
                       );
            END $$

            -- DELETE trigger
            CREATE TRIGGER trg_WorkHoursInMonths_delete
                AFTER DELETE ON WorkHoursInMonths
                FOR EACH ROW
            BEGIN
                INSERT INTO AuditLog (tableName, recordId, action, oldData, changedBy)
                VALUES (
                           'WorkHoursInMonths',
                           OLD.workHoursInMonthId,
                           'DELETE',
                           JSON_OBJECT(
                                   'workHoursInMonthId', OLD.workHoursInMonthId,
                                   'employeeId', OLD.employeeId,
                                   'payrollYear', OLD.payrollYear,
                                   'payrollMonth', OLD.payrollMonth,
                                   'periodStart', OLD.periodStart,
                                   'periodEnd', OLD.periodEnd,
                                   'totalHours', OLD.totalHours,
                                   'hasSubstituted', OLD.hasSubstituted
                           ),
                           USER()
                       );
            END $$
DELIMITER ;
            -- =========================
-- =========================
-- Create user
-- =========================
-- create API user ------------------------------------------------------------------
            CREATE USER IF NOT EXISTS 'api' @'127.0.0.1' IDENTIFIED BY '123456';
-- GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON cykelBudDB.* TO 'api'@'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Bicycles TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Employees TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.ListOfShift TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Routes TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.ShiftPlans TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Substituteds TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Users TO 'api' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.WorkHoursInMonths TO 'api' @'127.0.0.1';
GRANT SELECT ON cykelBudDB.AuditLog TO 'api' @'127.0.0.1';
GRANT SELECT, INSERT, DELETE ON cykelBudDB.OutboxEvents TO 'api' @'127.0.0.1';
            GRANT EXECUTE ON cykelBudDB.* TO 'api' @'127.0.0.1';
-- FLUSH PRIVILEGES;
-- create migration user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'migrator' @'127.0.0.1' IDENTIFIED BY '123456';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Bicycles TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Employees TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.ListOfShift TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Routes TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.ShiftPlans TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Substituteds TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Users TO 'migrator' @'127.0.0.1';
GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.WorkHoursInMonths TO 'migrator' @'127.0.0.1';
GRANT SELECT ON cykelBudDB.AuditLog TO 'migrator' @'127.0.0.1';
            -- Så api kan ikke skrive, ændre eller slette audit-logs, 
-- men kan stadig læse dem, hvilket ofte er ønskeligt for rapportering eller overvågning
            GRANT EXECUTE ON cykelBudDB.* TO 'migrator' @'127.0.0.1';
-- FLUSH PRIVILEGES;
-- create readOnly user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'readOnly' @'127.0.0.1' IDENTIFIED BY '123456';
GRANT SELECT ON cykelBudDB.Bicycles TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.Employees TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.ListOfShift TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.Routes TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.ShiftPlans TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.Substituteds TO 'readOnly' @'127.0.0.1';
            GRANT SELECT ON cykelBudDB.WorkHoursInMonths TO 'readOnly' @'127.0.0.1';            
            GRANT SELECT,
            UPDATE,
            INSERT,
            DELETE ON cykelBudDB.Users TO 'readOnly' @'127.0.0.1';
-- Ingen adgang til AuditLog
-- REVOKE SELECT,UPDATE,DELETE,INSERT ON cykelBudDB.AuditLog FROM 'readOnly'@'127.0.0.1';
-- FLUSH PRIVILEGES;
-- create onwer user ------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'jan' @'127.0.0.1' IDENTIFIED BY '123456';
GRANT ALL PRIVILEGES ON cykelBudDB.* TO 'jan' @'127.0.0.1';
-- FLUSH PRIVILEGES;


-- =========================
-- input test data
-- =========================
-- 10 Employees ---------------------------------------------------------------
            INSERT INTO Employees (
                firstName,
                lastName,
                address,
                phone,
                email,
                experienceLevel
            )
            VALUES (
                       'Alice',
                       'Andersen',
                       'Street 1',
                       '11111111',
                       'alice@mail.com',
                       1
                   ),
                   (
                       'Bob',
                       'Bendtsen',
                       'Street 2',
                       '22222222',
                       'bob@mail.com',
                       1
                   ),
                   (
                       'Charlie',
                       'Christensen',
                       'Street 3',
                       '33333333',
                       'charlie@mail.com',
                       1
                   ),
                   (
                       'Diana',
                       'Dahl',
                       'Street 4',
                       '44444444',
                       'diana@mail.com',
                       2
                   ),
                   (
                       'Erik',
                       'Eriksen',
                       'Street 5',
                       '55555555',
                       'erik@mail.com',
                       1
                   ),
                   (
                       'Freja',
                       'Friis',
                       'Street 6',
                       '66666666',
                       'freja@mail.com',
                       2
                   ),
                   (
                       'Gustav',
                       'Gul',
                       'Street 7',
                       '77777777',
                       'gustav@mail.com',
                       1
                   ),
                   (
                       'Helle',
                       'Hansen',
                       'Street 8',
                       '88888888',
                       'helle@mail.com',
                       1
                   ),
                   (
                       'Ivan',
                       'Iversen',
                       'Street 9',
                       '99999999',
                       'ivan@mail.com',
                       1
                   ),
                   (
                       'Julie',
                       'Jensen',
                       'Street 10',
                       '10101010',
                       'julie@mail.com',
                       1
                   );
-- 10 Users ---------------------------------------------------------------
            INSERT INTO Users (username, hash, role, employeeId)
            VALUES -- argon2id hashing from 123456  
                   (
                       'alice',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Admin",
                       1
                   ),
                   (
                       'bob',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       2
                   ),
                   (
                       'charlie',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       3
                   ),
                   (
                       'diana',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       4
                   ),
                   (
                       'erik',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       5
                   ),
                   (
                       'freja',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       6
                   ),
                   (
                       'gustav',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       7
                   ),
                   (
                       'helle',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       8
                   ),
                   (
                       'ivan',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       9
                   ),
                   (
                       'julie',
                       '$2a$11$Z9mzo.Y4NhRm05VfBAyd8.E5Ezvaav1y6299buDF6aSL7yiU8xOFa',
                       "Employee",
                       10
                   );
-- 15 Bicycles ----------------------------------------------------------------
            INSERT INTO Bicycles (bicycleNumber, inOperate)
            VALUES (1, TRUE),
                   (2, TRUE),
                   (3, TRUE),
                   (4, TRUE),
                   (5, TRUE),
                   (6, TRUE),
                   (7, TRUE),
                   (8, TRUE),
                   (9, TRUE),
                   (10, TRUE),
                   (11, TRUE),
                   (12, TRUE),
                   (13, TRUE),
                   (14, TRUE),
                   (15, TRUE);
-- 13 Routes ------------------------------------------------------------------
            INSERT INTO Routes (routeNumber)
            VALUES (101),
                   (102),
                   (103),
                   (104),
                   (105),
                   (106),
                   (107),
                   (108),
                   (109),
                   (110),
                   (111),
                   (112),
                   (113);
-- Substituteds (10 employees, substitute != employeeId) ---------------------
            INSERT INTO Substituteds (employeeId, hasSubstituted)
            VALUES (1, FALSE),
                   (2, FALSE),
                   (3, FALSE),
                   (4, FALSE),
                   (5, FALSE),
                   (6, FALSE),
                   (7, FALSE),
                   (8, FALSE),
                   (9, FALSE),
                   (10, FALSE);
            -- ListOfShift 30 shifts ------------------------------------------------------
-- For simplicity, we will assign shifts 1-10 to November, 11-20 December, 21-30 January
            INSERT INTO ListOfShift (
                dateOfShift,
                employeeId,
                bicycleId,
                substitutedId,
                routeId,
                startTime,
                endTime
            ) -- before today (3 with hasSubstituted TRUE)
            VALUES ('2025-11-01', 1, 1, 2, 1, '08:00', '12:00'),
                   -- Alice, substituted by Bob
                   ('2025-11-02', 2, 2, 3, 2, '09:00', '13:00'),
                   -- Bob, substituted by Charlie
                   ('2025-11-03', 3, 3, 4, 3, '07:00', '11:00'),
                   -- Charlie, substituted by Diana
                   ('2025-11-04', 4, 4, 5, 4, '08:00', '12:00'),
                   ('2025-11-05', 5, 5, 6, 5, '10:00', '14:00');
-- December shifts (all after today, no startTime/endTime)
            INSERT INTO ListOfShift (
                dateOfShift,
                employeeId,
                bicycleId,
                substitutedId,
                routeId
            )
            VALUES ('2025-12-01', 1, 1, 1, 1),
                   ('2025-12-02', 2, 2, 2, 2),
                   ('2025-12-03', 3, 3, 3, 3),
                   ('2025-12-04', 4, 4, 4, 4),
                   ('2025-12-05', 5, 5, 5, 5),
                   ('2025-12-06', 6, 6, 6, 6),
                   ('2025-12-07', 7, 7, 7, 7),
                   ('2025-12-08', 8, 8, 8, 8),
                   ('2025-12-09', 9, 9, 9, 9),
                   ('2025-12-10', 10, 10, 10, 10);
-- January shifts (all after today, no startTime/endTime)
            INSERT INTO ListOfShift (
                dateOfShift,
                employeeId,
                bicycleId,
                substitutedId,
                routeId
            )
            VALUES ('2026-01-01', 1, 1, 1, 1),
                   ('2026-01-02', 2, 2, 2, 2),
                   ('2026-01-03', 3, 3, 3, 3),
                   ('2026-01-04', 4, 4, 4, 4),
                   ('2026-01-05', 5, 5, 5, 5),
                   ('2026-01-06', 6, 6, 6, 6),
                   ('2026-01-07', 7, 7, 7, 7),
                   ('2026-01-08', 8, 8, 8, 8),
                   ('2026-01-09', 9, 9, 9, 9),
                   ('2026-01-10', 10, 10, 10, 10);
                   
CREATE TABLE OutboxEvents (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,

    AggregateType VARCHAR(100) NOT NULL,
    AggregateId INT NOT NULL,

    EventType VARCHAR(50) NOT NULL,
    -- Created | Updated | Deleted

    PayloadJson JSON NULL,
    -- optional, usually re-fetch from MySQL

    CreatedUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ProcessedUtc DATETIME NULL,

    RetryCount INT NOT NULL DEFAULT 0,
    LastError TEXT NULL,

    INDEX idx_outbox_unprocessed (ProcessedUtc),
    INDEX idx_outbox_aggregate (AggregateType, AggregateId)
);
