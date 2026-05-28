-- ============================================
-- Database: EmployeeMaster
-- Created: 2026-05-27
-- Description: Employee Master Database
-- ============================================

-- Create Database
IF DB_ID('EmployeeMaster') IS NULL
    CREATE DATABASE EmployeeMaster;
USE EmployeeMaster;

-- ============================================
-- Table: Employee
-- ============================================
IF OBJECT_ID('dbo.Employee', 'U') IS NULL
CREATE TABLE Employee (
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        VARCHAR(100)    NOT NULL,
    SSN         VARCHAR(11)     NOT NULL UNIQUE,        -- Format: XXX-XX-XXXX
    DOB         DATE            NOT NULL,
    Address     VARCHAR(255)    NOT NULL,
    City        VARCHAR(100)    NOT NULL,
    State       VARCHAR(100)    NOT NULL,               -- Full state name
    Zip         VARCHAR(10)     NOT NULL,               -- Supports ZIP+4 format
    Phone       VARCHAR(15)     NOT NULL,
    JoinDate    DATE            NOT NULL,
    ExitDate    DATE            NULL,                   -- NULL means currently employed

    CONSTRAINT PK_Employee PRIMARY KEY (Id)
);

-- ============================================
-- Table: EmployeeSalary
-- ============================================
IF OBJECT_ID('dbo.EmployeeSalary', 'U') IS NULL
CREATE TABLE EmployeeSalary (
    Id          INT IDENTITY(1,1) NOT NULL,
    EmployeeId  INT             NOT NULL,
    FromDate    DATE            NOT NULL,
    ToDate      DATE            NULL,                   -- NULL means current salary
    Title       VARCHAR(100)    NOT NULL,
    Salary      DECIMAL(12, 2)  NOT NULL,

    CONSTRAINT PK_EmployeeSalary    PRIMARY KEY (Id),
    CONSTRAINT FK_Salary_Employee   FOREIGN KEY (EmployeeId)
                                    REFERENCES Employee(Id)
                                    ON DELETE CASCADE
);

