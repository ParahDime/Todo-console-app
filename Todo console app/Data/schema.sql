--Table for user information
CREATE TABLE Users (
	username TEXT NOT NULL,
	userID INTEGER NOT NULL UNIQUE PRIMARY KEY,
	passwrd_hash REAL NOT NULL
	--created_at INTEGER not null
);

--Table for actions on the ToDo list
CREATE TABLE ActionsToDo (
	id INTEGER NOT NULL,
	userID INTEGER NOT NULL,
	title TEXT NOT NULL,
	description TEXT NULL,
	is_complete INTEGER NOT NULL DEFAULT 0,
	time_create INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP --could be text?

	FOREIGN KEY (userID) REFERENCES Users(userID) --end of table foreign key
);

CREATE TABLE Expenses (
	id INTEGER UNIQUE,
	userID INTEGER NOT NULL,
	amount INTEGER NOT NULL,
	frequency TEXT DEFAULT 0,
	title TEXT NOT NULL UNIQUE

	FOREIGN KEY (userID) REFERENCES Users(userID)
);