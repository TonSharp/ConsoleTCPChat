# Simple Console TCP Chat

Simple .NET Core application that allows you to communicate with other users, via the console and the **TCP** protocol. You can create a room with **any port**, or connect to an existing one.

TCP Chat supports **color nicknames** (currently only available colors in the console, in the future all RGB colors), **commands** for interacting with the server and clients (in development). Many other cool features will appear in the future)

TCP Chat is a completely anonymous application, all you need is to enter any nickname and color.

## Client Side

Clients connect to the server and communicate with each other only through the server. They don't know anything about each other, which allows for greater anonymity. The server gives each client its unique **ID** address once when connecting, which is used later for identification (in the future for decrypting messages). 

Attackers will not be able to decrypt these messages, as they will not have a unique address.

## Server Side

The server has information about each client, and when connected, it receives the client's userdata. Other clients receive public **UserData** from the server only when this client sends a message, it consists of a **UserName** and a **Color**. At any other time, the server does not send data to other clients, which provides increased security and anonymity.

## Message system

All requests from clients and the server are sent in the form of so-called **Messages**. The first four bytes consist of an integer called a **PostCode**, it shows the content of the message.

Available **PostCodes**:

|PostCodes|Description|
|--|--|
|1-4|Usual messages from client or server|
|5| Request from client for **ID**|
|6|Request from client for **UserData**|
|7|Reserved|
|8|Client connecting message|
|9|Client disconnecting message|
|10|Server disconnecting message|
|11|Sending client **ID** from Server|
|12|Sending client **UserData** from Server|

In the full version of Message, the **PostCode** is followed by the public **UserData**, followed by the message **Text**. Depending on the **PostCode**, the Message may not contain **UserData** or the message **Text**.

## Commands

Here is a list of currently available commands:

|Command|Description|
|--|--|
|**/join** [adress]:[port]|Connect to the server|
|/join [adress] [port]||
|**/connect** [adress]:[port]|The same as /join|
|/connect [adress] [port]||
|**/room** [port]|Start server|
|**/create** [port]|The same as /room|
|**/disconnect**|Disconnect from server or stop server|
|**/dconnect**|The same as /disconnect|
|**/clear**|Clears screen|
|**/clr**|The same as /clear|