-- ============================================
-- Indexes for performance
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Employee_SSN')
    CREATE INDEX IDX_Employee_SSN ON Employee(SSN);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Employee_Name')
    CREATE INDEX IDX_Employee_Name ON Employee(Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Salary_EmpId')
    CREATE INDEX IDX_Salary_EmpId ON EmployeeSalary(EmployeeId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Salary_FromDate')
    CREATE INDEX IDX_Salary_FromDate ON EmployeeSalary(FromDate);

-- ============================================
-- Sample Data: 100 Employees
-- Age range: 23-64 | ~85% active, ~15% exited
-- ============================================
INSERT INTO Employee (Name, SSN, DOB, Address, City, State, Zip, Phone, JoinDate, ExitDate)
VALUES
    ('Ashley Gonzalez', '106-48-4635', '1970-03-10', '3619 Hill St', 'New York', 'New York', '46170', '766-555-9376', '2017-06-24', NULL),
    ('Jerry Murphy', '464-29-3267', '1997-01-15', '4441 Valley Way', 'Milwaukee', 'Wisconsin', '86304', '455-555-4817', '2017-11-29', NULL),
    ('Ryan White', '851-19-6319', '1978-10-07', '8005 Elm St', 'Indianapolis', 'Indiana', '85527', '297-555-5921', '2005-11-03', NULL),
    ('Thomas Lewis', '442-54-8115', '1998-01-25', '6924 Maple Dr', 'Baltimore', 'Maryland', '39716', '965-555-1187', '2023-03-05', NULL),
    ('Joshua Wright', '426-79-3450', '1984-09-02', '5454 Meadow Ln', 'Washington', 'District of Columbia', '38940', '356-555-1827', '2020-02-10', NULL),
    ('Susan Diaz', '192-38-8076', '1965-10-17', '4847 Forest Dr', 'Phoenix', 'Arizona', '27326', '544-555-2352', '2015-09-04', NULL),
    ('Stephen Cruz', '508-74-8118', '1978-05-03', '2236 Cedar Ln', 'Indianapolis', 'Indiana', '83308', '402-555-8993', '2013-11-13', NULL),
    ('Betty Davis', '352-41-2173', '1964-12-17', '4462 Sunset Blvd', 'Austin', 'Texas', '99749', '789-555-2467', '2007-07-11', NULL),
    ('Gary Martin', '452-86-1396', '1981-04-11', '5094 Elm St', 'Jacksonville', 'Florida', '90769', '652-555-3389', '2014-11-18', NULL),
    ('Raymond Cox', '259-20-2841', '1991-11-12', '993 Park Ave', 'Nashville', 'Tennessee', '29356', '494-555-1225', '2017-07-25', NULL),
    ('Margaret King', '439-36-9814', '1975-08-01', '3571 Washington Blvd', 'Seattle', 'Washington', '78585', '968-555-5237', '2017-04-23', NULL),
    ('Jacob Scott', '779-77-1712', '2002-07-19', '4285 Elm St', 'Los Angeles', 'California', '47226', '405-555-5444', '2020-11-04', NULL),
    ('Linda Ross', '778-94-6901', '1979-11-09', '1867 Main St', 'San Jose', 'California', '95119', '595-555-4787', '2013-07-16', NULL),
    ('Dennis Carter', '711-78-2442', '1983-08-28', '3154 Washington Blvd', 'San Francisco', 'California', '70029', '774-555-1352', '2005-11-22', NULL),
    ('Jeffrey Ramirez', '862-30-1301', '1982-08-13', '5781 Valley Way', 'Jacksonville', 'Florida', '91950', '509-555-4869', '2016-03-30', NULL),
    ('Edward Harris', '130-81-5590', '1987-07-27', '3795 Cedar Ln', 'Austin', 'Texas', '61491', '872-555-3248', '2007-07-16', NULL),
    ('Matthew Powell', '332-63-4938', '1964-11-22', '9486 Cedar Ln', 'Columbus', 'Ohio', '50944', '819-555-6395', '2015-01-28', NULL),
    ('Jonathan Diaz', '800-35-3262', '1995-10-18', '5162 Meadow Ln', 'Albuquerque', 'New Mexico', '27727', '640-555-3378', '2021-02-25', '2022-07-13'),
    ('Elizabeth Bell', '221-33-6203', '1968-09-24', '2484 Main St', 'Louisville', 'Kentucky', '51771', '623-555-7737', '2011-10-22', NULL),
    ('James Lee', '255-34-2749', '1963-12-09', '2255 Elm St', 'New York', 'New York', '90213', '831-555-5372', '2018-08-02', NULL),
    ('Patrick White', '126-55-2302', '1989-04-22', '7908 Cedar Ln', 'San Francisco', 'California', '89260', '680-555-7126', '2011-06-07', '2022-12-11'),
    ('Joseph Sanders', '133-62-1245', '1993-04-08', '4521 Sunset Blvd', 'San Diego', 'California', '97993', '407-555-4351', '2012-09-30', NULL),
    ('James Peterson', '432-68-5043', '1964-08-23', '7105 Forest Dr', 'Louisville', 'Kentucky', '15482', '822-555-7232', '2021-02-25', NULL),
    ('Eric Taylor', '637-92-1630', '1967-10-18', '8630 Maple Dr', 'Charlotte', 'North Carolina', '69886', '243-555-5025', '2016-08-20', NULL),
    ('Jonathan Cooper', '883-90-1807', '1968-07-17', '3210 River Rd', 'Charlotte', 'North Carolina', '62171', '511-555-5684', '2015-05-08', NULL),
    ('Joshua Allen', '245-69-8400', '2000-12-17', '1581 Meadow Ln', 'Houston', 'Texas', '33562', '997-555-9952', '2020-06-23', NULL),
    ('Brian Green', '704-35-8449', '2002-03-15', '1577 Lake Dr', 'Austin', 'Texas', '95010', '928-555-3686', '2024-03-25', NULL),
    ('Jennifer Cooper', '187-77-7292', '1975-11-12', '8483 Lincoln Ave', 'Philadelphia', 'Pennsylvania', '59634', '763-555-7235', '2005-12-26', NULL),
    ('Kimberly Sanchez', '253-30-3369', '1967-11-28', '3030 Oak Ave', 'Columbus', 'Ohio', '50378', '516-555-8311', '2011-06-24', NULL),
    ('Anthony Moore', '737-42-1200', '1995-04-01', '7614 Highland Ave', 'Baltimore', 'Maryland', '75358', '756-555-1094', '2024-04-25', NULL),
    ('Christopher Brooks', '552-20-1107', '1973-08-14', '4109 Church St', 'Dallas', 'Texas', '91233', '843-555-4401', '2006-02-05', NULL),
    ('Timothy Perry', '974-14-5889', '1981-01-05', '5524 Pine Rd', 'Los Angeles', 'California', '24272', '626-555-3920', '2008-08-14', NULL),
    ('Dorothy Rivera', '675-32-8460', '1965-10-26', '9585 Valley Way', 'Albuquerque', 'New Mexico', '14697', '295-555-5903', '2012-03-29', NULL),
    ('Jennifer Jenkins', '663-18-5797', '1999-07-27', '7813 Cedar Ln', 'Houston', 'Texas', '45968', '537-555-9635', '2022-03-17', NULL),
    ('Timothy Richardson', '685-90-9666', '1964-01-04', '1536 Highland Ave', 'Las Vegas', 'Nevada', '89269', '923-555-5705', '2014-07-31', NULL),
    ('Brian Campbell', '462-26-9132', '1970-10-20', '8614 Highland Ave', 'Indianapolis', 'Indiana', '33580', '824-555-4812', '2020-11-22', NULL),
    ('Frank Moore', '740-25-4396', '1996-01-15', '6893 Sunset Blvd', 'Los Angeles', 'California', '53750', '558-555-4478', '2014-03-02', NULL),
    ('Jonathan Thomas', '407-74-6307', '1987-02-10', '200 Maple Dr', 'El Paso', 'Texas', '53684', '549-555-7488', '2021-05-11', NULL),
    ('Patrick Rodriguez', '953-53-8849', '1992-04-11', '7874 Oak Ave', 'Oklahoma City', 'Oklahoma', '27793', '693-555-3545', '2024-05-05', NULL),
    ('George Rogers', '933-62-1917', '1982-08-26', '8637 River Rd', 'Denver', 'Colorado', '98892', '523-555-9779', '2005-06-03', NULL),
    ('Elizabeth Long', '387-13-6556', '1981-02-05', '6247 Church St', 'Portland', 'Oregon', '31753', '784-555-7382', '2009-03-16', NULL),
    ('Frank Jenkins', '422-14-5448', '1984-12-28', '6880 Pine Rd', 'Indianapolis', 'Indiana', '41148', '580-555-8404', '2023-08-09', '2024-09-13'),
    ('Thomas Rogers', '903-14-4311', '1992-02-12', '1617 Spring St', 'San Francisco', 'California', '13268', '694-555-9192', '2013-07-28', NULL),
    ('Stephen Cook', '435-28-2609', '1970-05-25', '8556 Cedar Ln', 'San Antonio', 'Texas', '14791', '670-555-7350', '2021-01-03', '2022-02-24'),
    ('Kevin Cox', '708-21-6460', '2001-11-28', '7749 Oak Ave', 'Houston', 'Texas', '78233', '984-555-6119', '2020-01-13', NULL),
    ('Matthew Watson', '297-46-5532', '1990-06-07', '8820 Hill St', 'Louisville', 'Kentucky', '67817', '887-555-2927', '2014-07-19', NULL),
    ('Nancy Thomas', '626-23-4384', '2003-10-18', '8988 Valley Way', 'Seattle', 'Washington', '51744', '477-555-3319', '2023-05-21', NULL),
    ('Edward Ward', '653-66-1831', '1984-10-11', '2231 Main St', 'Indianapolis', 'Indiana', '98538', '251-555-2735', '2013-08-09', NULL),
    ('Lisa Perez', '706-90-8304', '1992-12-20', '5926 River Rd', 'Austin', 'Texas', '35971', '930-555-9740', '2012-09-23', '2023-08-31'),
    ('Jennifer Lewis', '199-68-6085', '1998-12-15', '7725 Highland Ave', 'San Diego', 'California', '39333', '347-555-5227', '2021-01-11', NULL),
    ('Kimberly Wood', '599-13-2070', '1982-06-15', '7887 Maple Dr', 'Jacksonville', 'Florida', '72737', '672-555-3798', '2011-05-02', NULL),
    ('Ryan Morgan', '515-23-6339', '1995-09-04', '9211 Pine Rd', 'Memphis', 'Tennessee', '49599', '745-555-8552', '2022-08-31', NULL),
    ('Alexander Scott', '593-59-2082', '1988-09-06', '5682 Washington Blvd', 'Portland', 'Oregon', '87459', '641-555-8132', '2019-05-27', NULL),
    ('Kenneth Young', '785-19-8972', '1988-03-22', '1973 Broadway', 'Nashville', 'Tennessee', '70674', '533-555-2599', '2024-08-28', NULL),
    ('Joseph Lee', '801-86-7208', '1999-10-04', '5464 Highland Ave', 'Oklahoma City', 'Oklahoma', '96529', '262-555-2483', '2020-03-13', NULL),
    ('Mark Gonzalez', '250-94-3444', '1963-01-25', '7521 Church St', 'San Jose', 'California', '61115', '830-555-8326', '2009-05-22', NULL),
    ('Margaret Roberts', '795-48-8122', '1984-10-26', '5740 Pine Rd', 'Denver', 'Colorado', '11064', '826-555-9362', '2014-01-25', NULL),
    ('Sarah Perry', '154-10-6885', '2000-06-18', '1897 River Rd', 'Baltimore', 'Maryland', '14087', '440-555-8073', '2022-11-18', NULL),
    ('Anthony Watson', '658-37-3841', '1978-07-14', '7880 Cedar Ln', 'San Antonio', 'Texas', '82800', '593-555-6902', '2023-11-03', NULL),
    ('Ronald Smith', '173-92-1257', '1995-03-01', '2459 Highland Ave', 'El Paso', 'Texas', '67243', '339-555-2387', '2016-03-29', NULL),
    ('Donald Jackson', '535-89-3945', '1974-12-20', '7544 Meadow Ln', 'Columbus', 'Ohio', '82358', '660-555-7303', '2012-12-20', NULL),
    ('Brandon Richardson', '967-40-9220', '2003-10-16', '4511 Sunset Blvd', 'Albuquerque', 'New Mexico', '81587', '790-555-7012', '2021-12-23', '2025-08-27'),
    ('Jessica Carter', '593-98-2311', '1980-02-19', '2427 Meadow Ln', 'Baltimore', 'Maryland', '46624', '676-555-9469', '2006-10-03', NULL),
    ('William Hernandez', '859-94-2279', '1988-08-21', '8934 Broadway', 'San Jose', 'California', '90352', '254-555-6628', '2018-08-20', NULL),
    ('William Jones', '851-52-7897', '1971-08-19', '5882 River Rd', 'San Jose', 'California', '31955', '236-555-5981', '2021-04-22', NULL),
    ('Joshua Hughes', '273-83-1845', '1963-08-19', '4880 Lincoln Ave', 'New York', 'New York', '47593', '696-555-2351', '2007-11-05', NULL),
    ('Steven Ward', '730-45-5509', '1986-05-05', '4309 Main St', 'Las Vegas', 'Nevada', '24621', '818-555-3860', '2018-04-10', NULL),
    ('Jacob Rivera', '901-99-4869', '1980-03-09', '8044 Elm St', 'Memphis', 'Tennessee', '79125', '233-555-6617', '2006-11-23', '2020-09-28'),
    ('Benjamin Gonzalez', '522-59-4898', '1994-08-25', '7311 Elm St', 'San Antonio', 'Texas', '85727', '951-555-6653', '2013-11-13', NULL),
    ('Sarah Carter', '575-61-8802', '1996-01-06', '5037 Highland Ave', 'Las Vegas', 'Nevada', '79798', '439-555-7222', '2021-09-05', NULL),
    ('Betty Sanchez', '545-11-5731', '1994-09-28', '7453 Forest Dr', 'El Paso', 'Texas', '51377', '742-555-1672', '2014-12-04', '2022-07-25'),
    ('Lisa Robinson', '978-44-6488', '1976-10-21', '4529 Lake Dr', 'Memphis', 'Tennessee', '22881', '461-555-7408', '2015-05-06', NULL),
    ('James James', '445-96-3947', '1966-04-17', '5232 Church St', 'New York', 'New York', '98185', '208-555-2113', '2005-04-08', NULL),
    ('Betty Nelson', '842-46-5127', '1975-06-08', '3263 Oak Ave', 'New York', 'New York', '91185', '294-555-1931', '2018-04-30', NULL),
    ('Anthony Nelson', '825-25-2415', '1965-11-04', '2228 Forest Dr', 'El Paso', 'Texas', '94922', '779-555-4027', '2007-06-28', NULL),
    ('Dorothy Perez', '754-68-5528', '1974-11-03', '7095 Hill St', 'Dallas', 'Texas', '31229', '999-555-2212', '2011-03-22', NULL),
    ('Mark Torres', '932-43-3915', '1982-01-09', '7318 Pine Rd', 'Indianapolis', 'Indiana', '36302', '824-555-7432', '2009-01-01', NULL),
    ('Christopher Smith', '728-92-2740', '1969-07-07', '7334 Lake Dr', 'Houston', 'Texas', '14569', '274-555-2153', '2017-10-25', NULL),
    ('Gregory Cruz', '768-91-1014', '2002-08-05', '719 Forest Dr', 'Dallas', 'Texas', '29871', '916-555-6403', '2022-10-15', NULL),
    ('Benjamin Jackson', '513-18-9849', '1972-08-14', '3800 Main St', 'Fort Worth', 'Texas', '73185', '324-555-9770', '2013-01-19', NULL),
    ('Thomas Brown', '192-56-9759', '2000-11-21', '5671 Pine Rd', 'Portland', 'Oregon', '70245', '978-555-5750', '2021-03-01', NULL),
    ('Joshua Nguyen', '499-23-6886', '1991-05-23', '8070 Pine Rd', 'Milwaukee', 'Wisconsin', '92941', '648-555-4104', '2012-07-12', NULL),
    ('John King', '498-18-9856', '1966-04-16', '5141 Highland Ave', 'Seattle', 'Washington', '68891', '216-555-1606', '2012-08-11', NULL),
    ('Jessica Taylor', '316-93-9570', '2001-07-04', '7609 Church St', 'Phoenix', 'Arizona', '85936', '438-555-2180', '2019-09-17', NULL),
    ('Paul Foster', '146-41-3138', '2002-08-20', '335 Elm St', 'Oklahoma City', 'Oklahoma', '16548', '514-555-2594', '2023-03-12', NULL),
    ('William Reed', '609-86-4613', '1999-02-04', '1133 Washington Blvd', 'Columbus', 'Ohio', '54216', '955-555-7586', '2020-09-24', NULL),
    ('Linda Brown', '956-37-1746', '1969-03-11', '9817 Elm St', 'Austin', 'Texas', '18286', '323-555-5487', '2008-03-03', NULL),
    ('Donald Nelson', '549-41-1666', '1980-09-06', '2285 Maple Dr', 'Memphis', 'Tennessee', '92863', '827-555-7162', '2022-06-11', NULL),
    ('Brandon Murphy', '283-21-5648', '1998-01-01', '6919 Highland Ave', 'Washington', 'District of Columbia', '26632', '411-555-3344', '2023-03-05', NULL),
    ('Daniel Williams', '808-14-3949', '1988-03-20', '8290 Lincoln Ave', 'Chicago', 'Illinois', '92207', '508-555-6699', '2015-08-10', NULL),
    ('Ryan Jenkins', '733-56-9842', '1996-09-04', '8415 Park Ave', 'Charlotte', 'North Carolina', '92111', '875-555-4409', '2017-10-10', NULL),
    ('Andrew Powell', '585-73-3888', '1994-11-28', '2717 Oak Ave', 'Los Angeles', 'California', '94523', '572-555-5670', '2017-08-10', NULL),
    ('Emily Miller', '858-16-9804', '1974-01-04', '2981 Elm St', 'Memphis', 'Tennessee', '13809', '696-555-9436', '2007-08-09', NULL),
    ('Andrew Allen', '433-63-4256', '1975-10-19', '5436 Park Ave', 'Louisville', 'Kentucky', '24435', '533-555-4731', '2007-03-15', NULL),
    ('Barbara White', '175-70-8936', '1991-07-08', '990 Sunset Blvd', 'Phoenix', 'Arizona', '38333', '824-555-8454', '2013-07-05', NULL),
    ('Emily Green', '376-35-6503', '1991-11-12', '2184 Oak Ave', 'Washington', 'District of Columbia', '84536', '261-555-5597', '2012-12-04', NULL),
    ('Daniel Gonzalez', '451-38-5835', '1977-03-25', '9989 Elm St', 'Memphis', 'Tennessee', '93623', '826-555-3784', '2017-12-19', '2021-01-12'),
    ('Christopher Bell', '321-85-8757', '1970-08-28', '7681 Lake Dr', 'New York', 'New York', '66113', '953-555-5041', '2010-07-24', NULL),
    ('Timothy King', '105-63-1724', '1966-03-19', '756 Highland Ave', 'Las Vegas', 'Nevada', '76088', '234-555-7974', '2016-11-29', NULL),
    ('Kimberly Martinez', '346-20-1203', '1997-08-16', '6762 Washington Blvd', 'Phoenix', 'Arizona', '56976', '502-555-4916', '2022-11-19', '2023-06-19');

-- ============================================
-- Sample Data: EmployeeSalary
-- 1 salary record per employee (ToDate = NULL = current)
-- ============================================
INSERT INTO EmployeeSalary (EmployeeId, FromDate, ToDate, Title, Salary)
VALUES
    (1, '2017-06-24', NULL, 'Accountant', 62500.00),
    (2, '2017-11-29', NULL, 'Senior Developer', 99500.00),
    (3, '2005-11-03', NULL, 'Accountant', 66500.00),
    (4, '2023-03-05', NULL, 'HR Coordinator', 51000.00),
    (5, '2020-02-10', NULL, 'HR Coordinator', 57500.00),
    (6, '2015-09-04', NULL, 'Software Engineer', 82000.00),
    (7, '2013-11-13', NULL, 'Project Manager', 96000.00),
    (8, '2007-07-11', NULL, 'HR Manager', 83000.00),
    (9, '2014-11-18', NULL, 'Systems Administrator', 75000.00),
    (10, '2017-07-25', NULL, 'Lead Engineer', 115000.00),
    (11, '2017-04-23', NULL, 'DevOps Engineer', 102500.00),
    (12, '2020-11-04', NULL, 'Junior Developer', 48000.00),
    (13, '2013-07-16', NULL, 'Customer Success Manager', 71500.00),
    (14, '2005-11-22', NULL, 'Sales Representative', 58000.00),
    (15, '2016-03-30', NULL, 'Software Engineer', 83500.00),
    (16, '2007-07-16', NULL, 'Sales Representative', 64000.00),
    (17, '2015-01-28', NULL, 'Project Manager', 97500.00),
    (18, '2021-02-25', NULL, 'IT Specialist', 58000.00),
    (19, '2011-10-22', NULL, 'Technical Writer', 73500.00),
    (20, '2018-08-02', NULL, 'Product Manager', 110000.00),
    (21, '2011-06-07', NULL, 'Marketing Manager', 84500.00),
    (22, '2012-09-30', NULL, 'Marketing Specialist', 69000.00),
    (23, '2021-02-25', NULL, 'Finance Manager', 109000.00),
    (24, '2016-08-20', NULL, 'DevOps Engineer', 100500.00),
    (25, '2015-05-08', NULL, 'Data Scientist', 103000.00),
    (26, '2020-06-23', NULL, 'IT Specialist', 64500.00),
    (27, '2024-03-25', NULL, 'Senior Developer', 92000.00),
    (28, '2005-12-26', NULL, 'Technical Writer', 65500.00),
    (29, '2011-06-24', NULL, 'Lead Engineer', 124500.00),
    (30, '2024-04-25', NULL, 'Accountant', 71000.00),
    (31, '2006-02-05', NULL, 'Software Engineer', 83500.00),
    (32, '2008-08-14', NULL, 'Finance Manager', 108000.00),
    (33, '2012-03-29', NULL, 'HR Coordinator', 63500.00),
    (34, '2022-03-17', NULL, 'Customer Success Manager', 73500.00),
    (35, '2014-07-31', NULL, 'IT Specialist', 61500.00),
    (36, '2020-11-22', NULL, 'Project Manager', 87000.00),
    (37, '2014-03-02', NULL, 'Customer Success Manager', 74500.00),
    (38, '2021-05-11', NULL, 'Junior Developer', 60500.00),
    (39, '2024-05-05', NULL, 'Network Engineer', 87000.00),
    (40, '2005-06-03', NULL, 'Operations Manager', 94500.00),
    (41, '2009-03-16', NULL, 'Network Engineer', 91000.00),
    (42, '2023-08-09', NULL, 'Sales Representative', 58000.00),
    (43, '2013-07-28', NULL, 'Technical Writer', 68000.00),
    (44, '2021-01-03', NULL, 'Junior Developer', 52000.00),
    (45, '2020-01-13', NULL, 'Data Scientist', 103500.00),
    (46, '2014-07-19', NULL, 'Data Analyst', 69000.00),
    (47, '2023-05-21', NULL, 'Lead Analyst', 84000.00),
    (48, '2013-08-09', NULL, 'HR Manager', 83500.00),
    (49, '2012-09-23', NULL, 'Accountant', 62000.00),
    (50, '2021-01-11', NULL, 'Customer Success Manager', 81000.00),
    (51, '2011-05-02', NULL, 'Finance Manager', 106500.00),
    (52, '2022-08-31', NULL, 'Product Manager', 110000.00),
    (53, '2019-05-27', NULL, 'Sales Manager', 96500.00),
    (54, '2024-08-28', NULL, 'Data Scientist', 99500.00),
    (55, '2020-03-13', NULL, 'Security Analyst', 92500.00),
    (56, '2009-05-22', NULL, 'Security Analyst', 88000.00),
    (57, '2014-01-25', NULL, 'Network Engineer', 83000.00),
    (58, '2022-11-18', NULL, 'Customer Success Manager', 76500.00),
    (59, '2023-11-03', NULL, 'Data Scientist', 107500.00),
    (60, '2016-03-29', NULL, 'HR Manager', 91000.00),
    (61, '2012-12-20', NULL, 'HR Manager', 87000.00),
    (62, '2021-12-23', NULL, 'Marketing Manager', 93500.00),
    (63, '2006-10-03', NULL, 'Finance Manager', 98500.00),
    (64, '2018-08-20', NULL, 'Marketing Manager', 89500.00),
    (65, '2021-04-22', NULL, 'Data Analyst', 68000.00),
    (66, '2007-11-05', NULL, 'Network Engineer', 91000.00),
    (67, '2018-04-10', NULL, 'Operations Manager', 96500.00),
    (68, '2006-11-23', NULL, 'Technical Writer', 73500.00),
    (69, '2013-11-13', NULL, 'QA Engineer', 80500.00),
    (70, '2021-09-05', NULL, 'Marketing Manager', 86500.00),
    (71, '2014-12-04', NULL, 'IT Manager', 105000.00),
    (72, '2015-05-06', NULL, 'DevOps Engineer', 104000.00),
    (73, '2005-04-08', NULL, 'Sales Representative', 54500.00),
    (74, '2018-04-30', NULL, 'Systems Administrator', 74500.00),
    (75, '2007-06-28', NULL, 'Data Analyst', 72000.00),
    (76, '2011-03-22', NULL, 'HR Coordinator', 56500.00),
    (77, '2009-01-01', NULL, 'Lead Engineer', 112500.00),
    (78, '2017-10-25', NULL, 'QA Engineer', 74500.00),
    (79, '2022-10-15', NULL, 'Sales Representative', 64500.00),
    (80, '2013-01-19', NULL, 'Data Scientist', 99500.00),
    (81, '2021-03-01', NULL, 'Software Engineer', 79500.00),
    (82, '2012-07-12', NULL, 'Junior Developer', 59000.00),
    (83, '2012-08-11', NULL, 'UX Designer', 87000.00),
    (84, '2019-09-17', NULL, 'Marketing Manager', 92500.00),
    (85, '2023-03-12', NULL, 'Systems Administrator', 80500.00),
    (86, '2020-09-24', NULL, 'Operations Manager', 98500.00),
    (87, '2008-03-03', NULL, 'Lead Analyst', 94500.00),
    (88, '2022-06-11', NULL, 'Security Analyst', 91000.00),
    (89, '2023-03-05', NULL, 'Marketing Specialist', 62000.00),
    (90, '2015-08-10', NULL, 'Product Manager', 103000.00),
    (91, '2017-10-10', NULL, 'Lead Engineer', 123000.00),
    (92, '2017-08-10', NULL, 'IT Specialist', 67000.00),
    (93, '2007-08-09', NULL, 'Product Manager', 112000.00),
    (94, '2007-03-15', NULL, 'HR Manager', 91500.00),
    (95, '2013-07-05', NULL, 'Business Analyst', 70500.00),
    (96, '2012-12-04', NULL, 'Marketing Specialist', 62500.00),
    (97, '2017-12-19', NULL, 'Senior Developer', 96500.00),
    (98, '2010-07-24', NULL, 'Accountant', 63500.00),
    (99, '2016-11-29', NULL, 'UX Designer', 88000.00),
    (100, '2022-11-19', NULL, 'Marketing Manager', 93000.00);
