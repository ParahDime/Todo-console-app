--Table for user information

CREATE TABLE IF NOT EXISTS Users (
	Username TEXT NOT NULL,
	UserId INTEGER PRIMARY KEY,
	Salt TEXT NOT NULL,
	Passwrd_Hash REAL NOT NULL
	--created_at INTEGER not null
);

--Table for actions on the ToDo list
CREATE TABLE IF NOT EXISTS ActionsToDo (
	Id INTEGER PRIMARY KEY,
	UserId INTEGER NOT NULL,
	Title TEXT NOT NULL,
	Description TEXT NULL,
	Is_Complete INTEGER NOT NULL DEFAULT 0,
	Time_Create INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP, --could be text?

	FOREIGN KEY (UserId) REFERENCES Users(UserId) --end of table foreign key
);

CREATE TABLE IF NOT EXISTS Expenses (
	Id INTEGER PRIMARY KEY,
	UserId INTEGER NOT NULL,
	Amount INTEGER NOT NULL,
	Frequency TEXT DEFAULT 3,
	Title TEXT NOT NULL,

	FOREIGN KEY (UserId) REFERENCES Users(UserId)
);