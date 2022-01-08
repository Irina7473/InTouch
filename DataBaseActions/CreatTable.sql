create table table_chats (
   id int not null /*auto_increment*/ primary key ,
   chatName varchar(100) not null unique ,
   avatar varbinary(8000) null
 );

 create table table_users
(
    id int not null /*auto_increment*/ primary key ,
    login varchar(100) not null unique ,
    password varchar(100) not null ,
    avatar varbinary(8000) null
);

create table table_contacts
(
    id     int not null /*auto_increment*/ primary key,
    chatId int not null,
    userId int not null,
    unique (chatId, userId)
);

create table table_messages
(
    id          int             not null /*auto_increment*/ primary key,
    messageType varchar(50)     not null,
    time        datetime        not null,
    senderId    int             not null,
    chatId      int             not null,
    content     varchar(8000) not null,
    status      tinyint(1)      null
);