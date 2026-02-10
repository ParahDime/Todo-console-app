--Populating data for users
INSERT INTO Users (UserID, passwrd_hash)	
VALUES
('Roy', 'fehdhd'),
('Mark', 'password123');

--Populating data for TodoList
INSERT INTO TodoList (id, userID, title, description, is_complete, time_create)
VALUES
(1,'Roy', 'Make bed', 'Make the bed now', 0, 0),
(2, 'Roy', 'Drink water', NULL, 0, 0),
(3, 'Mark', 'Drink water', NULL, 0, 0);

--Expenses (to look into frequency amount)
INSERT INTO Expenses (id, userID, amount, frequency, title)
VALUES
(1, 'Roy', 50, 'Weekly', 'Cash ISA'),
(2, 'Roy', 100, 'Monthly', 'Savings'),
(3, 'Mark', 40, 'Weekly', 'Groceries')
;

