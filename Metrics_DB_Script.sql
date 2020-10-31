CREATE TABLE CUSTOMER (
    USERNAME VARCHAR2(64) NOT NULL PRIMARY KEY,
    CustomerID int NOT NULL,
    Email varchar(255) NOT NULL,
    FirstName varchar(255) NOT NULL,
    LastName varchar(255) NOT NULL,
    Address varchar(255) NOT NULL,
    Zip varchar(255) NOT NULL,
    State varchar(2) NOT NULL,
    PaymentMethod varchar(255) NOT NULL,
	PasswordHash varchar2(128)
);

CREATE TABLE INVENTORY (
    InventoryID int NOT NULL PRIMARY KEY,
    Name varchar(255) NOT NULL,
    Color varchar(255) NOT NULL,
    BikeSize varchar(255) NOT NULL,
    Model varchar(255) NOT NULL,
    TireSize varchar(255) NOT NULL,
    SeatType varchar(2) NOT NULL,
    FrameType varchar(255) NOT NULL
);

CREATE Table CartLog (
    LOGID           NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    USERNAME        VARCHAR2(64)  NOT NULL,
    EVENTTIMESTAMP  DATE,
    DESCRIPTION     VARCHAR2(256),
    ITEMTABLE       VARCHAR2(32),
    ITEMID          NUMBER(*,0),
    QUANTITY        NUMBER(6),
        CONSTRAINT cartlog_pkey PRIMARY KEY (LOGID),
        CONSTRAINT cartlog_customer_fkey FOREIGN KEY (USERNAME) REFERENCES metric.Customer (USERNAME)
    );
    
CREATE TABLE USERLOGINLOG (
    LOGID NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    USERNAME VARCHAR2(64) REFERENCES Customer(USERNAME),
    EVENTTIMESTAMP DATE,
    DESCRIPTION VARCHAR2(256),
    ATTEMPT NUMBER(2),
        CONSTRAINT userloginlog_pkey PRIMARY KEY (LOGID)
    );
    
    
CREATE Table WISHLISTLOG (
    LOGID           NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    EVENTTIMESTAMP  DATE,
    DESCRIPTION     VARCHAR2(256),
    ITEMTABLE       VARCHAR2(32),
    ITEMID          NUMBER(*,0),
    LISTID          NUMBER(*,0) REFERENCES WISHLIST(LISTID),
        CONSTRAINT wishlistlog_pkey PRIMARY KEY (LOGID)
    );
    
CREATE TABLE SEARCHLOG (
    LOGID NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    USERNAME VARCHAR2(64) REFERENCES Customer(USERNAME),
    EVENTTIMESTAMP DATE,
    SEARCHTEXT VARCHAR2(32),
        CONSTRAINT searchlog_pkey PRIMARY KEY (LOGID)
    );
    
CREATE TABLE ITEMVIEWLOG (
    LOGID NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    ITEMID NUMBER (*, 0),
    ITEMTABLE VARCHAR2(32),
    USERNAME VARCHAR2(64) REFERENCES Customer(USERNAME),
    EVENTTIMESTAMP DATE,
        CONSTRAINT itemviewlog_pkey PRIMARY KEY (LOGID)
    );
    
CREATE TABLE WISHLIST (
    LISTID  NUMBER(*,0) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    LISTNAME VARCHAR2(32),
    USERNAME VARCHAR2(64) REFERENCES Customer(USERNAME),
        CONSTRAINT wishlist_pkey PRIMARY KEY (LISTID)
    );
    
CREATE TABLE WISHLISTITEM (
    ITEMID NUMBER (*, 0),
    ITEMTABLE VARCHAR2(32),
    LISTID NUMBER(*,0) REFERENCES WISHLIST(LISTID),
        CONSTRAINT wishlistitem_pkey PRIMARY KEY (ITEMID, ITEMTABLE)
    );
	
CREATE Table CheckoutLog (
    LOGID           NUMBER(15) GENERATED BY DEFAULT ON NULL AS IDENTITY,
    USERNAME        VARCHAR2(64)  NOT NULL,
    EVENTTIMESTAMP  DATE,
    PURCHASEID      NUMBER(*,0),
        CONSTRAINT checkoutlog_pkey PRIMARY KEY (LOGID),
        CONSTRAINT checkoutlog_customer_fkey FOREIGN KEY (USERNAME) REFERENCES metric.Customer (USERNAME)
    );
CREATE Table CartItem (
    USERNAME        VARCHAR2(64)  NOT NULL,
    ITEMTABLE       VARCHAR2(32),
    ITEMID          NUMBER(*,0),
    ITEMPRICE       NUMBER(38,4),
    QUANTITY        NUMBER(*,0),
        CONSTRAINT cartitem_pkey PRIMARY KEY (USERNAME, ITEMID, ITEMTABLE),
        CONSTRAINT cartitem_customer_fkey FOREIGN KEY (USERNAME) REFERENCES metric.Customer (USERNAME)
    );