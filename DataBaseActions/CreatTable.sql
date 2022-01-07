create table table_chats 
(
   id       int             not null primary key ,
   chatName varchar(100)    not null,
   avatar   varbinary(8000) null
);

 create table table_users
(
    id          int             not null primary key,
    login       varchar(100)    not null,
    password    varchar(100)    not null,
    avatar      varbinary(8000) null
);

create table table_contacts
(
    id     int not null primary key,
    chatId int not null,
    userId int not null
);

create table table_messages
(
    id          int             not null primary key,
    messageType varchar(50)     not null,
    time        datetime        not null,
    senderId    int             not null,
    chatId      int             not null,
    content     varbinary(8000) not null,
    status      tinyint(1)      null
);